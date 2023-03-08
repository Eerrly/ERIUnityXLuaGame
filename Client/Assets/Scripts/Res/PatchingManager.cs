using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    public string vBytesFilePath;
    public string rcBytesFilePath;

    private string _remoteUrl;
    private XLua.LuaFunction _callback;

    private string remoteVersion = "";
    private bool bSaveRc = false;
    private List<ManifestItem> downloadList = null;

    public async void CoPatching(string remoteUrl, XLua.LuaFunction callback, object o)
    {
        FileUtil.CreateDirectory(Setting.CacheBundleRoot);

        _remoteUrl = remoteUrl;
        _callback = callback;

        downloadList = new List<ManifestItem>();
        var localMd5Map = new Dictionary<uint, string>();
        var localVersionText = File.Exists(vBytesFilePath) ? File.ReadAllText(vBytesFilePath) : "";
        bSaveRc = string.IsNullOrEmpty(localVersionText);

        var remoteVerPath = FileUtil.CombinePaths(remoteUrl, "v.bytes");
        await Global.Instance.HttpManager.CoHttpGet(FileUtil.CombinePaths(remoteUrl, "v.bytes"), 5, (state, text) =>
        {
            if (!state)
            {
                Logger.Log(LogLevel.Error, $"CoPatching CoHttpGet {remoteVerPath} Error!!! Msg : {text}");
                return;
            }
            else
            {
                Logger.Log(LogLevel.Info, $"CoPatching CoHttpGet Success!! Msg : {text}");
                remoteVersion = text.Split(',')[0];
                bSaveRc = bSaveRc || localVersionText.Split(',')[0] != remoteVersion;
            }
        });
        if (bSaveRc)
        {
            if (File.Exists(rcBytesFilePath))
            {
                var localConfJson = System.Text.ASCIIEncoding.Default.GetString(File.ReadAllBytes(rcBytesFilePath));
                var localConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(localConfJson);
                foreach (var item in localConf.items)
                {
                    localMd5Map.Add(item.hash, item.md5);
                }
            }

            var tmpLocalRcFilePath = rcBytesFilePath + ".tmp";
            var remoteRcPath = FileUtil.CombinePaths(remoteUrl, remoteVersion, "rc.bytes");
            await Global.Instance.HttpManager.CoHttpDownload(remoteRcPath, tmpLocalRcFilePath, false, null, (state, text) =>
            {
                if (!state)
                {
                    Logger.Log(LogLevel.Error, $"CoPatching CoHttpDownload {remoteRcPath} Error!!! Msg : {text}");
                    return;
                }
            });
            if (File.Exists(tmpLocalRcFilePath))
            {
                var tmpConfJson = System.Text.ASCIIEncoding.Default.GetString(File.ReadAllBytes(tmpLocalRcFilePath));
                var tmpConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(tmpConfJson);
                foreach (var item in tmpConf.items)
                {
                    var tmpMd5 = "";
                    if (!localMd5Map.TryGetValue(item.hash, out tmpMd5) || tmpMd5 != item.md5)
                    {
                        FileUtil.DeleteFile(FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s"));
                        downloadList.Add(item);
                    }
                }
            }
            while (downloadList.Count > 0)
            {
                for (int i = downloadList.Count - 1; i >= 0; --i)
                {
                    var remoteFilePath = FileUtil.CombinePaths(remoteUrl, remoteVersion, downloadList[i].hash + ".s");
                    var localFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, downloadList[i].hash + ".s");
                    await Global.Instance.HttpManager.CoHttpDownload(
                        remoteFilePath,
                        localFilePath,
                        true,
                        (progress) =>
                        {
                            Logger.Log(LogLevel.Info, $"CoPatching CoHttpDownload {remoteFilePath} Progress : {progress}");
                            _callback?.Call(0, "donwload", progress);
                        },
                        (state, text) =>
                        {
                            if (!state)
                            {
                                Logger.Log(LogLevel.Error, $"CoPatching CoHttpDownload {remoteFilePath} Error!!! Msg : {text}");
                            }
                            else
                            {
                                downloadList.RemoveAt(i);
                            }
                        });
                }
            }
            File.Copy(tmpLocalRcFilePath, rcBytesFilePath, true);
            FileUtil.DeleteFile(tmpLocalRcFilePath);
        }
        callback?.Call(o, "done");
    }

    public void CoPatchingCallBack(bool httpState, string httpText)
    {
        var localVersion = System.IO.File.Exists(vBytesFilePath) ? System.IO.File.ReadAllText(vBytesFilePath) : "";
        if (!httpState)
        {
            Logger.Log(LogLevel.Error, $"CoPatching {_remoteUrl} Error!! Msg : {httpText}");
            return;
        }
        if (!string.IsNullOrEmpty(httpText) && localVersion != httpText)
        {
            Logger.Log(LogLevel.Info, $"CoPatching Get Success!! Msg : {httpText}");
            _callback?.Call(0, "got", httpState, httpText);
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
        bSaveRc = false;
        downloadList.Clear();
        downloadList = null;
    }
}
