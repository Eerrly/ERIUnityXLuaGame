using System.IO;
using UnityEngine;

public class Setting
{
    public static readonly string EditorResourcePath = "Assets/Resources";
    public static readonly string EditorBundlePath = "Assets/Sources";
    public static readonly string EditorScriptRoot = "../Lua";

    public static bool UseAssetBundle = false;

    public static string StreamingRoot => Application.streamingAssetsPath;

    private static string _streamingBundleRoot = null;
    public static string StreamingBundleRoot
    {
        get
        {
            if (string.IsNullOrEmpty(_streamingBundleRoot))
            {
                _streamingBundleRoot = Path.Combine(_streamingBundleRoot, "Bundle");
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

}
