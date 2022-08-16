#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimatedMeshToAsset
{
    private const int BoneMatrixRowCount = 3;
    private static int TargetFrameRate => BattleConstant.FrameInterval;

    [MenuItem("AnimatedMeshRendererGenerator/MeshToAsset")]
    private static void Generate()
    {
        var targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            EditorUtility.DisplayDialog("Warning", "Selected object type is not gameobject.", "OK");
            return;
        }

        var skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderers.Any() || skinnedMeshRenderers.Count() != 1)
        {
            EditorUtility.DisplayDialog("Warning", "Selected object does not have one skinnedMeshRenderer.", "OK");
            return;
        }

        var animator = targetObject.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Warning", "Selected object does not have Animator.", "OK");
            return;
        }

        var selectionPath = "Assets/Resources";
        var skinnedMeshRenderer = skinnedMeshRenderers.First();
        var clips = animator.runtimeAnimatorController.animationClips;

        Directory.CreateDirectory(Path.Combine(selectionPath, "AnimatedMesh"));

        var animationTexture = GenerateAnimationTexture(targetObject, clips, skinnedMeshRenderer);
        AssetDatabase.CreateAsset(animationTexture, string.Format("{0}/AnimatedMesh/{1}_AnimationTexture.asset", selectionPath, targetObject.name));

        var mesh = GenerateUvBoneWeightedMesh(skinnedMeshRenderer);
        AssetDatabase.CreateAsset(mesh, string.Format("{0}/AnimatedMesh/{1}_Mesh.asset", selectionPath, targetObject.name));

        var materials = GenerateMaterial(skinnedMeshRenderer, animationTexture, skinnedMeshRenderer.bones.Length);
        for (int i = 0; i < materials.Length; i++)
        {
            AssetDatabase.CreateAsset(materials[i], string.Format("{0}/AnimatedMesh/{1}_Material_{2}.asset", selectionPath, targetObject.name, i));
        }

        var go = GenerateMeshRendererObject(targetObject, mesh, materials, clips);
        PrefabUtility.SaveAsPrefabAsset(go, string.Format("{0}/AnimatedMesh/{1}.prefab", selectionPath, targetObject.name));

        Object.DestroyImmediate(go);
    }

    private static Mesh GenerateUvBoneWeightedMesh(SkinnedMeshRenderer smr)
    {
        var mesh = Object.Instantiate(smr.sharedMesh);

        var boneSets = smr.sharedMesh.boneWeights;
        var boneIndexes = boneSets.Select(x => new Vector4(x.boneIndex0, x.boneIndex1, x.boneIndex2, x.boneIndex3)).ToList();
        var boneWeights = boneSets.Select(x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3)).ToList();

        mesh.SetUVs(2, boneIndexes);
        mesh.SetUVs(3, boneWeights);

        return mesh;
    }

    private static Texture GenerateAnimationTexture(GameObject targetObject, IEnumerable<AnimationClip> clips, SkinnedMeshRenderer smr)
    {
        var textureBoundary = GetCalculatedTextureBoundary(clips, smr.bones.Count());

        var texture = new Texture2D((int)textureBoundary.x, (int)textureBoundary.y, TextureFormat.RGBAHalf, false, true);
        var pixels = texture.GetPixels();
        var pixelIndex = 0;

        //Setup 0 to bindPoses
        foreach (var boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
        {
            pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
            pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
            pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);
        }

        foreach (var clip in clips)
        {
            var totalFrames = (int)(clip.length * TargetFrameRate);
            foreach (var frame in Enumerable.Range(0, totalFrames))
            {
                clip.SampleAnimation(targetObject, (float)frame / TargetFrameRate);

                foreach (var boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
                {
                    pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
                    pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
                    pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;
    }

    private static Vector2 GetCalculatedTextureBoundary(IEnumerable<AnimationClip> clips, int boneLength)
    {
        var boneMatrixCount = BoneMatrixRowCount * boneLength;

        var totalPixels = clips.Aggregate(boneMatrixCount, (pixels, currentClip) => pixels + boneMatrixCount * (int)(currentClip.length * TargetFrameRate));

        var textureWidth = 1;
        var textureHeight = 1;

        while (textureWidth * textureHeight < totalPixels)
        {
            if (textureWidth <= textureHeight)
            {
                textureWidth *= 2;
            }
            else
            {
                textureHeight *= 2;
            }
        }

        return new Vector2(textureWidth, textureHeight);
    }

    private static Material[] GenerateMaterial(SkinnedMeshRenderer smr, Texture texture, int boneLength)
    {
        List<Material> materials = new List<Material>();
        for (int i = 0; i < smr.sharedMaterials.Length; i++)
        {
            var material = Object.Instantiate(smr.sharedMaterials[i]);
            material.shader = Shader.Find("AnimationGpuInstancing/Standard");
            material.SetTexture("_AnimTex", texture);
            material.SetInt("_PixelCountPerFrame", BoneMatrixRowCount * boneLength);
            material.enableInstancing = true;
            materials.Add(material);
        }

        return materials.ToArray();
    }

    private static GameObject GenerateMeshRendererObject(GameObject targetObject, Mesh mesh, Material[] materials, IEnumerable<AnimationClip> clips)
    {
        var go = new GameObject();
        go.name = targetObject.name;

        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.materials = materials;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.lightProbeUsage = LightProbeUsage.Off;

        var animtedMeshRenderer = go.AddComponent<AnimatedMeshAnimator>();
        var properyBlockController = go.AddComponent<MaterialPropertyBlockController>();
        var frameInformations = new List<AnimationFrameInfo>();

        var currentClipFrames = 0;
        foreach (var clip in clips)
        {
            var frameCount = (int)(clip.length * TargetFrameRate);
            var startFrame = currentClipFrames + 1;
            var endFrame = startFrame + frameCount - 1;

            frameInformations.Add(new AnimationFrameInfo(clip.name, startFrame, endFrame, frameCount));
            currentClipFrames = endFrame;
        }

        animtedMeshRenderer.Setup(frameInformations, properyBlockController);

        return go;
    }
}
#endif