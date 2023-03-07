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

    public static int CreateWindow(int parentId, string path, int layer, LuaTable args, LuaFunction callback)
    {
        return Global.Instance.UIManager.CreateWindow(parentId, path, layer, args, (id) => 
        {
            callback.Call(id);
        });
    }

    public static void DestroyWindow(int id, bool destroy)
    {
        Global.Instance.UIManager.DestroyWindow(id, destroy);
    }

    public static void ClearUICache()
    {
        Global.Instance.UIManager.ClearCache();
    }

    public static void LoadScene(string scene, LuaFunction callback)
    {
        Global.Instance.SceneManager.LoadScene(scene, (state, progress) =>
        {
            callback.Call((int)state, progress);
        });
    }

    public static void Patching(string url, object o, LuaFunction callback = null)
    {
        Global.Instance.PatchingManager.CoPatching(url, callback, o);
    }

    public static void SetSelfPlayerId(int id)
    {
        switch (id)
        {
            case 1:
                BattleConstant.SelfID = 0;
                break;
            case 2:
                BattleConstant.SelfID = 1;
                break;
        }
    }

}
