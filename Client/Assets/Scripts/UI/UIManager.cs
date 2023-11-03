using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private static int _dynamicID = 1;

    private Camera _camera;
    public Camera UICamera => _camera;

    private Camera _noneCamera;
    public Camera NoneCamera => _noneCamera;

    private Dictionary<int, UIWindow> _windows = new Dictionary<int, UIWindow>();
    private LinkedList<UIWindow> _cacheWindows = new LinkedList<UIWindow>();
    private List<int> _creatingWindows = new List<int>();

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        StartCoroutine(nameof(CoInitialize));
    }

    /// <summary>
    /// 初始化UI相机
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoInitialize()
    {
        var go = new GameObject("UI");
        go.transform.SetParent(Global.Instance.transform, true);

        _camera = go.AddComponent<Camera>();
        _camera.backgroundColor = new Color(0, 0, 0, 0);
        _camera.clearFlags = CameraClearFlags.Depth;
        _camera.cullingMask = 1 << Setting.LAYER_UI;
        _camera.orthographic = true;
        _camera.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        _camera.nearClipPlane = -200000;
        _camera.farClipPlane = 200000;
        _camera.depth = 1;
        _camera.allowHDR = false;
        _camera.allowMSAA = false;

        if (_noneCamera == null)
        {
            var noneCamraGo = new GameObject("NoneCamera");
            noneCamraGo.transform.parent = go.transform;
            _noneCamera = noneCamraGo.AddComponent<Camera>();
            _noneCamera.clearFlags = CameraClearFlags.SolidColor;
            _noneCamera.backgroundColor = Color.black;
            _noneCamera.depth = -50;
            _noneCamera.cullingMask = 0;
            _noneCamera.allowHDR = false;
            _noneCamera.allowMSAA = false;
            _noneCamera.useOcclusionCulling = false;
        }

        yield return null;
        IsInitialized = true;
    }

    private int NewID()
    {
        return _dynamicID++;
    }

    /// <summary>
    /// 创建窗口
    /// </summary>
    /// <param name="parentId">父窗口ID</param>
    /// <param name="id">窗口ID</param>
    /// <param name="path">窗口Prefab路径</param>
    /// <param name="layer">窗口层级</param>
    /// <param name="obj">Lua对象</param>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    private IEnumerator CoCreateWindows(int parentId, int id, string path, int layer, object obj, Action<int> callback)
    {
        UIWindow win = null;
        var iter = _cacheWindows.Last;
        while(iter != null)
        {
            var current = iter.Value;
            if(current.path == path)
            {
                win = current;
                _cacheWindows.Remove(iter);
                break;
            }
            iter = iter.Previous;
        }
        if(win == null)
        {
            GameObject prefab = null;
            Resource winRes = null;
            yield return StartCoroutine(LoadPrefab(path, false, (res) => { winRes = res; }));
            if(winRes.Error != null)
            {
                winRes.Release();
            }
            else
            {
                if (_creatingWindows.Contains(id))
                {
                    prefab = Instantiate(winRes.Asset) as GameObject;
                    prefab.name = string.IsNullOrEmpty(winRes.Name) ? Path.GetFileNameWithoutExtension(winRes.Path) : winRes.Name;
                }
            }
            if (_creatingWindows.Remove(id))
            {
                GameObject winObj = InstantiateWindowObject(prefab);
                winObj.SetActive(true);

                win = Util.GetOrAddComponent<UIWindow>(winObj);
                win.transform.localPosition = Vector3.zero;
                win.transform.localScale = Vector3.one;
                win.transform.localRotation = Quaternion.identity;
                _windows.Add(id, win);
                LuaUtil.DontDestroyOnLoad(win);

                UIWindow parent = null;
                if(parentId >= 0)
                {
                    _windows.TryGetValue(parentId, out parent);
                }
                win.Create(parent, id, prefab.name, path, layer, obj);
                win.OnShow();
                if (callback != null)
                {
                    callback(id);
                }
            }
        }
        else
        {
            _creatingWindows.Remove(id);
            _windows.Add(id, win);
            UIWindow parent = null;
            if (parentId >= 0)
            {
                _windows.TryGetValue(parentId, out parent);
            }
            win.Create(parent, id, win.name, win.path, layer, obj);
            win.OnShow();
            if (callback != null)
            {
                callback(id);
            }
        }
    }

    /// <summary>
    /// 创建窗口
    /// </summary>
    /// <param name="parentId">父窗口ID</param>
    /// <param name="path">窗口Prefab路径</param>
    /// <param name="layer">窗口层级</param>
    /// <param name="obj">Lua对象</param>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    public int CreateWindow(int parentId, string path, int layer, object obj, Action<int> callback)
    {
        var id = NewID();
        _creatingWindows.Add(id);
        StartCoroutine(CoCreateWindows(parentId, id, path, layer, obj, callback));
        return id;
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="id">窗口ID</param>
    /// <param name="isDestroy">是否删除窗口</param>
    public void DestroyWindow(int id, bool isDestroy)
    {
        if (!_creatingWindows.Remove(id) && _windows.ContainsKey(id))
        {
            UIWindow window = null;
            if(_windows.TryGetValue(id, out window))
            {
                _windows.Remove(id);
                window.OnHide(() => 
                {
                    window.Destory();
                    using(var e = _windows.GetEnumerator())
                    {
                        while (e.MoveNext()) { 
                            if(e.Current.Value.parent == window)
                            {
                                e.Current.Value.parent = null;
                            }
                        }
                    }
                    if (isDestroy)
                    {
                        window.gameObject.SetActive(false);
                        GameObject.DestroyImmediate(window.gameObject);
                    }
                    else
                    {
                        CacheWindow(window);
                    }
                });
            }
        }
    }

    /// <summary>
    /// 缓存窗口
    /// </summary>
    /// <param name="win"></param>
    private void CacheWindow(UIWindow win)
    {
        _cacheWindows.AddLast(win);
        while(_cacheWindows.Count > Constant.CacheUICount)
        {
            var head = _cacheWindows.First;
            _cacheWindows.RemoveFirst();
            if(head.Value != null)
            {
                GameObject.DestroyImmediate(head.Value.gameObject);
            }
        }
    }

    /// <summary>
    /// 清除所有缓存的窗口
    /// </summary>
    public void ClearCache()
    {
        var iter = _cacheWindows.First;
        while (iter != null)
        {
            var next = iter.Next;
            _cacheWindows.Remove(iter);
            if (iter.Value != null)
            {
                iter.Value.gameObject.SetActive(false);
                GameObject.DestroyImmediate(iter.Value.gameObject);
            }
            else
            {
                Debug.LogError("清理UI缓存失败: NULL");
            }
            iter = next;
        }
    }

    /// <summary>
    /// 实例化窗口对象
    /// </summary>
    /// <param name="prefab">窗口对象</param>
    /// <returns></returns>
    public GameObject InstantiateWindowObject(GameObject prefab)
    {
        var canvas = Util.GetOrAddComponent<Canvas>(prefab);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = UICamera;
        canvas.planeDistance = 0;
        return prefab;
    }

    public void OnRelease()
    {
        if(_camera != null)
        {
            _camera.enabled = false;
        }
        IsInitialized = false;
        var list = new List<int>();
        using (var e = _windows.GetEnumerator())
        {
            while (e.MoveNext())
            {
                list.Add(e.Current.Key);
            }
        }
        for (var i = 0; i < list.Count; ++i)
        {
            DestroyWindow(list[i], true);
        }
        _windows.Clear();
        _creatingWindows.Clear();
    }

}

public partial class UIManager
{
    /// <summary>
    /// 加载Prefab
    /// </summary>
    /// <param name="path">Prefab路径</param>
    /// <param name="isAsync">是否异步</param>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    public IEnumerator LoadPrefab(string path, bool isAsync, Action<Resource> callback)
    {
        var loader = new ResLoader(FileUtil.CombinePaths("UI", path + ".prefab"), null, isAsync);
        yield return loader;
        var res = loader.Res;
        res.Retain();
        loader.Dispose();
        loader = null;
        callback(res);
    }

}
