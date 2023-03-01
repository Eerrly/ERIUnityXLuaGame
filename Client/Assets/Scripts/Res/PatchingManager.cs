using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private TimeoutController timeoutController;
    private bool _httpState;
    private string _httpText;

    public bool HttpState => _httpState;
    public string HttpText => _httpText;

    private async UniTask UniHttpGet(string url, int timeout)
    {
        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequest.Get(new Uri(url));
            await request.SendWebRequest().WithCancellation(timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
            switch (request.result)
            {
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    _httpState = false;
                    _httpText = request.error;
                    break;
                case UnityWebRequest.Result.Success:
                    _httpState = true;
                    _httpText = request.downloadHandler.text;
                    break;
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
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

    public async void CoHttpGet(string url, int timeout, System.Action<bool, string> callback = null)
    {
        await UniHttpGet(url, timeout);
        if (callback != null)
        {
            callback(HttpState, HttpText);
        }
    }

    private async UniTask UniHttpDownload(string url, string path, System.Action<float> _progress = null)
    {
        DownloadHandlerFile downloadHandler = null;
        UnityWebRequest request = null;
        try
        {
            downloadHandler = new DownloadHandlerFile(path, true);
            request = new UnityWebRequest(new Uri(url), UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            await request.SendWebRequest().ToUniTask(Progress.Create<float>(_progress));
            switch (request.result)
            {
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    _httpState = false;
                    _httpText = request.error;
                    break;
                case UnityWebRequest.Result.Success:
                    _httpState = true;
                    _httpText = request.downloadHandler.text;
                    break;
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
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

    public async void CoHttpDownload(string url, string path, System.Action<float> progress = null, System.Action<bool, string> callback = null)
    {
        await UniHttpDownload(url, path, progress);
        callback?.Invoke(HttpState, HttpText);
    }

    public void OnInitialize()
    {
        _httpState = false;
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
