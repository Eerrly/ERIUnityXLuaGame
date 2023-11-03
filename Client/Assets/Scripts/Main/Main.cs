using System.Collections;
using UnityEngine;

public class Main : MonoBehaviour
{

    void Start()
    {
        Global.Instance.OnGameStart.AddListener(LuaStart);
        Global.Instance.OnPatchingDone.AddListener(OnPatchingDone);
        Global.Instance.Run();
    }

    void LuaStart()
    {
        Global.Instance.LuaManager.luaEnv.DoString(@"
require 'main/main'
");
    }

    void OnPatchingDone()
    {
        Global.Instance.Shutdown();
        Global.Instance.Run();
    }

}