using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    private static int _dynamicID = 1;

    public Camera UICamera { get; private set; }

    public Camera NoneCamera { get; private set; }

    private readonly Dictionary<int, UIWindow> _windows = new Dictionary<int, UIWindow>();
    private readonly LinkedList<UIWindow> _cacheWindows = new LinkedList<UIWindow>();
    private readonly List<int> _creatingWindows = new List<int>();

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

        UICamera = go.AddComponent<Camera>();
        UICamera.backgroundColor = new Color(0, 0, 0, 0);
        UICamera.clearFlags = CameraClearFlags.Depth;
        UICamera.cullingMask = 1 << Setting.LAYER_UI;
        UICamera.orthographic = true;
        UICamera.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UICamera.nearClipPlane = -200000;
        UICamera.farClipPlane = 200000;
        UICamera.depth = 1;
        UICamera.allowHDR = false;
        UICamera.allowMSAA = false;

        if (NoneCamera == null)
        {
            var noneCameraGo = new GameObject("NoneCamera");
            noneCameraGo.transform.parent = go.transform;
            NoneCamera = noneCameraGo.AddComponent<Camera>();
            NoneCamera.clearFlags = CameraClearFlags.SolidColor;
            NoneCamera.backgroundColor = Color.black;
            NoneCamera.depth = -50;
            NoneCamera.cullingMask = 0;
            NoneCamera.allowHDR = false;
            NoneCamera.allowMSAA = false;
            NoneCamera.useOcclusionCulling = false;
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
                    if(prefab != null) prefab.name = string.IsNullOrEmpty(winRes.Name) ? Path.GetFileNameWithoutExtension(winRes.Path) : winRes.Name;
                }
            }
            if (_creatingWindows.Remove(id))
            {
                var winObj = InstantiateWindowObject(prefab);
                winObj.SetActive(true);

                win = Util.GetOrAddComponent<UIWindow>(winObj);
                var transform1 = win.transform;
                transform1.localPosition = Vector3.zero;
                transform1.localScale = Vector3.one;
                transform1.localRotation = Quaternion.identity;
                _windows.Add(id, win);
                LuaUtil.DontDestroyOnLoad(win);

                UIWindow parent = null;
                if(parentId >= 0)
                {
                    _windows.TryGetValue(parentId, out parent);
                }

                if (prefab != null) win.Create(parent, id, prefab.name, path, layer, obj);
                win.OnShow();
                callback?.Invoke(id);
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
            callback?.Invoke(id);
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
            if (!_windows.TryGetValue(id, out var window)) return;
            
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
                    GameObject o;
                    (o = window.gameObject).SetActive(false);
                    GameObject.DestroyImmediate(o);
                }
                else
                {
                    CacheWindow(window);
                }
            });
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
                GameObject o;
                (o = iter.Value.gameObject).SetActive(false);
                GameObject.DestroyImmediate(o);
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
    private GameObject InstantiateWindowObject(GameObject prefab)
    {
        var canvas = Util.GetOrAddComponent<Canvas>(prefab);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = UICamera;
        canvas.planeDistance = 0;
        return prefab;
    }

    public void OnRelease()
    {
        if(UICamera != null)
        {
            UICamera.enabled = false;
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
        foreach (var t in list)
        {
            DestroyWindow(t, true);
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
    private IEnumerator LoadPrefab(string path, bool isAsync, Action<Resource> callback)
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
