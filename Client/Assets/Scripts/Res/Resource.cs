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
        if(_bundle != null)
        {
            _bundle.Release();
            _bundle = null;
        }
    }

    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject()
    {
        GameObject go = Asset as GameObject;
        return go;
    }

    /// <summary>
    /// 获取GameObject
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    public GameObject GetGameObject(string name)
    {
        if(Assets != null)
        {
            for (int i = 0; i < Assets.Length; i++)
            {
                if(Assets[i].name == name && Assets[i] is GameObject)
                {
                    return Assets[i] as GameObject;
                }
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
        GameObject go = Asset as GameObject;
        GameObject instance = GameObject.Instantiate<GameObject>(go);
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
        if (Assets != null)
        {
            for (int i = 0; i < Assets.Length; i++)
            {
                if (Assets[i].name == name && Assets[i] is GameObject)
                {
                    GameObject go = Assets[i] as GameObject;
                    GameObject instance = GameObject.Instantiate<GameObject>(go);
                    instance.name = Assets[i].name;
                    return instance;
                }
            }
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
            for (int i = 0; i < Assets.Length; i++)
            {
                if (Assets[i].name == name)
                {
                    sprite = Assets[i] as Sprite;
                    if(sprite == null && Assets[i] != null)
                    {
                        var tex = Assets[i] as Texture2D;
                        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    if (sprite != null)
                    {
                        break;
                    }
                }
            }
        }
        if (sprite == null && Asset != null)
        {
            sprite = Asset as Sprite;
            if (sprite == null && Asset != null)
            {
                var tex = Asset as Texture2D;
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
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
