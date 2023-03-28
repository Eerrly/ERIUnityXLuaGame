using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour
{
    [System.NonSerialized] public Canvas canvas;
    [System.NonSerialized] public int id;
    [System.NonSerialized] public string path;
    [System.NonSerialized] new public string name;
    [System.NonSerialized] public int layer;
    [System.NonSerialized] public UIWindow parent;
    [System.NonSerialized] public Transform root;
    [System.NonSerialized] public bool isShow;
    [System.NonSerialized] public GraphicRaycaster raycaster;

    public LuaBehaviour behaviour;

    public int realLayer => canvas.sortingOrder;

    /// <summary>
    /// 创建UI窗口
    /// </summary>
    /// <param name="parent">父窗口</param>
    /// <param name="id">窗口ID</param>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <param name="layer"></param>
    /// <param name="obj"></param>
    public void Create(UIWindow parent, int id, string name, string path, int layer, object obj)
    {
        if(root == null)
        {
            root = transform.Find("Root");
            if(root == null)
            {
                root = transform.Find("@Root");
            }
            SetRoot();
        }
        this.parent = parent;
        this.id = id;
        this.name = name;
        this.path = path;
        canvas = GetComponent<Canvas>();
        if(canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.pixelPerfect = false;
        raycaster = GetComponent<GraphicRaycaster>();
        this.layer = layer;
        canvas.sortingOrder = layer * 10;
        canvas.planeDistance = 8000 - canvas.sortingOrder;
        if(canvas.planeDistance <= 0)
        {
            canvas.planeDistance = 0;
        }
        behaviour = GetComponent<LuaBehaviour>();
        if(behaviour == null)
        {
            behaviour = gameObject.AddComponent<LuaBehaviour>();
            behaviour.Initialize(obj, null);
        }
        else
        {
            behaviour.BindInstance(obj);
        }
    }

    /// <summary>
    /// 设置Root的锚点
    /// </summary>
    private void SetRoot()
    {
        if (root != null)
        {
            var rt = root.GetComponent<RectTransform>();
            if(rt != null)
            {
                rt.anchorMax = Vector2.one;
                rt.anchorMin = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="callback">回调</param>
    public void OnShow(System.Action callback = null)
    {
        raycaster.enabled = true;
        isShow = true;
        Util.SetGameObjectLayer(gameObject, Setting.LAYER_UI, true);
        if(callback != null)
        {
            callback();
        }
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="callback">回调</param>
    public void OnHide(System.Action callback = null)
    {
        raycaster.enabled = false;
        isShow = false;
        Util.SetGameObjectLayer(gameObject, Setting.LAYER_HIDE, true);
        if(callback != null)
        {
            callback();
        }
    }

    public void Destory()
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        isShow = false;
        if(behaviour != null)
        {
            behaviour.Release();
            behaviour = null;
        }
    }

}
