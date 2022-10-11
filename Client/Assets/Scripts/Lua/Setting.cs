using System.IO;
using UnityEngine;

public partial class Setting
{
    public static readonly string EditorResourcePath = "Assets/Resources";
    public static readonly string EditorBundlePath = "Assets/Sources";
    public static readonly string EditorScriptRoot = "../Lua";
    public static readonly string EditorBundleBuildCachePath = "BundleCache/" + Platform;
    public static readonly string EditorSpriteAtlasPath = EditorBundlePath + "/SpriteAtlas";
    public static readonly string EditorConfigPath = "Configs";

    public static string StreamingRoot => Application.streamingAssetsPath;

    private static string _streamingBundleRoot = null;
    public static string StreamingBundleRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_streamingBundleRoot))
            {
                _streamingBundleRoot = Path.Combine(StreamingRoot, "Bundle");
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

    private static string _cacheRoot = null;
    public static string CacheRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_cacheRoot))
            {
                _cacheRoot = Path.Combine(Application.persistentDataPath, UniqueID);
            }
            return _cacheRoot;
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
