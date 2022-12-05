using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSkinedModel : MonoBehaviour
{
    public Mesh shadowMesh;
    private List<Renderer> _shadowRenderers = new List<Renderer>();

    public bool Matrix4x4Equal(Matrix4x4 m1, Matrix4x4 m2)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector4 row1 = m1.GetRow(i);
            Vector4 row2 = m2.GetRow(i);

            if (Mathf.Abs(row1.x - row2.x) > 0.001)
                return false;

            if (Mathf.Abs(row1.y - row2.y) > 0.001)
                return false;

            if (Mathf.Abs(row1.z - row2.z) > 0.001)
                return false;

            if (Mathf.Abs(row1.w - row2.w) > 0.001)
                return false;
        }

        return true;
    }

    public void SkinMeshRendersToPlaneShadow()
    {
        Transform root = transform.Find("root");
        if (root != null)
        {
            Shader planeShadowShader = Resources.Load("TA/PlanarShadow", typeof(Shader)) as Shader;
            SkinnedMeshRenderer[] skinnedMeshRenderers = root.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
            {
                List<Transform> boneList = new List<Transform>();
                List<Matrix4x4> bindPosList = new List<Matrix4x4>();
                List<Vector3> vertexList = new List<Vector3>();
                List<int> indexList = new List<int>();
                List<BoneWeight> boneWeightList = new List<BoneWeight>();
                for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                {
                    if (skinnedMeshRenderers[i] == null)
                    {
                        continue;
                    }
                    if (skinnedMeshRenderers[i].transform.gameObject.name.Equals("shadow"))
                    {
                        GameObject.DestroyImmediate(skinnedMeshRenderers[i].transform.gameObject);
                        continue;
                    }
                    SkinnedMeshRenderer renderer = skinnedMeshRenderers[i];
                    Transform[] bones = renderer.bones;
                    Mesh mesh = renderer.sharedMesh;
                    Dictionary<Transform, int> specialBonesIndexs = new Dictionary<Transform, int>();
                    for (int j = 0; j < bones.Length; j++)
                    {
                        if (!boneList.Contains(bones[j]))
                        {
                            boneList.Add(bones[j]);
                            bindPosList.Add(mesh.bindposes[j]);
                        }
                        else
                        {
                            int index = boneList.IndexOf(bones[j]);
                            if (!Matrix4x4Equal(bindPosList[index], mesh.bindposes[j]))
                            {
                                boneList.Add(bones[j]);
                                bindPosList.Add(mesh.bindposes[j]);
                                specialBonesIndexs.Add(bones[j], boneList.Count - 1);
                            }
                        }
                    }
                    BoneWeight[] boneWeights = mesh.boneWeights;
                    for (int k = 0; k < boneWeights.Length; k++)
                    {
                        Transform bone = bones[boneWeights[k].boneIndex0];
                        boneWeights[k].boneIndex0 = specialBonesIndexs.ContainsKey(bone) ? specialBonesIndexs[bone] : boneList.IndexOf(bone);

                        bone = bones[boneWeights[k].boneIndex1];
                        boneWeights[k].boneIndex1 = specialBonesIndexs.ContainsKey(bone) ? specialBonesIndexs[bone] : boneList.IndexOf(bone);

                        bone = bones[boneWeights[k].boneIndex2];
                        boneWeights[k].boneIndex2 = specialBonesIndexs.ContainsKey(bone) ? specialBonesIndexs[bone] : boneList.IndexOf(bone);

                        bone = bones[boneWeights[k].boneIndex3];
                        boneWeights[k].boneIndex3 = specialBonesIndexs.ContainsKey(bone) ? specialBonesIndexs[bone] : boneList.IndexOf(bone);
                    }

                    for (int j = 0; j < mesh.subMeshCount; j++)
                    {
                        int[] submeshIndexs = mesh.GetIndices(j);
                        for (int k = 0; k < submeshIndexs.Length; k++)
                        {
                            submeshIndexs[k] = submeshIndexs[k] + vertexList.Count;
                        }

                        indexList.AddRange(submeshIndexs);
                    }

                    vertexList.AddRange(mesh.vertices);
                    boneWeightList.AddRange(boneWeights);
                }

                if (shadowMesh)
                {
                    shadowMesh.Clear(true);
                }
                else
                {
                    shadowMesh = new Mesh();
                }

                Mesh mergedMesh = shadowMesh;
                mergedMesh.vertices = vertexList.ToArray();
                mergedMesh.SetIndices(indexList.ToArray(), MeshTopology.Triangles, 0);
                mergedMesh.bindposes = bindPosList.ToArray();
                mergedMesh.boneWeights = boneWeightList.ToArray();
                mergedMesh.UploadMeshData(false);

                GameObject shadowGoc = new GameObject("shadow");
                shadowGoc.transform.SetParent(transform.Find("root"));
                shadowGoc.transform.localPosition = new Vector3(0, 0, 0);
                shadowGoc.transform.localScale = new Vector3(1, 1, 1);
                shadowGoc.transform.localEulerAngles = new Vector3(-90, 0, 0);

                SkinnedMeshRenderer skinedMeshRender = shadowGoc.AddComponent<SkinnedMeshRenderer>();

                Material material = new Material(planeShadowShader);
                _shadowRenderers.Add(skinedMeshRender);

                material.renderQueue = 3000;
                material.SetFloat("_Intensity", 15);
                material.SetColor("_PlaneColor", Color.black);

                material.SetFloat("_OffsetY", 0);

                skinedMeshRender.sharedMaterial = material;
                skinedMeshRender.bones = boneList.ToArray();
                skinedMeshRender.sharedMesh = mergedMesh;
                skinedMeshRender.receiveShadows = false;
                skinedMeshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                skinedMeshRender.updateWhenOffscreen = true;
            }
        }
    }

    public void Start()
    {
        SkinMeshRendersToPlaneShadow();
    }

}
