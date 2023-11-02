using System.IO;
using UnityEditor;

public class ToolbarEditorMenu
{
    /// <summary>
    /// 图集工具
    /// </summary>
    [MenuItem("工具/SpriteAtlas Tools", false, 1000)]
    public static void SpriteAtlasTools()
    {
        SpriteAtlasToolsEditorWin.Open();
    }

    /// <summary>
    /// 构建AB工具
    /// </summary>
    [MenuItem("工具/Build Tools", false, 1000)]
    public static void BuildTools()
    {
        BuildToolsEditorWin.Open();
    }

    /// <summary>
    /// AB资源查看工具
    /// </summary>
    [MenuItem("工具/AssetBundle Tools", false, 1000)]
    public static void AssetBundleTools()
    {
        AssetBundleToolsEditorWin.Open();
    }

    /// <summary>
    /// 构建热更工具
    /// </summary>
    [MenuItem("工具/Patch Tools", false, 1000)]
    public static void PatchTools()
    {
        PatchToolsEditorWin.Open();
    }

    /// <summary>
    /// 节点工具
    /// </summary>
    [MenuItem("工具/Node Tools", false, 1000)]
    public static void NodeTools()
    {
        NodeToolsEditorWin.Open();
    }

    /// <summary>
    /// 缓存路径工具
    /// </summary>
    [MenuItem("工具/Cache Tools", false, 1000)]
    public static void CacheTools()
    {
        CacheToolsEditorWin.Open();
    }

}

public class RightClickEditorMenu
{

    [MenuItem("Assets/工具/SpriteAtlas Tools/添加目录到SpriteAtlas配置", false, 1)]
    static void SpriteAtlasAdd()
    {
        if (Selection.objects.Length != 1)
        {
            EditorUtility.DisplayDialog("错误", "仅支持[单选]的[文件夹]", "OK");
            return;
        }

        var obj = Selection.objects[0];
        var assetPath = AssetDatabase.GetAssetPath(obj);
        if (false == Directory.Exists(assetPath))
        {
            EditorUtility.DisplayDialog("错误", "仅支持[单选]的[文件夹]", "OK");
            return;
        }

        SpriteAtlasToolsUtility.AddSpriteAtlas(assetPath, false);
    }

}