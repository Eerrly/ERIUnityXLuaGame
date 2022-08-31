using System.IO;
using XLua;
using System.Collections.Generic;
using UnityEngine;

public class LuaManager : Singleton<LuaManager> {

    private LuaEnv _luaEnv;
    public LuaEnv luaEnv {
        get {
            if (_luaEnv == null) {
                _luaEnv = new LuaEnv();
                _luaEnv.AddLoader(MyLuaCustomLoader);
                _luaEnv.DoString("require('preinit')");
            }
            return _luaEnv;
        }
    }

    private Dictionary<string, object[]> _luaDic = new Dictionary<string, object[]>();

    byte[] MyLuaCustomLoader(ref string fileName) {
        string filePath = Path.Combine(Application.dataPath.Replace("/Assets", ""), Setting.EditorScriptRoot) + "/" + fileName + ".lua";
        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }
        return default(byte[]);
    }

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
                self.Set<string, int>(e.Current.Key, e.Current.Value);
            }
        }
        self.Set<string, LuaTable>("ID", id);
    }

    public override void OnRelease() {
        if (_luaEnv != null)
            _luaEnv.Dispose();
        _luaDic.Clear();
    }

    public override void OnInitialize()
    {
        _luaDic.Clear();
    }

    public void OnDestroy()
    {
        OnRelease();
    }

}
