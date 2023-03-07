using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    public string vBytesFilePath;
    public string rcBytesFilePath;

    private string _remoteUrl;
    private XLua.LuaFunction _callback;

    public async void CoPatching(string remoteUrl, XLua.LuaFunction callback, object o)
    {
        _remoteUrl = remoteUrl;
        _callback = callback;

        FileUtil.CreateDirectory(Setting.CacheBundleRoot);
        _callback?.Call(o, "ready");
        await Global.Instance.HttpManager.CoHttpGet(FileUtil.CombinePaths(remoteUrl, "v.bytes"), 5, CoPatchingCallBack);
    }

    public void CoPatchingCallBack(bool httpState, string httpText)
    {
        var localVersion = System.IO.File.Exists(vBytesFilePath) ? System.IO.File.ReadAllText(vBytesFilePath) : "";
        if (!httpState)
        {
            Logger.Log(LogLevel.Error, string.Format($"CoPatching {_remoteUrl} Error!! Msg : {httpText}"));
            return;
        }
        if (!string.IsNullOrEmpty(httpText) && localVersion != httpText)
        {
            Logger.Log(LogLevel.Info, string.Format($"CoPatching Get Success!! Msg : {httpText}"));
            _callback?.Call(0, "got");
        }
    }

    public void OnInitialize()
    {
        vBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "v.bytes");
        rcBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "rc.bytes");
        IsInitialized = true;
    }

    public void OnRelease()
    {
        IsInitialized = false;
        _remoteUrl = string.Empty;
        _callback = null;
    }
}
