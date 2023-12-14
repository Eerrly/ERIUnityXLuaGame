using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Util
{
    private static readonly Encoding UTF8 = new System.Text.UTF8Encoding(false);
    private static readonly List<Transform> SetGameObjectLayerList = new List<Transform>();

    /// <summary>
    /// 获取或者添加组件
    /// </summary>
    /// <typeparam name="T">组件</typeparam>
    /// <param name="go">物体</param>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T t = null;
        if (null == go) return null;
        
        t = go.GetComponent<T>();
        if(t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }

    /// <summary>
    /// 设置GameObject的Layer
    /// </summary>
    /// <param name="go">物体</param>
    /// <param name="layer">层级</param>
    /// <param name="includeChildren">是否包括他的全部子物体</param>
    public static void SetGameObjectLayer(GameObject go, int layer, bool includeChildren)
    {
        if (null == go || go.layer == layer) return;
        
        if (includeChildren)
        {
            SetGameObjectLayerList.Clear();
            go.GetComponentsInChildren<Transform>(true, SetGameObjectLayerList);
            foreach (var igo in SetGameObjectLayerList)
            {
                igo.gameObject.layer = layer;
            }
        }
        else
        {
            go.layer = layer;
        }
    }

    /// <summary>
    /// 执行属性类方法
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="classType">类类型</param>
    /// <param name="inherit">是否类继承</param>
    /// <param name="methodType">方法类型</param>
    /// <param name="methodInherit">是否方法继承</param>
    public static void InvokeAttributeCall(object obj, Type classType, bool inherit, Type methodType, bool methodInherit)
    {
        if (null == obj) return;
        
        var types = obj.GetType().Assembly.GetExportedTypes();
        foreach (var t in types)
        {
            if (!t.IsDefined(classType, inherit)) continue;
            
            var methods = t.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            foreach (var t1 in methods)
            {
                if (t1.IsDefined(methodType, methodInherit))
                {
                    t1.Invoke(null, null);
                }
            }
        }
    }

    /// <summary>
    /// 从字符串获取MD5值
    /// </summary>
    /// <param name="str">字符串</param>
    /// <returns></returns>
    public static string MD5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);
        return MD5(bytes);
    }

    /// <summary>
    /// 从二进制数据获取MD5值
    /// </summary>
    /// <param name="bytes">二进制数据</param>
    /// <returns></returns>
    public static string MD5(byte[] bytes)
    {
        using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        {
            var result = md5.ComputeHash(bytes);
            var builder = new System.Text.StringBuilder();
            foreach (var t in result)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 从文件流获取MD5值
    /// </summary>
    /// <param name="fs">文件流</param>
    /// <returns></returns>
    public static string MD5(FileStream fs)
    {
        using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        {
            var result = md5.ComputeHash(fs);
            var builder = new System.Text.StringBuilder();
            foreach (var t in result)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 保存配置表
    /// </summary>
    /// <param name="data">配置表类实例</param>
    /// <param name="fileName">需要保存的配置表文件名</param>
    public static void SaveConfig(object data, string fileName)
    {
        var json = Regex.Unescape(Newtonsoft.Json.JsonConvert.SerializeObject(data));
        File.WriteAllText(FileUtil.CombinePaths(Setting.EditorResourcePath, Setting.EditorConfigPath, fileName), json, UTF8);
    }

    /// <summary>
    /// 加载配置表
    /// </summary>
    /// <typeparam name="T">配置表类</typeparam>
    /// <param name="fileName">配置表资源路径</param>
    /// <returns></returns>
    public static T LoadConfig<T>(string fileName)
    {
        var path = FileUtil.CombinePaths(Setting.EditorConfigPath, fileName);
        var configPath = path.Substring(0, path.LastIndexOf(".", StringComparison.Ordinal));
        try
        {
            var json = UTF8.GetString(Resources.Load<TextAsset>(configPath).bytes);
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                
            };
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return t;
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"LoadConfig {path} Error !! Msg : {e.Message}");
        }
        var obj = System.Activator.CreateInstance<T>();
        return obj;
    }

    /// <summary>
    /// 编辑器模式下，设置路径为当前选择
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static bool SetPathToSelection(string path)
    {
#if UNITY_EDITOR
        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (obj == null) return false;
        
        Selection.objects = new UnityEngine.Object[] { obj };
        return true;
#endif
        return false;
    }

    /// <summary>
    /// 将路径转换为Hash值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static unsafe uint HashPath(string input)
    {
        uint h = 2166136261;
        fixed (char* key = input)
        {
            int lenght = input.Length;
            for (var i = 0; i < lenght; ++i)
            {
                h = (h * 16777619) ^ (byte)key[i];
            }
        }
        return h;
    }

    /// <summary>
    /// 检测是否为空或者False，提示错误
    /// </summary>
    /// <param name="o"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static bool CheckAndLogError(object o, string error)
    {
        if (o == null || (o is bool && !(bool)o))
        {
            Debug.LogError(error);
            return false;
        }
        return true;
    }

    /// <summary>
    /// 执行批处理
    /// </summary>
    /// <param name="dir">需要执行批处理所需要的当前目录</param>
    /// <param name="bat">批处理路径</param>
    /// <param name="arg">参数</param>
    /// <returns></returns>
    public static int ExecuteBat(string dir, string bat, string arg)
    {
#if UNITY_EDITOR
        System.Diagnostics.Process process = null;
        var currDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(dir);
        try
        {
            process = new System.Diagnostics.Process();
            process.StartInfo.FileName = bat;
            process.StartInfo.Arguments = arg;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;
            process.Close();
            Directory.SetCurrentDirectory(currDirectory);
            return code;
        }
        catch(System.Exception e)
        {
            UnityEngine.Debug.LogError($"ExecuteBat {bat} Error !! Msg : {e.Message}");
            return -1;
        }
        finally
        {
            if (process != null)
            {
                process.Close();
                process = null;
            }
            Directory.SetCurrentDirectory(currDirectory);
        }
#else
        return -1;
#endif
    }

#if UNITY_EDITOR

    /// <summary>
    /// 查找所有的资源
    /// </summary>
    /// <param name="searchInFolders">指定搜索的文件夹</param>
    /// <param name="searchOption"></param>
    /// <param name="filter"></param>
    /// <param name="directories"></param>
    /// <returns></returns>
    public static string[] FindAssets(string searchInFolders, SearchOption searchOption, string filter, bool directories)
    {
        
        string[] items;
        if (directories)
        {
            items = Directory.GetDirectories(searchInFolders, filter, searchOption);
        }
        else
        {
            items = Directory.GetFiles(searchInFolders, filter, searchOption);
        }

        // 返回结果
        return items.Where(t => !t.EndsWith(".meta")).ToArray();
    }

#endif
    
    public static string DebugBytes(byte[] data, uint len)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("[");
        for (int i = 0; i < len; i++)
        {
            sb.AppendFormat("{0} ", (int)data[i]);
        }
        sb.Append("]");
        return sb.ToString();
    }

}
