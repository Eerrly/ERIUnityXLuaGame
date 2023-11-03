using System.IO;
using UnityEngine;

public partial class Setting
{
    public static readonly string EditorResourcePath = "Assets/Resources";
    public static readonly string EditorBundlePath = "Assets/Sources";
    public static readonly string EditorScriptRoot = "../Lua";
    public static readonly string EditorBundleBuildCachePath = "BundleCache/" + Platform;
    public static readonly string EditorSpriteAtlasPath = EditorBundlePath + "/SpriteAtlas";
    public static readonly string EditorScriptBundleName = EditorBundlePath + "/Lua";
    public static readonly string EditorConfigPath = "Configs";
    public static readonly string EditorPatchPath = "Patch";

    public static string StreamingRoot => Application.streamingAssetsPath;
    /// <summary>
    /// 运行时Lua路径
    /// </summary>
    public static string RuntimeScriptBundleName
    {
        get
        {
            // 当指针大小为4时，为32位否则为64位
            if(System.IntPtr.Size == 4)
            {
                return EditorBundlePath + "/Lua/32";
            }
            else
            {
                return EditorBundlePath + "/Lua/64";
            }
        }
    }

    private static string _streamingBundleRoot = null;
    public static string StreamingBundleRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_streamingBundleRoot))
            {
                _streamingBundleRoot = FileUtil.CombinePaths(StreamingRoot, "Bundle");
            }
            return _streamingBundleRoot;
        }
    }

    private static string _uniqueID = null;
    public static string UniqueID
    {
        get
        {
            if (string.IsNullOrEmpty(_uniqueID))
            {
                _uniqueID = Util.MD5(Application.persistentDataPath);
            }
            return _uniqueID;
        }
    }

    private static string _root = null;
    public static string Root
    {
        get
        {
            if(_root == null)
            {
                _root = UnityEngine.Application.dataPath.Replace("/Client/Assets", "");
            }
            return _root;
        }
    }

    private static string _clientRoot = null;
    public static string ClientRoot
    {
        get
        {
            if(_clientRoot == null)
            {
                _clientRoot = UnityEngine.Application.dataPath.Replace("/Assets", "");
            }
            return _clientRoot;
        }
    }

    private static string _cacheRoot = null;
    public static string CacheRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_cacheRoot))
            {
                _cacheRoot = FileUtil.CombinePaths(Application.persistentDataPath, UniqueID);
            }
            return _cacheRoot;
        }
    }

    private static string _cacheBundleRoot = null;
    public static string CacheBundleRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_cacheBundleRoot))
            {
                _cacheBundleRoot = FileUtil.CombinePaths(CacheRoot, "Bundle");
            }
            return _cacheBundleRoot;
        }
    }

    private static string _platForm = null;

    public static string Platform
    {
        get
        {
            if (_platForm == null)
            {
#if UNITY_EDITOR
                switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
                {
                    case UnityEditor.BuildTarget.Android:
                        _platForm = "Android";
                        break;
                    case UnityEditor.BuildTarget.iOS:
                        _platForm = "IOS";
                        break;
                    default:
                        _platForm = "Standalone";
                        break;
                }
#else
                    switch (Application.platform) {
                        case RuntimePlatform.Android:
                            _platForm = "Android";
                            break;
                        case RuntimePlatform.IPhonePlayer:
                            _platForm = "IOS";
                            break;
                        default:
                            _platForm = "Default";
                            break;
                    }
#endif
            }
            return _platForm;
        }
    }

    /// <summary>
    /// Lua文件目录
    /// </summary>
    public static string EditorLuaScriptRoot => FileUtil.CombinePaths(ClientRoot, EditorScriptRoot);

    /// <summary>
    /// 默认层级
    /// </summary>
    public static readonly int LAYER_DEFAULT = 0;

    /// <summary>
    /// 显示层级
    /// </summary>
    public static readonly int LAYER_UI = 5;

    /// <summary>
    /// 隐藏层级
    /// </summary>
    public static readonly int LAYER_HIDE = 31;

}

public partial class Setting
{
    private static BuildToolsConfig _config = null;

    public static BuildToolsConfig Config
    {
        get
        {
            if(_config == null)
            {
                _config = Util.LoadConfig<BuildToolsConfig>(Constant.CLIENT_CONFIG_NAME);
            }
            return _config;
        }
    }

}
