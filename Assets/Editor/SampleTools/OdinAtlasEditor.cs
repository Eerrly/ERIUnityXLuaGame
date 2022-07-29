using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEngine;
using System.Collections.Generic;

public class AtlasSettings
{
    public AtlasSettings(int schemeIndex)
    {
        this.schemeIndex = schemeIndex;
    }

    public int schemeIndex;

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
}

public class InputData
{
    public InputData(string spriteFolderParent = null)
    {
        this.spriteFolderParent = spriteFolderParent;
        if(!string.IsNullOrEmpty(spriteFolderParent) && Directory.Exists(spriteFolderParent))
        {
            spriteFolders = Directory.GetDirectories(spriteFolderParent);
        }
    }

    public int schemeIndex = 0;

    [FolderPath(ParentFolder = "Assets")]
    [OnValueChanged("OnSpriteFolderParentChanged")]
    public string spriteFolderParent;

    [FolderPath(ParentFolder = "Assets")]
    public string[] spriteFolders;
}

public class OdinAtlasEditor : OdinEditorWindow
{
    private bool isInit = false;

    [MenuItem("SampleTools/Atlas Tools")]
    public static void ShowAtlasWin()
    {
        var window = GetWindow<OdinAtlasEditor>();
        window.Show();
    }

    public List<InputData> InputDatas;

    public List<AtlasSettings> settings;

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
        InputDatas = new List<InputData>();
        settings = new List<AtlasSettings>();
        OnInitialize();
    }

    public void OnInitialize()
    {
        settings.Add(new AtlasSettings(0));
    }

    [Button("Generate Atlas", ButtonSizes.Large, ButtonStyle.Box)]
    public void GenerateAtlas()
    {
        atlaes.Clear();
        for (int i = 0; i < InputDatas.Count; i++)
        {
            for (int j = 0; j < InputDatas[i].spriteFolders.Length; j++)
            {

            }
        }
        isInit = true;
    }

    public void Generate(string directoryPath, int index)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        string atlasPath = Path.Combine(outputPath, directoryInfo.Name + ".spriteatlas");
        if (File.Exists(atlasPath)) File.Delete(atlasPath);

        SpriteAtlas atlas = new SpriteAtlas();
        SpriteAtlasPackingSettings spriteAtlasPackingSettings = new SpriteAtlasPackingSettings()
        {
            blockOffset = settings[InputDatas[index].schemeIndex].blockOffset,
            enableRotation = settings[InputDatas[index].schemeIndex].enableRotation,
            enableTightPacking = settings[InputDatas[index].schemeIndex].enableTightPacking,
            padding = settings[InputDatas[index].schemeIndex].padding,
        };
        atlas.SetPackingSettings(spriteAtlasPackingSettings);
        SpriteAtlasTextureSettings spriteAtlasTextureSettings = new SpriteAtlasTextureSettings()
        {
            readable = settings[InputDatas[index].schemeIndex].readable,
            generateMipMaps = settings[InputDatas[index].schemeIndex].generateMipMaps,
            sRGB = settings[InputDatas[index].schemeIndex].sRGB,
            filterMode = settings[InputDatas[index].schemeIndex].filterMode,
        };
        atlas.SetTextureSettings(spriteAtlasTextureSettings);
        TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings()
        {
            maxTextureSize = settings[InputDatas[index].schemeIndex].maxTextureSize,
            format = settings[InputDatas[index].schemeIndex].format,
            crunchedCompression = settings[InputDatas[index].schemeIndex].crunchedCompression,
            textureCompression = settings[InputDatas[index].schemeIndex].textureCompression,
            compressionQuality = settings[InputDatas[index].schemeIndex].compressionQuality,
        };
        atlas.SetPlatformSettings(textureImporterPlatformSettings);
        AssetDatabase.CreateAsset(atlas, atlasPath);

        FileInfo[] files = directoryInfo.GetFiles("*.png");
        foreach (FileInfo file in files)
        {
            atlas.Add(new[] { AssetDatabase.LoadAssetAtPath<Sprite>(directoryPath + "/" + file.Name) });
        }
        AssetDatabase.SaveAssets();

        atlaes.Add(AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath));
    }

    protected override void OnDestroy()
    {
        atlaes.Clear();
        isInit = false;
    }

}
