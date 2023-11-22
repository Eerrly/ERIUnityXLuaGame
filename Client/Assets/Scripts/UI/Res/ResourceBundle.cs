using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceBundle : ReferenceCountBase
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    private ResManager Manager { get; set; }

    public uint Hash { get; private set; }

    private AssetBundle RawBundle { get; set; }

    private AssetBundle PackageBundle { get; set; }

    public bool CacheUnload = false;

    private Object _sharedAsset;

    /// <summary>
    /// 是否为流式加载的场景资源
    /// </summary>
    public bool isStreamedSceneAssetBundle => RawBundle != null && RawBundle.isStreamedSceneAssetBundle;

    public bool isSingleObject => RawBundle != null && RawBundle.GetAllAssetNames().Length == 1;

    public ResourceBundle(uint hash, AssetBundle bundle, AssetBundle packageBundle, ResManager manager)
    {
        Manager = manager;
        Hash = hash;
        RawBundle = bundle;
        PackageBundle = packageBundle;
    }

    /// <summary>
    /// 当引用计数为0时触发
    /// </summary>
    public override void OnReferenceBecameInvalid()
    {
        Manager.OnReferenceBecameInvalid(this);
    }

    /// <summary>
    /// 卸载AssetBundle但是保留已经加载出来的物体
    /// </summary>
    public void UnloadCache()
    {
        CacheUnload = true;
        if (RawBundle != null) RawBundle.Unload(false);
        if (PackageBundle != null) PackageBundle.Unload(false);
    }

    /// <summary>
    /// 释放AssetBundle并且卸载已经加载出来的物体
    /// </summary>
    public void UnloadBundle()
    {
        if (RawBundle != null) RawBundle.Unload(true);
        if (PackageBundle != null) PackageBundle.Unload(true);
        RawBundle = null;
        PackageBundle = null;
        if(_sharedAsset != null)
        {
            Object.DestroyImmediate(_sharedAsset);
            _sharedAsset = null;
        }
    }

    /// <summary>
    /// 释放AssetBundle并且卸载已经加载出来的物体(会修改依赖Bundle的引用计数)
    /// </summary>
    public void RealUnload()
    {
        Manager.UnloadBundleMap.Add(Hash, new BundleGroup() { PackageBundle = PackageBundle, RawBundle = RawBundle, Time = Time.realtimeSinceStartup });
        RawBundle = null;
        PackageBundle = null;
        Manager.Unload(Hash);
        Manager = null;
        if (_sharedAsset != null)
        {
            Object.DestroyImmediate(_sharedAsset);
            _sharedAsset = null;
        }
    }

    /// <summary>
    /// 加载Lua脚本
    /// </summary>
    /// <param name="dict">脚本名-字节流</param>
    public void LoadScript(Dictionary<string, byte[]> dict)
    {
        if(RawBundle != null)
        {
            var names = RawBundle.GetAllAssetNames();
            foreach (var t in names)
            {
                // skip: .bytes
                var name = t.Substring(0, t.Length - 6);
                var bytes = RawBundle.LoadAsset<TextAsset>(t).bytes;
                dict.Add(name, bytes);
            }
        }
        if (PackageBundle != null)
        {
            var names = PackageBundle.GetAllAssetNames();
            foreach (var t in names)
            {
                // skip: .bytes
                var name = t.Substring(0, t.Length - 6);
                var bytes = PackageBundle.LoadAsset<TextAsset>(t).bytes;
                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, bytes);
                }
            }
        }
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
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

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
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

    /// <summary>
    /// 异步加载所有资源
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="packageRequest">AssetBundleRequest</param>
    /// <returns></returns>
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
    public Object[] LoadAllAssetsFromRequest(AssetBundleRequest request, AssetBundleRequest packageRequest)
    {
        _tempList.Clear();
        Object[] rawAssets = null;
        if (request != null)
        {
            rawAssets = request.allAssets;
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
                    find = rawAssets.Any(rawAsset => (rawAsset.GetType() == asset.GetType() && rawAsset.name == asset.name));
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

    /// <summary>
    /// 加载所有资源
    /// </summary>
    /// <returns></returns>
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
                    find = rawAssets.Any(rawAsset => (rawAsset.GetType() == asset.GetType() && rawAsset.name == asset.name));
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