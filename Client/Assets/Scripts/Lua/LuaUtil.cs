using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp, GenComment]
public class LuaUtil
{
    private static List<UnityEngine.Object> dontDestroyOnLoadObjs = new List<UnityEngine.Object>();

    public static void ClearDontDestroyObjs()
    {
        dontDestroyOnLoadObjs.Clear();
    }

    public static void DontDestroyOnLoad(UnityEngine.Object obj, bool isDontDestroy = true)
    {
        if (isDontDestroy) GameObject.DontDestroyOnLoad(obj);
        if (obj != null) dontDestroyOnLoadObjs.Add(obj);
    }

    public static bool IsNull(object obj)
    {
        return obj == null;
    }

    public static object LoadRes(string path, string name = null, bool isAsync = false)
    {
        var loader = new ResLoader(path, name, isAsync);
        return loader.Current;
    }

}
