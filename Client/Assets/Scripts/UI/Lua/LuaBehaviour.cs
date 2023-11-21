using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XLua;
using System;

[LuaCallCSharp, GenComment]
public partial class LuaBehaviour : MonoBehaviour
{
    /// <summary>
    /// UI事件
    /// </summary>
    internal struct Event
    {
        public int EventId;
        public int ID;
    }

    /// <summary>
    /// UI事件ID
    /// </summary>
    internal enum EventID
    {
        ButtonClicked = 0,
        SliderValueChanged = 1,
    }

    /// <summary>
    /// 节点组件ID索引对应组件
    /// </summary>
    private UIBehaviour[] _id2UI;

    /// <summary>
    /// 节点名对应组件ID
    /// </summary>
    private Dictionary<string, int> _name2ID;
    private Dictionary<int, Coroutine> _cacheCoroutines = null;
    private List<Resource> _loadedCache = null;
    private object _instance;
    private LuaBehaviour _root;
    private List<LuaBehaviour> _children;
    private List<Event> _events;
    private int _index;

    /// <summary>
    /// 当前Lua执行器运行时所使用到的协程列表
    /// </summary>
    public Dictionary<int, Coroutine> CacheCoroutines => _cacheCoroutines ?? (_cacheCoroutines = new Dictionary<int, Coroutine>());

    /// <summary>
    /// 当前Lua执行器运行时所加载资源的缓存列表
    /// </summary>
    public List<Resource> LoadedCache => _loadedCache ?? (_loadedCache = new List<Resource>());

    private void Awake()
    {
        _name2ID = new Dictionary<string, int>();
        _children = new List<LuaBehaviour>();
        _events = new List<Event>();

        var uis = new List<Transform>();
        transform.GetComponentsInChildren<Transform>(true, uis);
        _id2UI = new UIBehaviour[uis.Count + 1];
        var id = 1;
        foreach (var t in uis)
        {
            var tName = t.name;
            if (string.IsNullOrEmpty(tName) || tName[0] != '@') continue;
            
            tName = tName.Substring(1, tName.Length - 1);
            if (_name2ID.ContainsKey(tName))
                continue;
            var ui = t.GetComponent<UIBehaviour>() ?? t.gameObject.AddComponent<LuaBehaviourNode>();
            // 子节点名对应组件ID
            _name2ID.Add(tName, id);
            // 子节点组件ID索引组件
            _id2UI[id++] = ui;
        }
    }

    /// <summary>
    /// 实例化
    /// </summary>
    /// <param name="instance">Lua对象</param>
    /// <param name="root">父节点Lua执行器</param>
    /// <param name="index">索引</param>
    [NoComment]
    public void Initialize(object instance, LuaBehaviour root = null, int index = 0)
    {
        this._instance = instance;
        this._root = root;
        this._index = index;
        if(this._root != null)
        {
            this._root._children.Add(this);
        }
        if(this._instance != null)
        {
            BindInstance(this._instance);
        }
    }

    /// <summary>
    /// 绑定Lua
    /// </summary>
    /// <param name="instance">Lua对象</param>
    [NoComment]
    public void BindInstance(object instance)
    {
        this._instance = instance;
        Global.Instance.LuaManager.BindInstance(this, _name2ID, this._instance);
    }

    [NoComment]
    public int NameToID(string tName)
    {
        int id = -1;
        _name2ID.TryGetValue(tName, out id);
        return id;
    }

    /// <summary>
    /// 尝试获取任意一个继承自MonoBehaviour的组件
    /// </summary>
    /// <typeparam name="T">继承自MonoBehaviour的组件类</typeparam>
    /// <param name="id">组件ID</param>
    /// <param name="view">组件</param>
    /// <param name="addIfNotExist">如果没有是否添加一个</param>
    /// <returns></returns>
    [NoComment]
    private bool TryGetControl<T>(int id, out T view, bool addIfNotExist = false) where T : MonoBehaviour
    {
        view = null;
        UIBehaviour behaviour = null;
        if(_id2UI == null || id >= _id2UI.Length || id < 0 || _id2UI[id] == null)
        {
            return false;
        }
        behaviour = _id2UI[id];
        view = behaviour as T;
        if (view != null) return true;
        
        view = behaviour.GetComponent<T>();
        if (view != null) return true;

        if (!addIfNotExist) return false;
        
        view = behaviour.gameObject.AddComponent<T>();
        return true;
    }

    /// <summary>
    /// 执行Lua函数
    /// </summary>
    /// <param name="callBack">Lua函数</param>
    /// <param name="args">参数</param>
    /// <returns></returns>
    [NoComment]
    public object[] InvokeLuaCallback(LuaFunction callBack, params object[] args)
    {
        if(callBack == null)
        {
            return null;
        }
        if (_root)
        {
            if(_root._instance != null)
            {
                var mixArgs = MixArgs(args, _root._instance, _index, _instance);
                return callBack.Call(mixArgs);
            }
        }
        else
        {
            if(_instance != null)
            {
                var mixArgs = MixArgs(args, _instance);
                return callBack.Call(mixArgs);
            }
        }
        return default(object[]);
    }

    /// <summary>
    /// 组合2个数组参数
    /// </summary>
    /// <param name="args">原数组</param>
    /// <param name="insert">插入数组</param>
    /// <returns></returns>
    [NoComment]
    public object[] MixArgs(object[] args, params object[] insert)
    {
        var result = new object[args.Length + insert.Length];
        Array.Copy(insert, result, insert.Length);
        Array.Copy(args, 0, result, insert.Length, args.Length);
        return result;
    }

    /// <summary>
    /// 获取子节点
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="root">父节点</param>
    /// <returns></returns>
    public object GetChild(int id, LuaBehaviour root = null)
    {
        LuaBehaviour child = null;
        if(TryGetControl(id, out child))
        {
            if(child._instance == null) {
                child.Initialize(Global.Instance.LuaManager.luaEnv.NewTable(), root);
            }
            return child._instance;
        }
        else
        {
            Debug.Log($"获取子物体的LuaBehaviour错误! id:{id}");
            return null;
        }
    }

    /// <summary>
    /// 绑定各种UI事件
    /// </summary>
    /// <param name="eventId">UI事件ID</param>
    /// <param name="id">组件ID</param>
    /// <param name="callBack">触发函数</param>
    public void BindEvent(int eventId, int id, LuaFunction callBack)
    {
        for (var i = _events.Count - 1; i >= 0; i--)
        {
            if(_events[i].EventId == eventId && _events[i].ID == id)
            {
                _events.RemoveAt(i);
            }
        }
        _events.Add(new Event() { EventId = eventId, ID = id });
        switch ((EventID)eventId)
        {
            case EventID.ButtonClicked:
                if(TryGetControl(id, out Button btn))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(()=> {
                        InvokeLuaCallback(callBack);
                    });
                }
                break;
            case EventID.SliderValueChanged:
                if(TryGetControl(id, out Slider slider))
                {
                    slider.onValueChanged.RemoveAllListeners();
                    slider.onValueChanged.AddListener((value) =>
                    {
                        InvokeLuaCallback(callBack, value);
                    });
                }
                break;
        }
    }

    /// <summary>
    /// Lua执行器释放
    /// </summary>
    [NoComment]
    public void Release()
    {
        foreach (var t in LoadedCache)
        {
            t.Release();
        }
        LoadedCache.Clear();
        if (_id2UI != null)
        {
            foreach (var t in _id2UI)
            {
                if (t == null) continue;
                
                var mono = t.GetComponents<MonoBehaviour>();
                foreach (var t1 in mono)
                {
                    t1.StopAllCoroutines();
                }
            }
        }
        CacheCoroutines.Clear();
        var table = (LuaTable)_instance;
        if(Global.Instance != null && Global.Instance.LuaManager.luaEnv != null && table != null)
        {
            table.Dispose();
        }
        _instance = null;
        if (this)
        {
            StopAllCoroutines();
        }
    }

}

public partial class LuaBehaviour : MonoBehaviour
{
    /// <summary>
    /// 物体是否激活
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <returns></returns>
    public bool IsActive(int id)
    {
        return TryGetControl(id, out UIBehaviour c) && c.gameObject.activeSelf;
    }

    /// <summary>
    /// 设置CanvasGroup的Alpha
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="alpha">Alpha</param>
    public void SetCanvasGroupAlpha(int id, float alpha)
    {
        if (!TryGetControl(id, out MonoBehaviour c)) return;
        
        var group = c.GetComponent<CanvasGroup>();
        if (group)
        {
            group.alpha = alpha;
            group.blocksRaycasts = alpha > 0;
        }
    }

    /// <summary>
    /// 设置物体激活
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="active">是否激活</param>
    public void SetActive(int id, bool active)
    {
        if (!TryGetControl(id, out UIBehaviour c)) return;
        
        if (c.gameObject.activeSelf != active)
        {
            c.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 设置父节点
    /// </summary>
    /// <param name="id1">当前节点组件ID</param>
    /// <param name="id2">父节点组件ID</param>
    /// <param name="worldPositionStays">保持与以前相同的世界空间位置、旋转和缩放</param>
    public void SetParent(int id1, int id2, bool worldPositionStays)
    {
        if (TryGetControl(id1, out UIBehaviour c1) && TryGetControl(id2, out UIBehaviour c2))
        {
            c1.transform.SetParent(c2.transform, worldPositionStays);
        }
    }

    /// <summary>
    /// 获取UI组件开关
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <returns></returns>
    public bool IsEnable(int id)
    {
        return TryGetControl(id, out UIBehaviour c) && c.enabled;
    }

    /// <summary>
    /// 设置UI组件开关
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="enable">开关</param>
    public void SetEnable(int id, bool enable)
    {
        if (TryGetControl(id, out UIBehaviour c))
        {
            c.enabled = enable;
        }
    }

    /// <summary>
    /// 设置Position
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    public void SetPosition(int id, float x, float y, float z)
    {
        if (TryGetControl(id, out MonoBehaviour c))
        {
            c.transform.localPosition = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// 设置旋转
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    public void SetRotation(int id, float x, float y, float z)
    {
        if (TryGetControl(id, out MonoBehaviour c))
        {
            c.transform.localRotation = Quaternion.Euler(new Vector3(x, y, z));
        }
    }

    /// <summary>
    /// 设置Scale
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    public void SetScale(int id, float x, float y, float z)
    {
        if (TryGetControl(id, out MonoBehaviour c))
        {
            c.transform.localScale = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// 设置AnchoredPosition
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    public void SetAnchoredPosition(int id, float x, float y, float z)
    {
        if (!TryGetControl(id, out MonoBehaviour c)) return;
        
        var rectTransform = c.gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// 设置激活状态
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="interactable">是否激活</param>
    public void SetInteractable(int id, bool interactable)
    {
        if (TryGetControl(id, out Selectable s))
        {
            s.interactable = interactable;
        }
    }

    /// <summary>
    /// 是否为激活状态
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <returns>是否为激活状态</returns>
    public bool IsInteractable(int id)
    {
        if (TryGetControl(id, out Selectable s))
        {
            return s.interactable;
        }
        return false;
    }

    /// <summary>
    /// 设置按钮组件开关
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="enable">开关</param>
    public void SetButtonEnable(int id, bool enable)
    {
        if (TryGetControl(id, out Button c))
        {
            c.interactable = enable;
        }
    }

    /// <summary>
    /// 获取文本
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <returns>文本</returns>
    public string GetText(int id)
    {
        if (TryGetControl(id, out Text c))
        {
            return c.text;
        }
        return "";
    }

    /// <summary>
    /// 设置文本
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="text">文本</param>
    public void SetText(int id, string text)
    {
        if (TryGetControl(id, out Text c))
        {
            c.text = text;
        }
    }

    /// <summary>
    /// 设置字体大小
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="size">字体大小</param>
    public void SetFontSize(int id, int size)
    {
        if (TryGetControl(id, out Text c))
        {
            c.fontSize = size;
        }
    }

    /// <summary>
    /// 设置进度条的值
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    public void SetSliderValue(int id, float value)
    {
        if(TryGetControl(id, out Slider c))
        {
            c.value = value;
        }
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="dir">目录</param>
    /// <param name="spriteName">图片名</param>
    /// <param name="resetSize">是否使用默认尺寸</param>
    /// <param name="sizeRatio">尺寸缩放比例</param>
    public void SetImage(int id, string dir, string spriteName, bool resetSize = false, float sizeRatio = 1)
    {
        if (!TryGetControl(id, out Image c)) return;
        
        if (CacheCoroutines.TryGetValue(id, out var co))
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
            CacheCoroutines.Remove(id);
        }

        co = StartCoroutine(CoSetImage(id, c, dir, spriteName, resetSize, sizeRatio));
        CacheCoroutines.Add(id, co);
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="c">Image</param>
    /// <param name="sprite">Sprite</param>
    /// <param name="cache">资源缓存</param>
    /// <param name="resetSize">是否使用默认尺寸</param>
    /// <param name="sizeRatio">尺寸缩放比例</param>
    private void SetImage(Image c, Sprite sprite, Resource cache, bool resetSize, float sizeRatio)
    {
        LoadedCache.Add(cache);
        c.sprite = sprite;
        if (resetSize)
        {
            c.SetNativeSize();
        }
        if (Math.Abs(sizeRatio - 1) > 0.0001f)
        {
            var size = c.rectTransform.sizeDelta;
            c.rectTransform.sizeDelta = size * sizeRatio;
        }
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="id">加载资源的协程ID</param>
    /// <param name="c">Image</param>
    /// <param name="dir">目录</param>
    /// <param name="spriteName">图片资源名</param>
    /// <param name="resetSize">是否使用默认尺寸</param>
    /// <param name="sizeRatio">尺寸缩放比例</param>
    /// <returns></returns>
    private IEnumerator CoSetImage(int id, Image c, string dir, string spriteName, bool resetSize, float sizeRatio)
    {
        var loader = new ResLoader(dir, spriteName, false);
        yield return loader;
        var resource = (Resource)loader.Current;
        if (resource == null) yield break;
        
        var sprite = resource.GetSprite();
        resource.Retain();
        loader.Dispose();
        loader = null;
        CacheCoroutines.Remove(id);
        SetImage(c, sprite, resource, resetSize, sizeRatio);

        yield break;
    }

}
