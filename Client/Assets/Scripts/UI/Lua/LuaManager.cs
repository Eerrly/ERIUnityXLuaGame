using System.IO;
using XLua;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LuaManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private readonly Dictionary<string, byte[]> _codes = new Dictionary<string, byte[]>();

    /// <summary>
    /// Lua与C#的桥接对象
    /// </summary>
    public LuaEnv luaEnv { get; private set; }

    /// <summary>
    /// 将所有预制好的组件与Lua对象进行绑定
    /// </summary>
    /// <param name="view">Lua执行器</param>
    /// <param name="name2ID">Name-Id的字典</param>
    /// <param name="instance">Lua对象</param>
    public void BindInstance(LuaBehaviour view, Dictionary<string, int> name2ID, object instance)
    {
        var self = (LuaTable)instance;
        self.Set<string, LuaBehaviour>("View", view);
        var id = luaEnv.NewTable();
        using(var e = name2ID.GetEnumerator())
        {
            while (e.MoveNext())
            {
                id.Set<string, int>(e.Current.Key, e.Current.Value);
            }
        }
        self.Set<string, LuaTable>("ID", id);
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void OnRelease() {
        _codes.Clear();
        luaEnv?.Dispose();
        IsInitialized = false;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        _codes.Clear();
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(Loader);
        StartCoroutine(nameof(CoLoadScript));
    }

    /// <summary>
    /// 自定义Lua脚本加载器
    /// </summary>
    /// <param name="path">Lua脚本路径</param>
    /// <returns>字节流</returns>
    private byte[] Loader(ref string path)
    {
        var key = path.ToLower().Replace(".", "/");
        if (Setting.Config.useAssetBundle)
        {
            if(_codes.TryGetValue(key, out var code))
            {
                return code;
            }
            return null;
        }

        // 在不使用AB包加载资源时，直接加载对应的Lua文件
        if (!Directory.Exists(Setting.EditorLuaScriptRoot)) return null;
            
        var filePath = Setting.EditorLuaScriptRoot + "/" + key.Replace(".", "/") + ".lua";
        if (!File.Exists(filePath)) return null;
            
        return File.ReadAllBytes(filePath);
    }

    /// <summary>
    /// 加载Lua脚本资源
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoLoadScript()
    {
        if (Setting.Config.useAssetBundle)
        {
            var loader = new ResLoader(Setting.RuntimeScriptBundleName, null, false);
            yield return loader;
            var resource = (Resource)loader.Current;
            try
            {
                resource?.LoadScript(_codes);
            }
            catch (System.Exception e)
            {
                Logger.Log(LogLevel.Exception, e.Message);
            }
            loader.Dispose();
            loader = null;

            luaEnv.DoString("require('preinit')");
            IsInitialized = true;
        }
        else
        {
            luaEnv.DoString("require('preinit')");
            IsInitialized = true;
        }
    }

}
