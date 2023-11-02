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

        // ############################# DEBUG #############################
        AssetBundle[] cacheBundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
        for (int i = 0; i < cacheBundles.Length; i++)
        {
            Debug.Log($"cacheBundles > {cacheBundles[i].name}");
        }
        Debug.Log(ResUtil.Dump());

        Global.Instance.Run();
        LuaStart();
    }

}