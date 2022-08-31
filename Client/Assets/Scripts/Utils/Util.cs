using System;
using UnityEngine;
public class Util
{
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

}
