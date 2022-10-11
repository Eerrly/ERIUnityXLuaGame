using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

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

    public override void OnReferenceBecameInvalid()
    {
        if(_bundle != null)
        {
            _bundle.Release();
            _bundle = null;
        }
    }

    public GameObject GetGameObject()
    {
        GameObject go = Asset as GameObject;
        return go;
    }

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

    public GameObject GetGameObjectInstance()
    {
        GameObject go = Asset as GameObject;
        GameObject instance = GameObject.Instantiate<GameObject>(go);
        instance.name = Asset.name;
        return instance;
    }

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

    public Sprite GetSprite(string name)
    {
        Sprite sprite = null;
        if(Assets != null)
        {
            for (int i = 0; i < Assets.Length; i++)
            {
                if(Assets[i].name == name)
                {
                    sprite = (Assets[i] as SpriteAtlas).GetSprite(name);
                    if (sprite != null)
                    {
                        break;
                    }
                }
            }
        }
        if (sprite == null && Asset != null)
        {
            sprite = (Asset as SpriteAtlas).GetSprite(name);
        }
        return sprite;
    }

}
