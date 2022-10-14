using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneManager : MonoBehaviour, IManager
{
    public enum LoadingState
    {
        None,
        Ready,
        Loading,
        Finished,
        LoadDone,
    }

    private Resource _lastLoadRes = null;
    private Resource _addiveLoadRes = null;

    public bool IsInitialized { get; set; }
    public System.Action<LoadingState, float> SceneLoading;

    private Camera _lastMainCamera;

    public void OnInitialize()
    {
        IsInitialized = true;
    }

    public void OnRelease()
    {
        IsInitialized = false;
    }

    public void LoadScene(string name, System.Action<LoadingState, float> progress)
    {
        StopAllCoroutines();
        StartCoroutine(CoLoadScene(name, progress));
    }

    IEnumerator CoLoadScene(string name, System.Action<LoadingState, float> progress)
    {
        if(progress != null)
        {
            progress(LoadingState.Ready, 0);
            yield return null;
        }
        if(SceneLoading!= null)
        {
            SceneLoading(LoadingState.Ready, 0);
        }
        if(_lastLoadRes != null)
        {
            _lastLoadRes.Release();
            _lastLoadRes = null;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
        Resources.UnloadUnusedAssets();
        yield return null;
        yield return StartCoroutine(CoRealLoadScene(name, progress, false));
        yield return null;
        if (Global.Instance.OnSceneChanged != null)
        {
            Global.Instance.OnSceneChanged.Invoke();
        }
        Global.Instance.UIManager.UICamera.cullingMask = 1 << Setting.LAYER_UI;
        if (progress != null)
        {
            progress(LoadingState.Finished, 1);
        }
        if (SceneLoading != null)
        {
            SceneLoading(LoadingState.Finished, 1);
        }
        if(Camera.main && Camera.main != Global.Instance.UIManager.NoneCamera)
        {
            _lastMainCamera = Camera.main;
        }
        else
        {
            _lastMainCamera = null;
        }
        if (progress != null)
        {
            progress(LoadingState.LoadDone, 1);
        }
        if (SceneLoading != null)
        {
            SceneLoading(LoadingState.LoadDone, 1);
        }
    }

    IEnumerator CoRealLoadScene(string name, System.Action<LoadingState, float> progress, bool isAddive)
    {
        if (Setting.Config.useAssetBundle)
        {
            Resource tmpRes = null;
            Global.Instance.ResManager.LoadAsync(string.Format("Scenes/{0}.unity", name), (res) =>
            {
                tmpRes = res;
                tmpRes.Retain();
            });
            while (tmpRes == null)
            {
                yield return null;
            }
            AsyncOperation request = null;
            if (isAddive)
            {
                _addiveLoadRes = tmpRes;
                UnityEngine.SceneManagement.SceneManager.LoadScene(name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            else
            {
                _lastLoadRes = tmpRes;
                request = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
            }
            if(request != null)
            {
                while (!request.isDone)
                {
                    if(progress != null)
                    {
                        progress(LoadingState.Loading, request.progress / 0.9f);
                    }
                    if (SceneLoading != null)
                    {
                        SceneLoading(LoadingState.Loading, request.progress / 0.9f);
                    }
                    yield return null;
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            AsyncOperation request = null;

            if (isAddive)
            {
                var parameters = new LoadSceneParameters() { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.None };
                EditorSceneManager.LoadSceneInPlayMode(System.IO.Path.Combine(Setting.EditorBundlePath, string.Format("Scenes/{0}.unity", name)), parameters);
            }
            else
            {
                var parameters = new LoadSceneParameters() { loadSceneMode = LoadSceneMode.Single, localPhysicsMode = LocalPhysicsMode.None };
                request = EditorSceneManager.LoadSceneAsyncInPlayMode(System.IO.Path.Combine(Setting.EditorBundlePath, string.Format("Scenes/{0}.unity", name)), parameters);
            }

            if (request != null)
            {
                while (!request.isDone)
                {
                    if (progress != null)
                    {
                        progress(LoadingState.Loading, request.progress / 0.9f);
                    }
                    if (SceneLoading != null)
                    {
                        SceneLoading(LoadingState.Loading, request.progress / 0.9f);
                    }
                    yield return null;
                }
            }
#endif
        }
    }

}
