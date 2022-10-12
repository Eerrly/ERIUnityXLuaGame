using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        Global.Instance.OnGameStart.AddListener(LuaStart);
        Global.Instance.Run();
        //StartCoroutine(nameof(CoLoadStart));
    }

    IEnumerator CoLoadStart()
    {
        var loader = new ResLoader("UI/A.prefab", null, false);
        yield return loader;
        loader.Res.GetGameObjectInstance();
    }

    void LuaStart()
    {
        Global.Instance.LuaManager.luaEnv.DoString(@"
require 'main/main'
");
    }
}