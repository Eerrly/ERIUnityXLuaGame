using System.Collections.Generic;
using UnityEngine;

public class ResourceBundle : ReferenceCountBase
{

    private ResManager _manager;

    public ResManager Manager => _manager;

    public uint Hash { get; private set; }

    public AssetBundle RawBundle { get; set; }
    
    public AssetBundle PackageBundle { get; set; }

    public bool CacheUnload = false;

    public Object SharedAsset;

    public bool isStreamedSceneAssetBundle => RawBundle != null && RawBundle.isStreamedSceneAssetBundle;

    public bool isSingleObject => RawBundle != null && RawBundle.GetAllAssetNames().Length == 1;

    public ResourceBundle(uint hash, AssetBundle bundle, AssetBundle packageBundle, ResManager manager)
    {
        _manager = manager;
        Hash = hash;
        RawBundle = bundle;
        PackageBundle = packageBundle;
    }

    public override void OnReferenceBecameInvalid()
    {
        Manager.OnReferenceBecameInvalid(this);
    }

    public void UnloadCache()
    {
        CacheUnload = true;
        RawBundle?.Unload(false);
        PackageBundle?.Unload(false);
    }

    public void UnloadBundle()
    {
        RawBundle?.Unload(true);
        PackageBundle?.Unload(true);
        RawBundle = null;
        PackageBundle = null;
        if(SharedAsset != null)
        {
            Object.DestroyImmediate(SharedAsset);
            SharedAsset = null;
        }
    }

    public void RealUnload()
    {
        RawBundle = null;
        PackageBundle = null;
        Manager.Unload(Hash);
        _manager = null;
        if (SharedAsset != null)
        {
            Object.DestroyImmediate(SharedAsset);
            SharedAsset = null;
        }
    }

    public void LoadScript(Dictionary<string, byte[]> dict)
    {
        if(RawBundle != null)
        {
            var names = RawBundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++)
            {
                // skip: .bytes
                var name = names[i].Substring(0, names[i].Length - 6);
                var bytes = RawBundle.LoadAsset<TextAsset>(names[i]).bytes;
                dict.Add(name, bytes);
            }
        }
        if (PackageBundle != null)
        {
            var names = PackageBundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++)
            {
                // skip: .bytes
                var name = names[i].Substring(0, names[i].Length - 6);
                var bytes = PackageBundle.LoadAsset<TextAsset>(names[i]).bytes;
                dict.Add(name, bytes);
            }
        }
    }

    public Object LoadAsset(string name)
    {
        if(RawBundle != null)
        {
            var asset = RawBundle.LoadAsset(name);
            if(asset != null)
            {
                return asset;
            }
        }
        if(PackageBundle != null)
        {
            var asset = PackageBundle.LoadAsset(name);
            if(asset != null)
            {
                return asset;
            }
        }
        return null;
    }

    public AssetBundleRequest LoadAssetAsync(string name)
    {
        if(RawBundle != null)
        {
            return RawBundle.LoadAssetAsync(name);
        }
        if(PackageBundle != null)
        {
            return PackageBundle.LoadAssetAsync(name);
        }
        return null;
    }

    public AssetBundleRequest LoadAllAssetsAsync(System.Type type, out AssetBundleRequest packageRequest)
    {
        packageRequest = null;
        if(PackageBundle != null)
        {
            packageRequest = type == null ? PackageBundle.LoadAllAssetsAsync() : PackageBundle.LoadAllAssetsAsync(type);
        }
        if(RawBundle != null)
        {
            return type == null ? RawBundle.LoadAllAssetsAsync() : RawBundle.LoadAllAssetsAsync(type);
        }
        return null;
    }

    private static List<Object> _tempList = new List<Object>();
    public Object[] LoadAllAssetsFromRequest(AssetBundleRequest requst, AssetBundleRequest packageRequest)
    {
        _tempList.Clear();
        Object[] rawAssets = null;
        if (requst != null)
        {
            rawAssets = requst.allAssets;
            foreach (var rawAsset in rawAssets)
            {
                _tempList.Add(rawAsset);
            }
        }
        if (packageRequest != null)
        {
            var assets = packageRequest.allAssets;
            foreach (var asset in assets)
            {
                var find = false;
                if (rawAssets != null)
                {
                    foreach (var rawAsset in rawAssets)
                    {
                        if ((rawAsset.GetType() == asset.GetType() && rawAsset.name == asset.name))
                        {
                            find = true;
                            break;
                        }
                    }
                }
                if (!find)
                {
                    _tempList.Add(asset);
                }
            }
        }
        var results = _tempList.ToArray();
        _tempList.Clear();
        return results;
    }

    public Object[] LoadAllAssets()
    {
        _tempList.Clear();
        Object[] rawAssets = null;
        if (RawBundle != null)
        {
            rawAssets = RawBundle.LoadAllAssets();
            foreach (var rawAsset in rawAssets)
            {
                _tempList.Add(rawAsset);
            }
        }
        if (PackageBundle != null)
        {
            var assets = PackageBundle.LoadAllAssets();
            foreach (var asset in assets)
            {
                var find = false;
                if (rawAssets != null)
                {
                    foreach (var rawAsset in rawAssets)
                    {
                        if ((rawAsset.GetType() == asset.GetType() && rawAsset.name == asset.name))
                        {
                            find = true;
                            break;
                        }
                    }
                }
                if (!find)
                {
                    _tempList.Add(asset);
                }
            }
        }
        var results = _tempList.ToArray();
        _tempList.Clear();
        return results;
    }

}