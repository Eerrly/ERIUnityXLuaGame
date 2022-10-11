using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        //StartCoroutine(nameof(CoLoadStart));
        LuaStart();
    }

    IEnumerator CoLoadStart()
    {
        var loader = new ResLoader("UI/A.prefab", null, false);
        yield return loader;
        loader.Res.GetGameObjectInstance();
    }

    void LuaStart()
    {
        LuaManager.Instance.luaEnv.DoString("require 'main/main'");
    }
}