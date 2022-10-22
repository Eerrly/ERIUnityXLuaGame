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

    public static int CreateWindow(string path, int layer, int property, LuaTable args, LuaFunction callback)
    {
        return Global.Instance.UIManager.CreateWindow(-1, path, layer, property, args, (id) =>
        {
            callback.Call(id);
        });
    }

    public static int CreateWindow(int parentId, string path, int layer, int property, LuaTable args, LuaFunction callback)
    {
        return Global.Instance.UIManager.CreateWindow(parentId, path, layer, property, args, (id) => 
        {
            callback.Call(id);
        });
    }

    public static void DestroyWindow(int id, bool destroy)
    {
        Global.Instance.UIManager.DestroyWindow(id, destroy);
    }

    public static void LoadScene(string scene, LuaFunction callback)
    {
        Global.Instance.SceneManager.LoadScene(scene, (state, progress) =>
        {
            callback.Call((int)state, progress);
        });
    }

    public static void HttpGet(string url, int timeout, LuaFunction callback = null)
    {
        Global.Instance.PatchingManager.CoHttpGet(url, timeout, (state, text) =>
        {
            callback.Call(state, text);
        });
    }

    public static void HttpDownload(string url, string path, LuaFunction progress = null, LuaFunction callback = null)
    {
        Global.Instance.PatchingManager.CoHttpDownload(url, path, (pro) => { progress.Call(progress); }, (state, text) =>
        {
            callback.Call(state, text);
        });
    }

}
