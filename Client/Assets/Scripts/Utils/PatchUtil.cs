using System.Linq;

/// <summary>
/// 热更工具类
/// </summary>
public class PatchUtil
{
    private static string[] _patchFiles = null;
    /// <summary>
    /// 获取差异文件的批处理文件
    /// </summary>
    private static readonly string CalcList = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/calclist.bat");
    /// <summary>
    /// 获取版本号的批处理文件
    /// </summary>
    private static readonly string GetVersion = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/getversion.bat");

    /// <summary>
    /// 获取需要热更的文件数组
    /// </summary>
    /// <param name="startVersion">开始版本</param>
    /// <param name="endVersion">结束版本</param>
    /// <returns>热更的文件数组</returns>
    public static string[] GetPatchFiles(string startVersion, string endVersion)
    {
        if (_patchFiles != null)
        {
            System.Array.Clear(_patchFiles, 0, _patchFiles.Length);
        }
#if UNITY_EDITOR
        var dir = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Sources");
        var output = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/diff.txt");
        var arg = $"{startVersion} {endVersion} {output}";
        if (Util.ExecuteBat(dir, CalcList, arg) == 0)
        {
            _patchFiles = System.IO.File.ReadAllLines(output).Where(path => path.StartsWith("Client/Assets/Sources/") || path.StartsWith("Lua/")).ToArray();
        }
#endif
        return _patchFiles;
    }

    /// <summary>
    /// 获取当前版本
    /// </summary>
    /// <returns>版本</returns>
    public static string GetGitVersion()
    {
#if UNITY_EDITOR
        var dir = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Sources");
        var output = FileUtil.CombinePaths(UnityEngine.Application.dataPath.Replace("/Assets", ""), UnityEditor.FileUtil.GetUniqueTempPathInProject());
        var arg = $"{output}";
        if (Util.ExecuteBat(dir, GetVersion, arg) == 0)
        {
            return System.IO.File.ReadLines(output).First();
        }
#endif
        return string.Empty;
    }

}
