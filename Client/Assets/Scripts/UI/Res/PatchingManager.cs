using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 是否热更的状态
/// </summary>
enum CanDownloadState
{
    Confirm = 1,
    Cancel,
}

/// <summary>
/// 热更管理器
/// </summary>
public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 本地V文件路径
    /// </summary>
    private string _vBytesFilePath;

    /// <summary>
    /// 本地RC文件路径
    /// </summary>
    private string _rcBytesFilePath;

    private XLua.LuaFunction _callback;

    /// <summary>
    /// 本地版本号
    /// </summary>
    private string _localVersionText = "";

    /// <summary>
    /// 远端版本号
    /// </summary>
    private string _remoteVersionText = "";

    /// <summary>
    /// 是否要保存RC文件
    /// </summary>
    private bool _bSaveRc = false;

    /// <summary>
    /// 热更需要下载的资源文件列表
    /// </summary>
    private List<ManifestItem> _downloadList = null;

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

        _downloadList = new List<ManifestItem>();
        var patchingResult = false;
        var localMd5Map = new Dictionary<uint, string>();
        _localVersionText = File.Exists(_vBytesFilePath) ? File.ReadAllText(_vBytesFilePath) : "";
        _bSaveRc = string.IsNullOrEmpty(_localVersionText);

        var remoteVerPath = FileUtil.CombinePaths(remoteUrl, "v.bytes");
        // 获取远端V文件版本号，对比本地V文件版本号比较
        await Global.Instance.HttpManager.CoHttpGet(remoteVerPath, 5, (state, text) =>
        {
            if (!state)
            {
#if UNITY_DEBUG
                Logger.Log(LogLevel.Error, $"热更网络获取 {remoteVerPath} 错误!!! 错误信息 : {text}");
#endif
            }
            else
            {
#if UNITY_DEBUG
                Logger.Log(LogLevel.Info, $"热更网络获取成功!! 获取结果 : {text}");
#endif
                _remoteVersionText = text.Split(',')[0];
                _bSaveRc = _bSaveRc || _localVersionText != _remoteVersionText;
            }
        });
        if (_bSaveRc)
        {
            if (File.Exists(_rcBytesFilePath))
            {
                var localConfJson = System.Text.Encoding.Default.GetString(File.ReadAllBytes(_rcBytesFilePath));
                var localConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(localConfJson);
                foreach (var item in localConf.items)
                {
                    localMd5Map.Add(item.hash, item.md5);
                }
            }

            var remoteVersion = _remoteVersionText.Split(',')[0];
            var tmpLocalRcFilePath = _rcBytesFilePath + ".tmp";
            var remoteRcPath = FileUtil.CombinePaths(remoteUrl, remoteVersion, "rc.bytes");
            // 下载RC文件
            await Global.Instance.HttpManager.CoHttpDownload(remoteRcPath, tmpLocalRcFilePath, false, null, (state, text) =>
            {
                if (!state)
                {
#if UNITY_DEBUG
                    Logger.Log(LogLevel.Error, $"热更网络下载 {remoteRcPath} 错误!!! 错误信息 : {text}");
#endif
                }
            });
            if (File.Exists(tmpLocalRcFilePath))
            {
                // 从RC文件中拿到需要下载的资源列表，比较MD5，如果不同则加入到下载集合中
                var tmpConfJson = System.Text.Encoding.Default.GetString(File.ReadAllBytes(tmpLocalRcFilePath));
                var tmpConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(tmpConfJson);
                foreach (var item in tmpConf.items)
                {
                    if (!localMd5Map.TryGetValue(item.hash, out var tmpMd5) || tmpMd5 != item.md5)
                    {
                        FileUtil.DeleteFile(FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s"));
                        _downloadList.Add(item);
                    }
                }

                // 等待Lua回应是否同意下载热更
                if(_downloadList.Count > 0)
                {
                    await CallLuaPatchDownloadInfo(o);
                }

                // 开始下载热更
                var remoteFilePath = "";
                var localFilePath = "";
                while (_downloadList.Count > 0)
                {
                    for (int i = _downloadList.Count - 1; i >= 0; --i)
                    {
                        remoteFilePath = FileUtil.CombinePaths(remoteUrl, remoteVersion, _downloadList[i].hash + ".s");
                        localFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, _downloadList[i].hash + ".s");
                        await Global.Instance.HttpManager.CoHttpDownload(
                            remoteFilePath,
                            localFilePath,
                            true,
                            (progress) =>
                            {
#if UNITY_DEBUG
                                Logger.Log(LogLevel.Info, $"热更网络下载 {remoteFilePath} 进度 : {progress}");
#endif
                                _callback?.Call(o, "donwload", progress);
                            },
                            (state, text) =>
                            {
                                if (!state)
                                {
#if UNITY_DEBUG
                                    Logger.Log(LogLevel.Error, $"热更网络下载 {remoteFilePath} 错误!!! 错误信息 : {text}");
#endif
                                }
                                else
                                {
                                    _downloadList.RemoveAt(i);
                                }
                            });
                    }
                }

                // 替换新RC文件
                File.Copy(tmpLocalRcFilePath, _rcBytesFilePath, true);
                FileUtil.DeleteFile(tmpLocalRcFilePath);

                // 替换新V文件
                File.WriteAllText(_vBytesFilePath, _remoteVersionText);
                patchingResult = true;
            }
        }
        if (patchingResult)
        {
            callback?.Call(o, "done");
            Global.Instance.onPatchingDone?.Invoke();
        }
    }

    /// <summary>
    /// 等待Lua返回是否同意下载
    /// </summary>
    /// <param name="o">Lua对象</param>
    /// <returns></returns>
    private System.Collections.IEnumerator CallLuaPatchDownloadInfo(object o)
    {
        var bytes = 0;
        foreach (var file in _downloadList)
        {
            bytes += file.size;
        }
        _callback?.Call(o, "setdownload", _downloadList.Count, bytes);

        var canDownload = true;
        while (true)
        {
            var state = _callback?.Func<object, string, int>(o, "candownload");
            if (state == (int)CanDownloadState.Confirm)
            {
                canDownload = true;
                break;
            }
            else if (state == (int)CanDownloadState.Cancel)
            {
                canDownload = false;
                break;
            }
            yield return null;
        }
        if (!canDownload)
        {
            _callback?.Call(o, "error");
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        _vBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "v.bytes");
        _rcBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "rc.bytes");
        IsInitialized = true;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void OnRelease()
    {
        IsInitialized = false;
        _callback = null;
        _bSaveRc = false;
        _downloadList?.Clear();
        _downloadList = null;
    }
}
