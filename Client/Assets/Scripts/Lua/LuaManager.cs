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
    public LuaEnv luaEnv => _luaEnv;

    private Dictionary<string, object[]> _luaDic = new Dictionary<string, object[]>();

    public object[] DoFile(string fileName) {
        if (!_luaDic.ContainsKey(fileName))
            _luaDic.Add(fileName, luaEnv.DoString(fileName));
        return _luaDic[fileName];
    }

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

    public void OnRelease() {
        if (_luaEnv != null)
            _luaEnv.Dispose();
        _luaDic.Clear();
        IsInitialized = false;
    }

    public void OnInitialize()
    {
        _luaDic.Clear();
        _luaEnv = new LuaEnv();
        _luaEnv.AddLoader(Loader);
        StartCoroutine(nameof(CoLoadScript));
    }

    public void OnDestroy()
    {
        OnRelease();
    }

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
            if (Directory.Exists(Path.Combine(Application.dataPath, Setting.RuntimeScriptBundleName)))
            {
                var filePath = Setting.RuntimeScriptBundleName + "/" + key + ".lua";
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
                UnityEngine.Debug.LogException(e);
            }
            loader.Dispose();
            loader = null;

            _luaEnv.DoString("require('preinit')");
            IsInitialized = true;
        }
    }

}
