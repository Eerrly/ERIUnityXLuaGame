using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

public class AssetBundleToolsEditorWin : OdinEditorWindow
{
    public static AssetBundleToolsEditorWin Open()
    {
        var win = GetWindow<AssetBundleToolsEditorWin>("AssetBundles Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    private AssetBundleToolsConfig _config;

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadConfigFile();
    }

    private void LoadConfigFile()
    {
        _config = Util.LoadConfig<AssetBundleToolsConfig>(Constant.ASSETBUNDLES_CONFIG_NAME);
        itemList.Clear();
        foreach (var item in _config.Items)
        {
            itemList.Add(new AssetBundleToolsItemEditor(this, item));
        }
    }

    [HideLabel] [LabelText("资源数据")] 
    public List<AssetBundleToolsItemEditor> itemList = new List<AssetBundleToolsItemEditor>();

    public struct AssetBundleToolsItemEditor
    {
        AssetBundleToolsEditorWin win;

        private uint hash;
        private List<uint> dependencies;

        [LabelText("AssetBundle文件"), LabelWidth(100)]
        [HideLabel, FilePath]
        [HorizontalGroup("Zero")]
        public string assetBundleFile;

        [LabelText(",字节总长"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("One")]
        public int size;

        [LabelText("路径"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Two")]
        public string packageResourcePath;

        [LabelText("所有的依赖"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Three")]
        public List<AssetBundleToolsItemEditor> assetBundleToolsItemEditors;

        public AssetBundleToolsItemEditor(AssetBundleToolsEditorWin win, AssetBundleToolsConfig.AssetBundleToolsConfigItem item)
        {
            this.win = win;

            hash = item.hash;
            size = item.size;
            packageResourcePath = item.packageResourcePath;
            dependencies = item.dependencies;

            assetBundleFile = FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, hash + ".s");
            assetBundleToolsItemEditors = new List<AssetBundleToolsItemEditor>();
            
            FindDependencies(ref assetBundleToolsItemEditors);
        }

        private List<AssetBundleToolsItemEditor> FindDependencies(ref List<AssetBundleToolsItemEditor> assetBundleToolsItemEditors)
        {
            foreach (var v in win._config.Items)
            {
                if (dependencies.Contains(v.hash))
                {
                    assetBundleToolsItemEditors.Add(new AssetBundleToolsItemEditor(win, v));
                }
            }
            return assetBundleToolsItemEditors;
        }

    }
}
