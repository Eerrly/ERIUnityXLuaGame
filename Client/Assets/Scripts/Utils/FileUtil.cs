using System;
using System.IO;

/// <summary>
/// 文件工具
/// </summary>
public class FileUtil
{
    /// <summary>
    /// 如果路径开头有文件分隔符，则移除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveStartPathSeparator(string path)
    {
        char[] pathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        string trimmedPath = path.TrimStart(pathSeparators);
        return trimmedPath;
    }

    /// <summary>
    /// 标准化路径中的路径分隔符（统一使用“/”符号）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string Normalized(string path)
    {
        path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return path;
    }

    /// <summary>
    /// 将给的路径合并起来
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string CombinePaths(params string[] args)
    {
        if (args.Length == 0)
        {
            return "";
        }

        string path = args[0];
        for (int i = 1; i < args.Length; i++)
        {
            var node = RemoveStartPathSeparator(args[i]);
            path = Path.Combine(path, node);
        }

        //为了好看
        path = Normalized(path);

        return path;
    }

    /// <summary>
    /// 创建文件夹目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CreateDirectory(string path)
    {
        for (var i = 0; i < 3; ++i)
        {
            if (!Directory.Exists(path))
            {
                var failed = false;
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (System.Exception e)
                {
                    failed = true;
                    if (i == 2)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
                if (!failed)
                {
                    break;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 拷贝文件
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="overwrite"></param>
    public static void CopyFile(string source, string target, bool overwrite = true)
    {
        if (false == File.Exists(source))
        {
            throw new Exception(string.Format("文件不存在:[{0}]", source));
        }

        var targetDir = Directory.GetParent(target);
        if (false == targetDir.Exists)
        {
            targetDir.Create();
        }

        File.Copy(source, target, overwrite);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool DeleteFile(string path)
    {
        for (var i = 0; i < 3; ++i)
        {
            if (System.IO.File.Exists(path))
            {
                var failed = false;
                try
                {
                    System.IO.File.Delete(path);
                    return true;
                }
                catch (System.Exception e)
                {
                    failed = true;
                    if (i == 2)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
                if (!failed)
                {
                    break;
                }
            }
        }
        return false;
    }

}
