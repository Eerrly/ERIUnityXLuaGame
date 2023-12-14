using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

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
        _itemList.Clear();
        foreach (var item in _config.Items)
        {
            _itemList.Add(new AssetBundleToolsItemEditor(this, item));
        }
    }

    [HideLabel] 
    [LabelText("资源数据")] 
    public List<AssetBundleToolsItemEditor> _itemList = new List<AssetBundleToolsItemEditor>();

    public struct AssetBundleToolsItemEditor
    {
        AssetBundleToolsEditorWin _win;

        private uint hash;
        private List<uint> dependencies;

        [LabelText("AssetBundle文件"), LabelWidth(100)]
        [HideLabel, FilePath]
        [HorizontalGroup("Zero")]
        public string AssetBundleFile;

        [LabelText(",字节总长"), LabelWidth(70)]
        [HideLabel]
        [HorizontalGroup("Zero")]
        public int Size;

        [LabelText("路径"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Two")]
        public string PackageResourcePath;

        [LabelText("所有的依赖"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Three")]
        public readonly List<AssetBundleToolsItemEditor> AssetBundleToolsItemEditors;

        public AssetBundleToolsItemEditor(AssetBundleToolsEditorWin win, AssetBundleToolsConfig.AssetBundleToolsConfigItem item)
        {
            this._win = win;

            hash = item.hash;
            Size = item.size;
            PackageResourcePath = item.packageResourcePath;
            dependencies = item.dependencies;

            AssetBundleFile = FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, hash + ".s");
            AssetBundleToolsItemEditors = new List<AssetBundleToolsItemEditor>();
            
            FindDependencies(ref AssetBundleToolsItemEditors);
        }

        private List<AssetBundleToolsItemEditor> FindDependencies(ref List<AssetBundleToolsItemEditor> assetBundleToolsItemEditors)
        {
            foreach (var v in _win._config.Items)
            {
                if (dependencies.Contains(v.hash))
                {
                    assetBundleToolsItemEditors.Add(new AssetBundleToolsItemEditor(_win, v));
                }
            }
            return assetBundleToolsItemEditors;
        }

    }
}
