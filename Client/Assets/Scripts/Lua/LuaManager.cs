using System.IO;
using XLua;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LuaManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private Dictionary<string, byte[]> _codes = new Dictionary<string, byte[]>();
    private LuaEnv _luaEnv;
    /// <summary>
    /// Lua与C#的桥接对象
    /// </summary>
    public LuaEnv luaEnv => _luaEnv;

    /// <summary>
    /// 将所有预制好的组件与Lua对象进行绑定
    /// </summary>
    /// <param name="view">Lua执行器</param>
    /// <param name="name2id">Name-Id的字典</param>
    /// <param name="instance">Lua对象</param>
    public void BindInstance(LuaBehaviour view, Dictionary<string, int> name2id, object instance)
    {
        LuaTable self = (LuaTable)instance;
        self.Set<string, LuaBehaviour>("View", view);
        var id = luaEnv.NewTable();
        using(var e = name2id.GetEnumerator())
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
        if (_luaEnv != null)
            _luaEnv.Dispose();
        IsInitialized = false;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        _codes.Clear();
        _luaEnv = new LuaEnv();
        _luaEnv.AddLoader(Loader);
        StartCoroutine(nameof(CoLoadScript));
    }

    public void OnDestroy()
    {
        OnRelease();
    }

    /// <summary>
    /// 自定义Lua脚本加载器
    /// </summary>
    /// <param name="path">Lua脚本路径</param>
    /// <returns>字节流</returns>
    public byte[] Loader(ref string path)
    {
        var key = path.ToLower().Replace(".", "/");
        if (Setting.Config.useAssetBundle)
        {
            byte[] code;
            if(_codes.TryGetValue(key, out code))
            {
                return code;
            }
            return null;
        }
        else
        {
            // 在不使用AB包加载资源时，直接加载对应的Lua文件
            if (Directory.Exists(Setting.EditorLuaScriptRoot))
            {
                var filePath = Setting.EditorLuaScriptRoot + "/" + key.Replace(".", "/") + ".lua";
                if (File.Exists(filePath))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        byte[] result = null;
                        var ok = true;
                        try
                        {
                            result = File.ReadAllBytes(filePath);
                            break;
                        }
                        catch
                        {
                            ok = false;
                        }
                        if (ok)
                        {
                            return result;
                        }
                    }
                    return File.ReadAllBytes(filePath);
                }
            }
            return null;
        }
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
                resource.LoadScript(_codes);
            }
            catch (System.Exception e)
            {
                Logger.Log(LogLevel.Exception, e.Message);
            }
            loader.Dispose();
            loader = null;

            _luaEnv.DoString("require('preinit')");
            IsInitialized = true;
        }
        else
        {
            _luaEnv.DoString("require('preinit')");
            IsInitialized = true;
        }
    }

}
