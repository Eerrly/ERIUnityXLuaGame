using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp, GenComment]
public class LuaUtil
{
    /// <summary>
    /// 加载删除列表
    /// </summary>
    private static List<UnityEngine.Object> dontDestroyOnLoadObjs = new List<UnityEngine.Object>();

    /// <summary>
    /// 清除不加载删除列表
    /// </summary>
    public static void ClearDontDestroyObjs()
    {
        dontDestroyOnLoadObjs.Clear();
    }

    /// <summary>
    /// 添加对象到不加载删除列表中
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isDontDestroy"></param>
    public static void DontDestroyOnLoad(UnityEngine.Object obj, bool isDontDestroy = true)
    {
        if (isDontDestroy) GameObject.DontDestroyOnLoad(obj);
        if (obj != null) dontDestroyOnLoadObjs.Add(obj);
    }

    /// <summary>
    /// 对象是否为空
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static bool IsNull(object obj)
    {
        return obj == null;
    }

    /// <summary>
    /// 创建窗口
    /// </summary>
    /// <param name="parentId">父窗口ID</param>
    /// <param name="path">Prefab路径</param>
    /// <param name="layer">窗口层级</param>
    /// <param name="args">窗口Lua对象</param>
    /// <param name="callback">回调</param>
    /// <returns>窗口ID</returns>
    public static int CreateWindow(int parentId, string path, int layer, LuaTable args, LuaFunction callback)
    {
        return Global.Instance.UIManager.CreateWindow(parentId, path, layer, args, (id) => 
        {
            callback.Call(id);
        });
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="id">窗口ID</param>
    /// <param name="destroy">是否删除窗口</param>
    public static void DestroyWindow(int id, bool destroy)
    {
        Global.Instance.UIManager.DestroyWindow(id, destroy);
    }

    /// <summary>
    /// 清理UI缓存
    /// </summary>
    public static void ClearUICache()
    {
        Global.Instance.UIManager.ClearCache();
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="scene">场景路径</param>
    /// <param name="callback">回调</param>
    public static void LoadScene(string scene, LuaFunction callback)
    {
        Global.Instance.SceneManager.LoadScene(scene, (state, progress) =>
        {
            callback.Call((int)state, progress);
        });
    }

    /// <summary>
    /// 热更
    /// </summary>
    /// <param name="url">远端链接地址</param>
    /// <param name="o">Lua对象</param>
    /// <param name="callback">回调</param>
    public static void Patching(string url, object o, LuaFunction callback = null)
    {
        Global.Instance.PatchingManager.CoPatching(url, callback, o);
    }

    /// <summary>
    /// 设置战斗内自己的ID
    /// </summary>
    /// <param name="id"></param>
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
