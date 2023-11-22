using System.Collections.Generic;
using UnityEngine;

public class Resource : ReferenceCountBase
{

    private ResourceBundle _bundle;

    public string Path { get; private set; }

    public string Name { get; private set; }

    public Object Asset { get; private set; }

    public Object[] Assets { get; private set; }

    public string Error { get; private set; }

    public Resource(string path, string name, Object asset, ResourceBundle bundle, string error)
    {
        Path = path;
        Name = name;
        Asset = asset;

        Assets = null;
        if(bundle != null)
        {
            _bundle = bundle;
            _bundle.Retain();
        }
        Error = error;
    }

    public Resource(string path, Object[] assets, ResourceBundle bundle, string error)
    {
        Path = path;
        Name = "";
        Assets = assets;

        Asset = null;
        if (bundle != null)
        {
            _bundle = bundle;
            _bundle.Retain();
        }
        Error = error;
    }

    /// <summary>
    /// 引用计数为0时触发
    /// </summary>
    public override void OnReferenceBecameInvalid()
    {
        if (_bundle == null) return;
        
        _bundle.Release();
        _bundle = null;
    }

    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject()
    {
        var go = Asset as GameObject;
        return go;
    }

    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    public GameObject GetGameObject(string name)
    {
        if (Assets == null) return null;
        
        foreach (var t in Assets)
        {
            if(t.name == name && t is GameObject o)
            {
                return o;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取GameObject实例
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObjectInstance()
    {
        var go = Asset as GameObject;
        var instance = Object.Instantiate<GameObject>(go);
        instance.name = Asset.name;
        return instance;
    }

    /// <summary>
    /// 获取GameObject实例
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    public GameObject GetGameObjectInstance(string name)
    {
        if (Assets == null) return null;
        foreach (var t in Assets)
        {
            if (t.name != name || !(t is GameObject o)) continue;
            
            var instance = Object.Instantiate<GameObject>(o);
            instance.name = o.name;
            return instance;
        }
        return null;
    }

    /// <summary>
    /// 获取图片
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    public Sprite GetSprite(string name = null)
    {
        Sprite sprite = null;
        if (name != null && Assets != null)
        {
            foreach (var t in Assets)
            {
                if (t.name != name) continue;
                
                sprite = t as Sprite;
                if(sprite == null && t != null)
                {
                    var tex = t as Texture2D;
                    if (tex != null)
                        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                if (sprite != null)
                {
                    break;
                }
            }
        }

        if (sprite != null || Asset == null) return sprite;
        
        sprite = Asset as Sprite;
        if (sprite == null && Asset != null)
        {
            var tex = Asset as Texture2D;
            if (tex != null) 
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        
        return sprite;
    }

    /// <summary>
    /// 加载Lua脚本
    /// </summary>
    /// <param name="dict">脚本名-字节流</param>
    public void LoadScript(Dictionary<string, byte[]> dict)
    {
        _bundle.LoadScript(dict);
    }

}
