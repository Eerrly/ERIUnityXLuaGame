using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;

public class BuildToolsEditorWin : OdinEditorWindow
{
    public static BuildToolsEditorWin Open()
    {
        var win = GetWindow<BuildToolsEditorWin>("Build Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    private BuildToolsConfig cfg;

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadConfigFile();
    }

    public void LoadConfigFile()
    {
        cfg = Util.LoadConfig<BuildToolsConfig>(Constant.CLIENT_CONFIG_NAME);
        enablePatching = cfg.enablePatching;
        useAssetBundle = cfg.useAssetBundle;
        itemList.Clear();
        foreach (var item in cfg.itemList)
        {
            itemList.Add(new BuildToolsItemEidtor(this, item));
        }
    }

    [Title("游戏及构建资源配置")]
    [LabelText("是否使用热更 (enablePatching)"), LabelWidth(300)]
    public bool enablePatching;

    [LabelText("是否使用assetbunle资源加载 (useAssetBundle)"), LabelWidth(300)]
    public bool useAssetBundle;

    [InfoBox("AssetBundle资源包配置 (Assets/Sources/)")]
    [HideLabel]
    [LabelText("资源数据")]
    public List<BuildToolsItemEidtor> itemList = new List<BuildToolsItemEidtor>();

    [Button("保存当前配置", ButtonSizes.Large)]
    public void SaveConfigFile()
    {
        cfg.enablePatching = this.enablePatching;
        cfg.useAssetBundle = this.useAssetBundle;
        cfg.itemList.Clear();
        foreach (var item in itemList)
        {
            var c = new BuildToolsConfig.BuildToolsConfigItem()
            {
                root = item.root,
                extension = item.extension,
                filter = item.filter,
                searchoption = (int)item.searchoption,
                directories = item.directories
            };
            if (string.IsNullOrEmpty(c.root))
            {
                this.ShowTip("根路径配置为空");
                return;
            }
            cfg.itemList.Add(c);
        }
        Util.SaveConfig(cfg, Constant.CLIENT_CONFIG_NAME);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        this.ShowTip("保存成功！");
    }

    [Button("开始构建", ButtonSizes.Large)]
    public void Build()
    {
        EditorUtility.DisplayProgressBar("Progress", "assetbundle building ...", 0);
        ResUtil.Build();
        EditorUtility.ClearProgressBar();

        var curVersion = PatchUtil.GetGitVersion();
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_START_VERSION", curVersion);
        UnityEngine.PlayerPrefs.Save();
        this.ShowTip("构建成功！");
    }
}


public struct BuildToolsItemEidtor
{
    BuildToolsEditorWin win;

    [LabelText("资源目录"), LabelWidth(50)]
    [HideLabel]
    [FolderPath(AbsolutePath = false, ParentFolder = "Assets/Sources/", UseBackslashes = false, RequireExistingPath = true)]
    [HorizontalGroup("Path")]
    public string root;

    [LabelText(",文件后缀"), LabelWidth(55)]
    [HideLabel]
    [HorizontalGroup("Path")]
    public string extension;

    [LabelText(",筛选格式"), LabelWidth(55)]
    [HideLabel]
    [HorizontalGroup("Path")]
    public string filter;

    [LabelText("是否以文件夹为单位"), LabelWidth(120)]
    [HideLabel]
    [HorizontalGroup("Setting")]
    public bool directories;

    [LabelText(",搜索选项"), LabelWidth(80)]
    [HideLabel]
    [HorizontalGroup("Setting")]
    public SearchOption searchoption;

    [LabelText("资源列表"), LabelWidth(300)]
    [HideLabel]
    [HorizontalGroup("Assets")]
    public string[] assets;

    public BuildToolsItemEidtor(BuildToolsEditorWin win, BuildToolsConfig.BuildToolsConfigItem item)
    {
        if(item == null)
        {
            item = new BuildToolsConfig.BuildToolsConfigItem();
        }
        root = string.IsNullOrEmpty(item.root) ? "" : item.root;
        directories = item.directories;
        searchoption = (SearchOption)item.searchoption;
        extension = string.IsNullOrEmpty(item.extension) ? "" : item.extension;
        filter = string.IsNullOrEmpty(item.filter) ? "*.*" : item.filter;
        assets = Util.FindAssets(FileUtil.CombinePaths(Setting.EditorBundlePath, root), searchoption, filter, directories);

        this.win = win;
    }

}