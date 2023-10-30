using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 网络管理器
/// </summary>
public class HttpManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 超时控制器
    /// </summary>
    private TimeoutController timeoutController;

    private bool _httpGetState;
    /// <summary>
    /// HttpGet 状态
    /// </summary>
    public bool HttpGetState => _httpGetState;

    private string _httpGetText;
    /// <summary>
    /// HttpGet 接受到的数据
    /// </summary>
    public string HttpGetText => _httpGetText;

    private bool _httpDownloadState;

    /// <summary>
    /// HttpDownload 状态
    /// </summary>
    public bool HttpDownloadState => _httpDownloadState;

    private string _httpDownloadText;

    /// <summary>
    /// HttpDownload 接受到的数据
    /// </summary>
    public string HttpDownloadText => _httpDownloadText;

    /// <summary>
    /// Htttp Get
    /// </summary>
    /// <param name="url">链接</param>
    /// <param name="timeout">超时</param>
    /// <returns></returns>
    private async UniTask UniHttpGet(string url, int timeout)
    {
        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequest.Get(url);
            await request.SendWebRequest().WithCancellation(timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
            if(request.isNetworkError || request.isHttpError)
            {
                _httpGetState = false;
                _httpGetText = request.error;
            }
            else
            {
                _httpGetState = true;
                _httpGetText = request.downloadHandler.text;
            }
        }
        catch(System.Exception e)
        {
            _httpGetText = e.Message;
#if UNITY_DEBUG
            Logger.Log(LogLevel.Exception, e.Message);
#endif
        }
        finally
        {
            if(request != null)
            {
                request.Dispose();
                request = null;
            }
            timeoutController.Reset();
        }
    }

    /// <summary>
    /// Http Get
    /// </summary>
    /// <param name="url">链接</param>
    /// <param name="timeout">超时</param>
    /// <param name="callback">完成回调</param>
    /// <returns></returns>
    public async UniTask CoHttpGet(string url, int timeout, System.Action<bool, string> callback = null)
    {
        await UniHttpGet(url, timeout);
        callback?.Invoke(HttpGetState, HttpGetText);
    }

    /// <summary>
    /// Http Download
    /// </summary>
    /// <param name="url">链接</param>
    /// <param name="path">路径</param>
    /// <param name="append">断点续传</param>
    /// <param name="_progress">进度回调</param>
    /// <returns></returns>
    private async UniTask UniHttpDownload(string url, string path, bool append, System.Action<float> _progress = null)
    {
        DownloadHandlerFile downloadHandler = null;
        UnityWebRequest request = null;
        try
        {
            downloadHandler = new DownloadHandlerFile(path, append);
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            await request.SendWebRequest().ToUniTask(Progress.Create<float>(_progress));
            if (request.isNetworkError || request.isHttpError)
            {
                _httpDownloadState = false;
                _httpDownloadText = request.error;
            }
            else
            {
                _httpDownloadState = true;
                _httpDownloadText = request.downloadHandler.text;
            }
        }
        catch (System.Exception e)
        {
            _httpDownloadText = e.Message;
#if UNITY_DEBUG
            Logger.Log(LogLevel.Exception, e.Message);
#endif
        }
        finally
        {
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
            if(downloadHandler != null)
            {
                downloadHandler.Dispose();
                downloadHandler = null;
            }
        }
    }

    /// <summary>
    /// Http Donwload
    /// </summary>
    /// <param name="url">链接</param>
    /// <param name="path">路径</param>
    /// <param name="append">断点续传</param>
    /// <param name="progress">进度回调</param>
    /// <param name="callback">完成回调</param>
    /// <returns></returns>
    public async UniTask CoHttpDownload(string url, string path, bool append, System.Action<float> progress = null, System.Action<bool, string> callback = null)
    {
        await UniHttpDownload(url, path, append, progress);
        callback?.Invoke(HttpDownloadState, HttpDownloadText);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        _httpGetState = false;
        _httpDownloadState = false;
        timeoutController = new TimeoutController();
        IsInitialized = true;
    }

    public void OnRelease()
    {
        timeoutController?.Reset();
        timeoutController = null;
        IsInitialized = false;
    }
}
