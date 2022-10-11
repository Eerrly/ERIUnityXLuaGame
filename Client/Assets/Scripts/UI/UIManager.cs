using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : Singleton<UIManager>
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

    private static int _dynamicID = 0;

    private Camera _camera;
    public Camera UICamera => _camera;

    private Dictionary<int, UIWindow> _windows = new Dictionary<int, UIWindow>();
    private LinkedList<UIWindow> _cacheWindows = new LinkedList<UIWindow>();
    private Dictionary<int, Property> _creatingWindows = new Dictionary<int, Property>();

    public override void OnInitialize()
    {
        var go = new GameObject("UI");
        _camera = go.AddComponent<Camera>();
        _camera.backgroundColor = new Color(0, 0, 0, 0);
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
                win = Util.GetOrAddComponent<UIWindow>(prefab);
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
                if(callback != null)
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
            if(callback != null)
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
