using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour
{
    [System.NonSerialized] public Canvas Canvas;
    [System.NonSerialized] public int ID;
    [System.NonSerialized] public string Path;
    [System.NonSerialized] public string Name;
    [System.NonSerialized] public int Layer;
    [System.NonSerialized] public UIWindow Parent;
    [System.NonSerialized] public Transform Root;
    [System.NonSerialized] public bool IsShow;
    [System.NonSerialized] public GraphicRaycaster Raycaster;

    public LuaBehaviour behaviour;

    public int realLayer => Canvas.sortingOrder;

    /// <summary>
    /// 创建UI窗口
    /// </summary>
    /// <param name="parent">父窗口</param>
    /// <param name="id">窗口ID</param>
    /// <param name="wName"></param>
    /// <param name="path"></param>
    /// <param name="layer"></param>
    /// <param name="obj"></param>
    public void Create(UIWindow parent, int id, string wName, string path, int layer, object obj)
    {
        if(Root == null)
        {
            Root = transform.Find("Root");
            if(Root == null)
            {
                Root = transform.Find("@Root");
            }
            SetRoot();
        }
        this.Parent = parent;
        this.ID = id;
        this.Name = wName;
        this.Path = path;
        Canvas = GetComponent<Canvas>();
        if(Canvas == null)
        {
            Canvas = gameObject.AddComponent<Canvas>();
        }
        Canvas.pixelPerfect = false;
        Raycaster = GetComponent<GraphicRaycaster>();
        this.Layer = layer;
        Canvas.sortingOrder = layer * 10;
        Canvas.planeDistance = 8000 - Canvas.sortingOrder;
        if(Canvas.planeDistance <= 0)
        {
            Canvas.planeDistance = 0;
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
        if (Root == null) return;
        
        var rt = Root.GetComponent<RectTransform>();
        if(rt != null)
        {
            rt.anchorMax = Vector2.one;
            rt.anchorMin = Vector2.zero;
        }
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="callback">回调</param>
    public void OnShow(System.Action callback = null)
    {
        IsShow = true;
        Raycaster.enabled = true;
        Util.SetGameObjectLayer(gameObject, Setting.LAYER_UI, true);
        callback?.Invoke();
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="callback">回调</param>
    public void OnHide(System.Action callback = null)
    {
        IsShow = false;
        Raycaster.enabled = false;
        Util.SetGameObjectLayer(gameObject, Setting.LAYER_HIDE, true);
        callback?.Invoke();
    }

    public void OnDestroy()
    {
        IsShow = false;
        if (behaviour == null) return;
        
        behaviour.Release();
        behaviour = null;
    }

}
