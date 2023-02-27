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
    internal struct Event
    {
        public int eventId;
        public int id;
    }

    internal enum EventID
    {
        ButtonClicked = 0,
    }

    private UIBehaviour[] id2ui;
    private Dictionary<string, int> name2id;
    private Dictionary<int, Coroutine> _cacheCoroutines = null;
    private List<Resource> _loadedCache = null;
    private object instance;
    private LuaBehaviour root;
    private List<LuaBehaviour> children;
    private List<Event> events;
    private int index;

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
                name2id.Add(name, id);
                id2ui[id++] = ui;
            }
        }
    }

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
            Debug.Log("GetChild LuaBehaviour Not Found!");
            return null;
        }
    }

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
        }
    }

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

    public bool IsActive(int id)
    {
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            return c.gameObject.activeSelf;
        }
        return false;
    }

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

    public void SetParent(int id1, int id2, bool worldPositionStays)
    {
        UIBehaviour c1 = null;
        UIBehaviour c2 = null;
        if (TryGetControl(id1, out c1) && TryGetControl(id2, out c2))
        {
            c1.transform.SetParent(c2.transform, worldPositionStays);
        }
    }

    public bool IsEnable(int id)
    {
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            return c.enabled;
        }
        return false;
    }

    public void SetEnable(int id, bool enabled)
    {
        UIBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            c.enabled = enabled;
        }
    }

    public void SetPosition(int id, float x, float y, float z)
    {
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            c.transform.localPosition = new Vector3(x, y, z);
        }
    }

    public void SetRotation(int id, float x, float y, float z)
    {
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            c.transform.localRotation = Quaternion.Euler(new Vector3(x, y, z));
        }
    }

    public void SetScale(int id, float x, float y, float z)
    {
        MonoBehaviour c = null;
        if (TryGetControl(id, out c))
        {
            c.transform.localScale = new Vector3(x, y, z);
        }
    }

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

    public void SetInteractable(int id, bool interactable)
    {
        Selectable s = null;
        if (TryGetControl(id, out s))
        {
            s.interactable = interactable;
        }
    }

    public bool IsInteractable(int id, bool interactable)
    {
        Selectable s = null;
        if (TryGetControl(id, out s))
        {
            return s.interactable;
        }
        return false;
    }

    public void SetButtonEnable(int id, bool enabled)
    {
        Button c = null;
        if (TryGetControl(id, out c))
        {
            c.interactable = enabled;
        }
    }

    public string GetText(int id)
    {
        Text c = null;
        if (TryGetControl(id, out c))
        {
            return c.text;
        }
        return "";
    }

    public void SetText(int id, string text)
    {
        Text c = null;
        if (TryGetControl(id, out c))
        {
            c.text = text;
        }
    }

    public void SetFontSize(int id, int size)
    {
        Text c = null;
        if (TryGetControl(id, out c))
        {
            c.fontSize = size;
        }
    }

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
