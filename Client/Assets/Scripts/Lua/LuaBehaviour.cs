using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XLua;
using System;

[LuaCallCSharp, GenComment]
public class LuaBehaviour : MonoBehaviour
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
    private object instance;
    private LuaBehaviour root;
    private List<LuaBehaviour> children;
    private List<Event> events;
    private int index;

    private void Awake()
    {
        name2id = new Dictionary<string, int>();

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
        LuaManager.Instance.BindInstance(this, name2id, instance);
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
                child.Initialize(LuaManager.Instance.luaEnv.NewTable(), root);
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

}
