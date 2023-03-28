using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 热更管理器
/// </summary>
public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    public string vBytesFilePath;
    public string rcBytesFilePath;

    private XLua.LuaFunction _callback;

    private string localVersionText = "";
    private string remoteVersionText = "";
    private bool bSaveRc = false;
    private List<ManifestItem> downloadList = null;

    /// <summary>
    /// 热更
    /// </summary>
    /// <param name="remoteUrl">远端链接地址</param>
    /// <param name="callback">Lua回调函数</param>
    /// <param name="o">Lua对象</param>
    public async void CoPatching(string remoteUrl, XLua.LuaFunction callback, object o)
    {
        FileUtil.CreateDirectory(Setting.CacheBundleRoot);

        _callback = callback;

        downloadList = new List<ManifestItem>();
        var localMd5Map = new Dictionary<uint, string>();
        localVersionText = File.Exists(vBytesFilePath) ? File.ReadAllText(vBytesFilePath) : "";
        bSaveRc = string.IsNullOrEmpty(localVersionText);

        var remoteVerPath = FileUtil.CombinePaths(remoteUrl, "v.bytes");
        await Global.Instance.HttpManager.CoHttpGet(remoteVerPath, 5, (state, text) =>
        {
            if (!state)
            {
                Logger.Log(LogLevel.Error, $"CoPatching CoHttpGet {remoteVerPath} Error!!! Msg : {text}");
            }
            else
            {
                Logger.Log(LogLevel.Info, $"CoPatching CoHttpGet Success!! Msg : {text}");
                remoteVersionText = text.Split(',')[0];
                bSaveRc = bSaveRc || localVersionText != remoteVersionText;
            }
        });
        if (bSaveRc || true)
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

            var remoteVersion = remoteVersionText.Split(',')[0];
            var tmpLocalRcFilePath = rcBytesFilePath + ".tmp";
            var remoteRcPath = FileUtil.CombinePaths(remoteUrl, remoteVersion, "rc.bytes");
            await Global.Instance.HttpManager.CoHttpDownload(remoteRcPath, tmpLocalRcFilePath, false, null, (state, text) =>
            {
                if (!state)
                {
                    Logger.Log(LogLevel.Error, $"CoPatching CoHttpDownload {remoteRcPath} Error!!! Msg : {text}");
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

                var remoteFilePath = "";
                var localFilePath = "";
                while (downloadList.Count > 0)
                {
                    for (int i = downloadList.Count - 1; i >= 0; --i)
                    {
                        remoteFilePath = FileUtil.CombinePaths(remoteUrl, remoteVersion, downloadList[i].hash + ".s");
                        localFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, downloadList[i].hash + ".s");
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

                File.WriteAllText(vBytesFilePath, remoteVersionText);
            }
        }
        callback?.Call(o, "done");
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
        _callback = null;
        bSaveRc = false;
        downloadList.Clear();
        downloadList = null;
    }
}
