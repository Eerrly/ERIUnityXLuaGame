using System.IO;
using System.Collections.Generic;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

public static class EditorWindowExtensions
{
    public static void ShowTip(this UnityEditor.EditorWindow thisRef, string content)
    {
        thisRef.ShowNotification(new UnityEngine.GUIContent(content));
    }
}

public class SpriteAtlasToolsEditorWin : OdinEditorWindow
{

    public static SpriteAtlasToolsEditorWin Open()
    {
        var win = GetWindow<SpriteAtlasToolsEditorWin>("SpriteAtlas Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    private SpriteAtlasToolsConfig cfg;

    protected override void OnEnable()
    {
        base.OnEnable();
        SpriteAtlasToolsUtility.onAddSpriteAtlas += OnUtilityAddSpriteAtlas;
        LoadConfigFile();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SpriteAtlasToolsUtility.onAddSpriteAtlas -= OnUtilityAddSpriteAtlas;
    }

    void OnUtilityAddSpriteAtlas()
    {
        LoadConfigFile();
    }

    void LoadConfigFile()
    {
        cfg = Util.LoadConfig<SpriteAtlasToolsConfig>(SpriteAtlasToolsUtility.ConfigName);
        spriteAtlasSaveDirPath = cfg.spriteAtlasSaveDirPath;
        packingTextureWidthLimit = cfg.packingTextureWidthLimit;
        packingTextureHeightLimit = cfg.packingTextureHeightLimit;
        itemList.Clear();
        for (var i = 0; i < cfg.itemList.Count; i++)
        {
            itemList.Add(new SpriteAtlasItemEditor(this, cfg.itemList[i]));
        }
    }

    [Title("Editor配置")]
    [InfoBox("工具创建的SpriteAtlas文件都会保存在该目录下")]
    [LabelText("SpriteAtlas文件保存目录"), LabelWidth(160)]
    [FolderPath(AbsolutePath = false, ParentFolder = "./", UseBackslashes = false)]
    [PropertyOrder(10)]
    [InlineButton("SelectSpriteAtlasDir", "查看")]
    public string spriteAtlasSaveDirPath;

    void SelectSpriteAtlasDir()
    {
        var isSuccess = Util.SetPathToSelection(spriteAtlasSaveDirPath);
        if (!isSuccess)
        {
            this.ShowTip("路径不存在：构建时将自动创建");
        }
    }

    [Title("打包到纹理集的资源大小限制", "大于等于配置的Width或Height的图片，会在构建时自动排除出SpriteAtlas")]
    [LabelText("宽度(Width)"), LabelWidth(80)]
    [PropertyOrder(11)]
    public int packingTextureWidthLimit;

    [LabelText("高度(Height)")]
    [PropertyOrder(11), LabelWidth(80)]
    public int packingTextureHeightLimit;

    [Title("SpriteAtlas配置")]
    [InfoBox("只需要配置好纹理放置的目录以及生成方式即可，SpriteAtlas的文件名会自动生成")]
    [LabelText("SpriteAtlas文件数据")]
    [PropertyOrder(20)]
    [HideReferenceObjectPicker]
    [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 10, AlwaysAddDefaultValue = true, CustomAddFunction = "AddSpriteAtlasItem", DraggableItems = false)]
    public List<SpriteAtlasItemEditor> itemList = new List<SpriteAtlasItemEditor>();

    public void AddSpriteAtlasItem()
    {
        itemList.Add(new SpriteAtlasItemEditor(this, null));
    }

    [Title("构建")]
    [InfoBox("构建将完成以下操作：\r\n 根据「SpriteAtlas文件数据」创建或更新「SpriteAtlas保存目录」中的SpriteAtlas文件")]
    [Button("构建所有的SpriteAtlas文件", ButtonSizes.Large)]
    [PropertyOrder(30)]
    void BuildAll()
    {
        if (itemList.Count == 0)
        {
            return;
        }

        EditorUtility.DisplayProgressBar("Progress", "spriteatlas building ...", 0);

        //检查保存目录是否存在，不在则生成
        if (!Directory.Exists(spriteAtlasSaveDirPath))
        {
            Directory.CreateDirectory(spriteAtlasSaveDirPath);
        }

        for (var i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            var progress = (i + 1f) / itemList.Count;
            EditorUtility.DisplayProgressBar("Progress", "spriteatlas building ...", progress);

            SpriteAtlasToolsUtility.BuildSpriteAtlas(spriteAtlasSaveDirPath, item.ToSpriteAtlasItem(), packingTextureWidthLimit, packingTextureHeightLimit);
        }

        EditorUtility.ClearProgressBar();
    }

    [Button("触发已构建SpriteAtlas文件的[Pack Preview]，刷新纹理集预览", ButtonSizes.Large)]
    [PropertyOrder(31)]
    void PackPreviewAll()
    {
        SpriteAtlasToolsUtility.PackPreview(spriteAtlasSaveDirPath);
    }

    [Button("保存当前配置", ButtonSizes.Large)]
    [PropertyOrder(32)]
    void SaveConfig()
    {
        var conf = new SpriteAtlasToolsConfig();
        conf.spriteAtlasSaveDirPath = spriteAtlasSaveDirPath;
        conf.packingTextureWidthLimit = packingTextureWidthLimit;
        conf.packingTextureHeightLimit = packingTextureHeightLimit;
        conf.itemList = new List<SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem>();
        foreach (var v in itemList)
        {
            var item = new SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem();
            item.isSubDirSplit = v.isSubDirSplit;
            item.textureDirPath = v.texturesDirPath;
            conf.itemList.Add(item);
        }
        Util.SaveConfig(conf, Constant.ATLAS_CONFIG_NAME);
    }

}

public struct SpriteAtlasItemEditor
{
    SpriteAtlasToolsEditorWin win;

    [HorizontalGroup("Item", MaxWidth = 190)]
    [LabelText("子目录单独生成SpriteAtlas"), LabelWidth(150)]
    public bool isSubDirSplit;

    [LabelText("纹理资源目录"), LabelWidth(100)]
    [HideLabel]
    [FolderPath(AbsolutePath = false, ParentFolder = "./", UseBackslashes = false, RequireExistingPath = true)]
    [InlineButton("SelectFile", "配置SpriteAtlas")]
    [HorizontalGroup("Item")]
    public string texturesDirPath;

    void SelectFile()
    {
        var name = SpriteAtlasToolsUtility.GenerateSpriteAtlasNameByPath(texturesDirPath);
        var filePath = FileUtil.CombinePaths(win.spriteAtlasSaveDirPath, name);
        var isSuccess = Util.SetPathToSelection(filePath);
        if (isSuccess)
        {
            return;
        }
        isSuccess = Util.SetPathToSelection(win.spriteAtlasSaveDirPath);
        if (!isSuccess)
        {
            win.ShowTip($"[{name}]不存在：未构建");
        }
    }

    [Button("构建")]
    [HorizontalGroup("Item", MaxWidth = 60)]
    void Build()
    {
        EditorUtility.DisplayProgressBar("Progress", "spriteatlas building ...", 0);
        SpriteAtlasToolsUtility.BuildSpriteAtlas(win.spriteAtlasSaveDirPath, ToSpriteAtlasItem(), win.packingTextureWidthLimit, win.packingTextureHeightLimit);
        EditorUtility.ClearProgressBar();
    }

    public SpriteAtlasItemEditor(SpriteAtlasToolsEditorWin win, SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem item)
    {
        if(item == null)
        {
            item = new SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem();
        }
        this.texturesDirPath = item.textureDirPath;
        this.isSubDirSplit = item.isSubDirSplit;
        this.win = win;
    }

    public SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem ToSpriteAtlasItem()
    {
        var item = new SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem();
        item.textureDirPath = texturesDirPath;
        item.isSubDirSplit = isSubDirSplit;
        return item;
    }

}
