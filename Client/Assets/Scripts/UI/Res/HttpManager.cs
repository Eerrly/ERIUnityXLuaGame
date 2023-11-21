using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class HttpManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 超时控制器
    /// </summary>
    private TimeoutController _timeoutController;

    /// <summary>
    /// HttpGet 状态
    /// </summary>
    private bool HttpGetState { get; set; }

    /// <summary>
    /// HttpGet 接受到的数据
    /// </summary>
    private string HttpGetText { get; set; }

    /// <summary>
    /// HttpDownload 状态
    /// </summary>
    private bool HttpDownloadState { get; set; }

    /// <summary>
    /// HttpDownload 接受到的数据
    /// </summary>
    private string HttpDownloadText { get; set; }

    private string GetResponseText(UnityWebRequest response)
    {
        if (response.downloadHandler == null)
            return string.Empty;
        if (response.downloadHandler is DownloadHandlerBuffer)
            return response.downloadHandler.text;
        return $"无响应文本 : {response.downloadHandler.GetType().Name}";
    }

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
            await request.SendWebRequest().WithCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
            if(request.isNetworkError || request.isHttpError)
            {
                HttpGetState = false;
                HttpGetText = request.error;
            }
            else
            {
                HttpGetState = true;
                HttpGetText = GetResponseText(request);
            }
        }
        catch(System.Exception e)
        {
            HttpGetText = e.Message;
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
            _timeoutController.Reset();
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
    /// <param name="progress">进度回调</param>
    /// <returns></returns>
    private async UniTask UniHttpDownload(string url, string path, bool append, System.Action<float> progress = null)
    {
        DownloadHandlerFile downloadHandler = null;
        UnityWebRequest request = null;
        try
        {
            downloadHandler = new DownloadHandlerFile(path, append);
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            await request.SendWebRequest().ToUniTask(Progress.Create<float>(progress));
            if (request.isNetworkError || request.isHttpError)
            {
                HttpDownloadState = false;
                HttpDownloadText = request.error;
            }
            else
            {
                HttpDownloadState = true;
                HttpDownloadText = GetResponseText(request);
            }
        }
        catch (System.Exception e)
        {
            HttpDownloadText = e.Message;
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
        HttpGetState = false;
        HttpDownloadState = false;
        _timeoutController = new TimeoutController();
        IsInitialized = true;
    }

    public void OnRelease()
    {
        _timeoutController?.Reset();
        _timeoutController = null;
        IsInitialized = false;
    }
}
