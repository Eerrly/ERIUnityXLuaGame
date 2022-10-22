using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        Global.Instance.OnGameStart.AddListener(LuaStart);
        Global.Instance.Run();
    }

    void LuaStart()
    {
        Global.Instance.LuaManager.luaEnv.DoString(@"
require 'main/main'
");
    }
}