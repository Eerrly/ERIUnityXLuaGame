using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEngine;
using System.Collections.Generic;

public class OdinAtlasEditor : OdinEditorWindow
{
    private bool isInit = false;

    [MenuItem("SampleTools/Atlas Tools")]
    public static void ShowAtlasWin()
    {
        var window = GetWindow<OdinAtlasEditor>();
        window.Show();
    }

    public string[] spritePaths;

    [InfoBox("Sprite Atlas Packing Settings", InfoMessageType.None)]
    public int blockOffset = 1;
    public bool enableRotation = false;
    public bool enableTightPacking = false;
    public int padding = 2;

    [InfoBox("Sprite Atlas Texture Settings", InfoMessageType.None)]
    public bool readable = false;
    public bool generateMipMaps = false;
    public bool sRGB = true;
    public FilterMode filterMode = FilterMode.Bilinear;

    [InfoBox("Texture Importer Platform Settings", InfoMessageType.None)]
    public int maxTextureSize = 2048;
    public TextureImporterFormat format = TextureImporterFormat.Automatic;
    public bool crunchedCompression = true;
    public TextureImporterCompression textureCompression = TextureImporterCompression.Compressed;
    public int compressionQuality = 50;

    [PropertySpace(20)]
    public string outputPath = "Assets/Resources/Atlas";

    [ShowIf("isInit", true)]
    [ReadOnly]
    [LabelText("The Resulting Atlas")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    public List<SpriteAtlas> atlaes;

    protected override void OnEnable()
    {
        atlaes = new List<SpriteAtlas>();
        spritePaths = Directory.GetDirectories("Assets/Resources/Sprites");
    }

    [Button("Generate Atlas", ButtonSizes.Large, ButtonStyle.Box)]
    public void GenerateAtlas()
    {
        atlaes.Clear();
        for (int i = 0; i < spritePaths.Length; i++)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(spritePaths[i]);
            string atlasPath = Path.Combine(outputPath, directoryInfo.Name + ".spriteatlas");
            if (File.Exists(atlasPath)) File.Delete(atlasPath);

            SpriteAtlas atlas = new SpriteAtlas();
            SpriteAtlasPackingSettings spriteAtlasPackingSettings = new SpriteAtlasPackingSettings()
            {
                blockOffset = blockOffset,
                enableRotation = enableRotation,
                enableTightPacking = enableTightPacking,
                padding = padding,
            };
            atlas.SetPackingSettings(spriteAtlasPackingSettings);
            SpriteAtlasTextureSettings spriteAtlasTextureSettings = new SpriteAtlasTextureSettings()
            {
                readable = readable,
                generateMipMaps = generateMipMaps,
                sRGB = sRGB,
                filterMode = filterMode,
            };
            atlas.SetTextureSettings(spriteAtlasTextureSettings);
            TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings()
            {
                maxTextureSize = maxTextureSize,
                format = format,
                crunchedCompression = crunchedCompression,
                textureCompression = textureCompression,
                compressionQuality = compressionQuality,
            };
            atlas.SetPlatformSettings(textureImporterPlatformSettings);
            AssetDatabase.CreateAsset(atlas, atlasPath);

            FileInfo[] files = directoryInfo.GetFiles("*.png");
            foreach (FileInfo file in files)
            {
                atlas.Add(new[] { AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i] + "/" + file.Name) });
            }
            AssetDatabase.SaveAssets();

            atlaes.Add(AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath));
        }
        isInit = true;
    }

    protected override void OnDestroy()
    {
        atlaes.Clear();
        isInit = false;
    }

}
