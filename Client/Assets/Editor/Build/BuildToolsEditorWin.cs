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

    private BuildToolsConfig _config;

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadConfigFile();
    }

    private void LoadConfigFile()
    {
        _config = Util.LoadConfig<BuildToolsConfig>(Constant.CLIENT_CONFIG_NAME);
        enablePatching = _config.enablePatching;
        useAssetBundle = _config.useAssetBundle;
        ItemList.Clear();
        foreach (var item in _config.itemList)
        {
            ItemList.Add(new BuildToolsItemEditor(this, item));
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
    public readonly List<BuildToolsItemEditor> ItemList = new List<BuildToolsItemEditor>();

    [Button("保存当前配置", ButtonSizes.Large)]
    public void SaveConfigFile()
    {
        _config.enablePatching = this.enablePatching;
        _config.useAssetBundle = this.useAssetBundle;
        _config.itemList.Clear();
        foreach (var item in ItemList)
        {
            var c = new BuildToolsConfig.BuildToolsConfigItem()
            {
                root = item.Root,
                filter = item.Filter,
                searchOption = (int)item.SearchOption,
                directories = item.Directories
            };
            if (string.IsNullOrEmpty(c.root))
            {
                this.ShowTip("根路径配置为空");
                return;
            }
            _config.itemList.Add(c);
        }
        Util.SaveConfig(_config, Constant.CLIENT_CONFIG_NAME);

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

public struct BuildToolsItemEditor
{
    BuildToolsEditorWin _win;

    [LabelText("资源目录"), LabelWidth(70)]
    [HideLabel]
    [FolderPath(AbsolutePath = false, ParentFolder = "Assets/Sources/", UseBackslashes = false, RequireExistingPath = true)]
    [HorizontalGroup("Setting")]
    [OnValueChanged("RefreshAssets")]
    public readonly string Root;

    [LabelText(",是否以文件夹为单位"), LabelWidth(120)]
    [HideLabel]
    [HorizontalGroup("Setting", 50)]
    [OnValueChanged("RefreshAssets")]
    public readonly bool Directories;
    
    [LabelText(",筛选格式"), LabelWidth(55)]
    [HideLabel]
    [HorizontalGroup("Setting", 70)]
    [OnValueChanged("RefreshAssets")]
    public readonly string Filter;

    [LabelText(",搜索选项"), LabelWidth(60)]
    [HideLabel]
    [HorizontalGroup("Setting", 200)]
    [OnValueChanged("RefreshAssets")]
    public readonly SearchOption SearchOption;

    [LabelText("资源列表"), LabelWidth(300)]
    [HideLabel]
    [HorizontalGroup("Assets")]
    public string[] Assets;

    public BuildToolsItemEditor(BuildToolsEditorWin win, BuildToolsConfig.BuildToolsConfigItem item)
    {
        if(item == null)
        {
            item = new BuildToolsConfig.BuildToolsConfigItem();
        }
        Root = string.IsNullOrEmpty(item.root) ? "" : item.root;
        Directories = item.directories;
        SearchOption = (SearchOption)item.searchOption;
        Filter = string.IsNullOrEmpty(item.filter) ? "*.*" : item.filter;
        Assets = Util.FindAssets(FileUtil.CombinePaths(Setting.EditorBundlePath, Root), SearchOption, Filter, Directories);

        this._win = win;
    }

    public void RefreshAssets()
    {
        Assets = Util.FindAssets(FileUtil.CombinePaths(Setting.EditorBundlePath, Root), SearchOption, Filter, Directories);
    }

}