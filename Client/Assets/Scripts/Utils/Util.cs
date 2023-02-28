using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Util
{
    private static List<Transform> _setGameObjectLayerList = new List<Transform>();

    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T t = null;
        if(null != go)
        {
            t = go.GetComponent<T>();
            if(t == null)
            {
                t = go.AddComponent<T>();
            }
        }
        return t;
    }

    public static void SetGameObjectLayer(GameObject go, int layer, bool includeChildren)
    {
        if(null != go && go.layer != layer)
        {
            if (includeChildren)
            {
                _setGameObjectLayerList.Clear();
                go.GetComponentsInChildren<Transform>(true, _setGameObjectLayerList);
                foreach (var igo in _setGameObjectLayerList)
                {
                    igo.gameObject.layer = layer;
                }
            }
            else
            {
                go.layer = layer;
            }
        }
    }

    public static void InvokeAttributeCall(object obj, Type classType, bool parengInherit, Type methodType, bool methodInherit)
    {
        if (null != obj)
        {
            var types = obj.GetType().Assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; ++i)
            {
                if (types[i].IsDefined(classType, parengInherit))
                {
                    var methods = types[i].GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    for (var j = 0; j < methods.Length; ++j)
                    {
                        if (methods[j].IsDefined(methodType, methodInherit))
                        {
                            methods[j].Invoke(null, null);
                        }
                    }
                }
            }
        }
    }

    public static string MD5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);
        return MD5(bytes);
    }

    public static string MD5(byte[] bytes)
    {
        using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        {
            var result = md5.ComputeHash(bytes);
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                builder.Append(result[i].ToString("x2"));
            }
            var value = builder.ToString();
            return value;
        }
    }

    public static void SaveConfig(object data, string fileName)
    {
        string json = Regex.Unescape(JsonUtility.ToJson(data));
        File.WriteAllText(FileUtil.CombinePaths(Setting.EditorResourcePath, Setting.EditorConfigPath, fileName), json, Encoding.UTF8);
    }

    public static T LoadConfig<T>(string fileName)
    {
        string path = FileUtil.CombinePaths(Setting.EditorConfigPath, fileName);
        string configPath = path.Substring(0, path.LastIndexOf("."));
        try
        {
            string json = System.Text.ASCIIEncoding.Default.GetString(Resources.Load<TextAsset>(configPath).bytes);
            T t = JsonUtility.FromJson<T>(json);
            return t;
        }
        catch(Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        var obj = default(T);
        if(null == obj)
        {
            obj = System.Activator.CreateInstance<T>();
        }
        return obj;
    }

    public static string GetAssetAbsolutePath(string assetPath)
    {
        string path = Application.dataPath;
        path = path.Substring(0, path.Length - 6);
        path += assetPath;
        return path;
    }

    public static bool SetPathToSelection(string path)
    {
#if UNITY_EDITOR
        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (obj != null)
        {
            Selection.objects = new UnityEngine.Object[] { obj };
            return true;
        }
#endif
        return false;
    }

    unsafe public static uint HashPath(string input)
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

    public static bool CheckAndLogError(object o, string error)
    {
        if (o == null || (o is bool && !(bool)o))
        {
            Debug.LogError(error);
            return false;
        }
        return true;
    }

    public static int ExecuteBat(string dir, string bat, string arg)
    {
#if UNITY_EDITOR
        var currDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(dir);
        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
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
            Logger.Log(LogLevel.Exception, e.Message);
            return -1;
        }
        finally
        {
            Directory.SetCurrentDirectory(currDirectory);
        }
#else
        return -1;
#endif
    }

}
