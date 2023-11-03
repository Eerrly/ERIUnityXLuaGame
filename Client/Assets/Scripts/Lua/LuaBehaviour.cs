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
        public int eventId;
        public int id;
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
    private UIBehaviour[] id2ui;

    /// <summary>
    /// 节点名对应组件ID
    /// </summary>
    private Dictionary<string, int> name2id;
    private Dictionary<int, Coroutine> _cacheCoroutines = null;
    private List<Resource> _loadedCache = null;
    private object instance;
    private LuaBehaviour root;
    private List<LuaBehaviour> children;
    private List<Event> events;
    private int index;

    /// <summary>
    /// 当前Lua执行器运行时所使用到的协程列表
    /// </summary>
    public Dictionary<int, Coroutine> CacheCoroutines
    {
        get
        {
            if (_cacheCoroutines == null)
            {
                _cacheCoroutines = new Dictionary<int, Coroutine>();
            }
            return _cacheCoroutines;
        }
    }

    /// <summary>
    /// 当前Lua执行器运行时所加载资源的缓存列表
    /// </summary>
    public List<Resource> LoadedCache
    {
        get
        {
            if (_loadedCache == null)
            {
                _loadedCache = new List<Resource>();
            }
            return _loadedCache;
        }
    }

    private void Awake()
    {
        name2id = new Dictionary<string, int>();
        children = new List<LuaBehaviour>();
        events = new List<Event>();

        var uis = new List<Transform>();
        transform.GetComponentsInChildren<Transform>(true, uis);
        id2ui = new UIBehaviour[uis.Count + 1];
        var id = 1;
        for (int i = 0; i < uis.Count; i++)
        {
            var name = uis[i].name;
            if(!string.IsNullOrEmpty(name) && name[0] == '@')
            {
                name = name.Substring(1, name.Length - 1);
                if (name2id.ContainsKey(name))
                {
                    continue;
                }
                var ui = uis[i].GetComponent<UIBehaviour>();
                if(ui == null)
                {
                    ui = uis[i].gameObject.AddComponent<LuaBehaviourNode>();
                }
                // 子节点名对应组件ID
                name2id.Add(name, id);
                // 子节点组件ID索引组件
                id2ui[id++] = ui;
            }
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
        this.instance = instance;
        this.root = root;
        this.index = index;
        if(this.root != null)
        {
            this.root.children.Add(this);
        }
        if(this.instance != null)
        {
            BindInstance(this.instance);
        }
    }

    /// <summary>
    /// 绑定Lua
    /// </summary>
    /// <param name="instance">Lua对象</param>
    [NoComment]
    public void BindInstance(object instance)
    {
        this.instance = instance;
        Global.Instance.LuaManager.BindInstance(this, name2id, this.instance);
    }

    [NoComment]
    public int NameToID(string name)
    {
        int id = -1;
        name2id.TryGetValue(name, out id);
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
    public bool TryGetControl<T>(int id, out T view, bool addIfNotExist = false) where T : MonoBehaviour
    {
        view = null;
        UIBehaviour behaviour = null;
        if(id2ui == null || id >= id2ui.Length || id < 0 || id2ui[id] == null)
        {
            return false;
        }
        behaviour = id2ui[id];
        view = behaviour as T;
        if(view == null)
        {
            view = behaviour.GetComponent<T>();
            if(view == null)
            {
                if (addIfNotExist)
                {
                    view = behaviour.gameObject.AddComponent<T>();
                    return true;
                }
                return false;
            }
            return true;
        }
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
        if (root)
        {
            if(root.instance != null)
            {
                var _args = MixArgs(args, root.instance, index, instance);
                return callBack.Call(_args);
            }
        }
        else
        {
            if(instance != null)
            {
                var _args = MixArgs(args, instance);
                return callBack.Call(_args);
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
        object[] result = new object[args.Length + insert.Length];
        Array.Copy(insert, result, insert.Length);
        Array.Copy(args, 0, result, insert.Length, args.Length);
        return result;
    }

    public object GetChild(int id, LuaBehaviour root = null)
    {
        LuaBehaviour child = null;
        if(TryGetControl(id, out child))
        {
            if(child.instance == null) {
                child.Initialize(Global.Instance.LuaManager.luaEnv.NewTable(), root);
            }
            return child.instance;
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
        for (int i = events.Count - 1; i >= 0; i--)
        {
            if(events[i].eventId == eventId && events[i].id == id)
            {
                events.RemoveAt(i);
            }
        }
        events.Add(new Event() { eventId = eventId, id = id });
        switch ((EventID)eventId)
        {
            case EventID.ButtonClicked:
                Button btn = null;
                if(TryGetControl(id, out btn))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(()=> {
                        InvokeLuaCallback(callBack);
                    });
                }
                break;
            case EventID.SliderValueChanged:
                Slider slider = null;
                if(TryGetControl(id, out slider))
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
        for (int i = 0; i < LoadedCache.Count; i++)
        {
            LoadedCache[i].Release();
        }
        LoadedCache.Clear();
        if (id2ui != null)
        {
            for (int i = 0; i < id2ui.Length; i++)
            {
                if(id2ui[i] != null)
                {
                    var monos = id2ui[i].GetComponents<MonoBehaviour>();
                    for (int j = 0; j < monos.Length; j++)
                    {
                        monos[j].StopAllCoroutines();
                    }
                }
            }
        }
        CacheCoroutines.Clear();
        LuaTable table = (LuaTable)instance;
        if(Global.Instance != null && Global.Instance.LuaManager.luaEnv != null && table != null)
        {
            table.Dispose();
        }
        instance = null;
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
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            return c.gameObject.activeSelf;
        }
        return false;
    }

    /// <summary>
    /// 设置CanvasGroup的Alpha
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="alpha">Alpha</param>
    public void SetCanvasGroupAlpha(int id, float alpha)
    {
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            CanvasGroup group = c.GetComponent<CanvasGroup>();
            if (group)
            {
                group.alpha = alpha;
                group.blocksRaycasts = alpha > 0;
            }
        }
    }

    /// <summary>
    /// 设置物体激活
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="active">是否激活</param>
    public void SetActive(int id, bool active)
    {
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            if (c.gameObject.activeSelf != active)
            {
                c.gameObject.SetActive(active);
            }
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
        UIBehaviour c1 = null;
        UIBehaviour c2 = null;
        if (TryGetControl(id1, out c1) && TryGetControl(id2, out c2))
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
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            return c.enabled;
        }
        return false;
    }

    /// <summary>
    /// 设置UI组件开关
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="enabled">开关</param>
    public void SetEnable(int id, bool enabled)
    {
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            c.enabled = enabled;
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
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
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
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
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
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
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
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            RectTransform rectTransform = c.gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector3(x, y, z);
            }
        }
    }

    /// <summary>
    /// 设置激活状态
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="interactable">是否激活</param>
    public void SetInteractable(int id, bool interactable)
    {
        Selectable s = null;
        if (TryGetControl(id, out s))
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
        Selectable s = null;
        if (TryGetControl(id, out s))
        {
            return s.interactable;
        }
        return false;
    }

    /// <summary>
    /// 设置按钮组件开关
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <param name="enabled">开关</param>
    public void SetButtonEnable(int id, bool enabled)
    {
        Button c = null;
        if (TryGetControl(id, out c))
        {
            c.interactable = enabled;
        }
    }

    /// <summary>
    /// 获取文本
    /// </summary>
    /// <param name="id">组件ID</param>
    /// <returns>文本</returns>
    public string GetText(int id)
    {
        Text c = null;
        if (TryGetControl(id, out c))
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
        Text c = null;
        if (TryGetControl(id, out c))
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
        Text c = null;
        if (TryGetControl(id, out c))
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
        Slider c = null;
        if(TryGetControl(id, out c))
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
        Image c = null;
        if (TryGetControl(id, out c))
        {
            Coroutine co = null;
            if (CacheCoroutines.TryGetValue(id, out co))
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
        if (sizeRatio != 1)
        {
            Vector2 size = c.rectTransform.sizeDelta;
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
        var sprite = resource.GetSprite();
        resource.Retain();
        loader.Dispose();
        loader = null;
        CacheCoroutines.Remove(id);
        SetImage(c, sprite, resource, resetSize, sizeRatio);
        yield break;
    }

}
