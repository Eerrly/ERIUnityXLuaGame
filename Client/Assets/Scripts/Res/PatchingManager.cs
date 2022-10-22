using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private TimeoutController timeoutController;
    private bool _httpGetState;
    private string _httpText;

    public bool HttpGetState => _httpGetState;
    public string HttpText => _httpText;

    private async UniTask UniHttpGet(string url, int timeout)
    {
        var request = UnityWebRequest.Get(new Uri(url));
        await request.SendWebRequest().WithCancellation(timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
        _httpGetState = !request.isHttpError && !request.isNetworkError;
        _httpText = _httpGetState ? request.downloadHandler.text : request.error;
        request.Dispose();
        request = null;
        timeoutController.Reset();
    }

    public async void CoHttpGet(string url, int timeout, System.Action<bool, string> callback = null)
    {
        await UniHttpGet(url, timeout);
        if (callback != null)
        {
            callback(HttpGetState, HttpText);
        }
    }

    private async UniTask UniHttpDownload(string url, string path, System.Action<float> _progress = null)
    {
        var progress = Progress.Create<float>(_progress);
        var downloadHandler = new DownloadHandlerFile(path, true);
        var request = new UnityWebRequest(new Uri(url), UnityWebRequest.kHttpVerbGET, downloadHandler, null);
        await request.SendWebRequest().ToUniTask(progress);
        _httpGetState = !request.isHttpError && !request.isNetworkError;
        _httpText = _httpGetState ? request.downloadHandler.text : request.error;
        request.Dispose();
        request = null;
        downloadHandler.Dispose();
    }

    public async void CoHttpDownload(string url, string path, System.Action<float> progress = null, System.Action<bool, string> callback = null)
    {
        await UniHttpDownload(url, path, progress);
        if (callback != null)
        {
            callback(HttpGetState, HttpText);
        }
    }

    public void OnInitialize()
    {
        _httpGetState = false;
        timeoutController = new TimeoutController();
        IsInitialized = true;
    }

    public void OnRelease()
    {
        timeoutController.Reset();
        timeoutController = null;
        IsInitialized = false;
    }
}
