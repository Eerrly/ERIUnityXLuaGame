using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : MonoBehaviour, IManager
{
    public enum Property
    {
        NoCache = 0x1,
        SyncLoad = 0x2,
        ShowScene = 0x8,
        IgnoreShowScene = 0x10,
        UseDefaultPopupAnimation = 0x100,
        UseDefaultNormalAnimation = 0x200,
        UseCustomAnimation = 0x400,
    }

    public bool IsInitialized { get; set; }

    private static int _dynamicID = 0;

    private Camera _camera;
    public Camera UICamera => _camera;

    private Camera _noneCamera;
    public Camera NoneCamera => _noneCamera;

    private Dictionary<int, UIWindow> _windows = new Dictionary<int, UIWindow>();
    private LinkedList<UIWindow> _cacheWindows = new LinkedList<UIWindow>();
    private Dictionary<int, Property> _creatingWindows = new Dictionary<int, Property>();

    public void OnInitialize()
    {
        StartCoroutine(nameof(CoInitialize));
    }

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

    private IEnumerator CoCreateWindows(int parentId, int id, string path, int layer, int property, object obj, Action<int> callback)
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
                if (_creatingWindows.ContainsKey(id))
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
                win.Create(parent, id, prefab.name, path, this, layer, property, obj);
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
            win.Create(parent, id, win.name, win.path, this, layer, property, obj);
            win.OnShow();
            if (callback != null)
            {
                callback(id);
            }
        }
    }

    public int CreateWindow(int parentId, string path, int layer, int property, object obj, Action<int> callback)
    {
        var id = NewID();
        _creatingWindows.Add(id, (Property)property);
        StartCoroutine(CoCreateWindows(parentId, id, path, layer, property, obj, callback));
        return id;
    }

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
