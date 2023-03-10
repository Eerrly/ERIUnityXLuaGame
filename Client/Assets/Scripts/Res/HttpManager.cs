using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class HttpManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private TimeoutController timeoutController;
    private bool _httpState;
    private string _httpText;
    private byte[] _httpBytes;

    public bool HttpState => _httpState;
    public string HttpText => _httpText;
    public byte[] HttpBytes => _httpBytes;

    private async UniTask UniHttpGet(string url, int timeout)
    {
        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequest.Get(url);
            await request.SendWebRequest().WithCancellation(timeoutController.Timeout(TimeSpan.FromSeconds(timeout)));
#if UNITY_2019_4_37
            _httpState = !request.isNetworkError && !request.isHttpError;
            _httpText = _httpState ? request.downloadHandler.text : request.error;
#else
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
#endif
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

    public async UniTask CoHttpGet(string url, int timeout, System.Action<bool, string> callback = null)
    {
        await UniHttpGet(url, timeout);
        callback?.Invoke(HttpState, HttpText);
    }

    private async UniTask UniHttpDownload(string url, string path, bool append, System.Action<float> _progress = null)
    {
        DownloadHandlerFile downloadHandler = null;
        UnityWebRequest request = null;
        try
        {
            downloadHandler = new DownloadHandlerFile(path, append);
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            await request.SendWebRequest().ToUniTask(Progress.Create<float>(_progress));
#if UNITY_2019_4_37
            _httpState = !request.isNetworkError && !request.isHttpError;
            _httpText = _httpState ? request.downloadHandler.text : request.error;
            _httpBytes = _httpState ? request.downloadHandler.data : null;
#else
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
                    _httpBytes = request.downloadHandler.data;
                    break;
            }
#endif
        }
        catch (System.Exception e)
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

    public async UniTask CoHttpDownload(string url, string path, bool append, System.Action<float> progress = null, System.Action<bool, string> callback = null)
    {
        await UniHttpDownload(url, path, append, progress);
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
