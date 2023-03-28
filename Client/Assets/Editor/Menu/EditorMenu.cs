using System.IO;
using UnityEditor;

public class ToolbarEditorMenu
{
    [MenuItem("工具/SpriteAtlas Tools", false, 1000)]
    public static void SpriteAtlasTools()
    {
        SpriteAtlasToolsEditorWin.Open();
    }

    [MenuItem("工具/Build Tools", false, 1000)]
    public static void BuildTools()
    {
        BuildToolsEditorWin.Open();
    }

    [MenuItem("工具/Patch Tools", false, 1000)]
    public static void PatchTools()
    {
        PatchToolsEditorWin.Open();
    }

    [MenuItem("工具/Test", false, 1000)]
    public static void Test()
    {
        var l = FileUtil.CombinePaths(UnityEngine.Application.dataPath, @"Examples\Tools\LuaJit\luajit64.exe");
        var s = @"E:\GitProjects\ERIUnitySimpleGame\Lua\init.lua";
        var t = @"E:\GitProjects\ERIUnitySimpleGame\Client\Assets\Sources\Lua\init.bytes";
        var r = Util.ExecuteBat(
            Path.GetDirectoryName(l),
            l, 
            string.Format("{0} {1} {2}", "-b", s, t));
        UnityEngine.Debug.Log("r : " + r);
        //ResUtil.BuildLuaScripts();

        var luaEnv = new XLua.LuaEnv();
        luaEnv.DoString(File.ReadAllBytes(t));
        luaEnv.Dispose();
        luaEnv = null;
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