using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class UnityResourceLoader : MonoBehaviour
{
    private static UnityResourceLoader instance;

    public static UnityResourceLoader Instance
    {
        get
        {
            if (instance == null){
                instance = new GameObject("[UnityResourceLoader]").AddComponent<UnityResourceLoader>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

    public async UniTask<AssetBundle> LoadAssetBundleAsyncTask(string path)
    {
        AssetBundle bundle;
        if(!loadedBundles.TryGetValue(path, out bundle))
        {
            bundle = await AssetBundle.LoadFromFileAsync(path);
        }
        return bundle;
    }

    public AssetBundle LoadAssetBundle(string path)
    {
        AssetBundle bundle;
        if (!loadedBundles.TryGetValue(path, out bundle))
        {
            bundle = AssetBundle.LoadFromFile(path);
        }
        return bundle;
    }

    public async UniTask<T> LoadObjectAsyncTask<T>(AssetBundle bundle, string name) where T : Object
    {
        T obj = await bundle.LoadAssetAsync(name, typeof(T)) as T;
        return obj;
    }

    public T LoadObject<T>(AssetBundle bundle, string name) where T : Object
    {
        T obj = bundle.LoadAsset(name, typeof(T)) as T;
        return obj;
    }

    public async UniTask<string[]> GetAllDependenciesAsyncTask(string bundleName)
    {
        AssetBundle mainBundle;
        if (!loadedBundles.TryGetValue(StaticValue.BundleCache, out mainBundle))
        {
            mainBundle = await LoadAssetBundleAsyncTask(StaticValue.BundleCache);
        }
        AssetBundleManifest mainManifest = await LoadObjectAsyncTask<AssetBundleManifest>(mainBundle, StaticValue.AssetBundleManifest);
        var dependencies = mainManifest.GetAllDependencies(bundleName);
        return dependencies;
    }

    public string[] GetAllDependencies(string bundleName)
    {
        AssetBundle mainBundle;
        if (!loadedBundles.TryGetValue(StaticValue.BundleCache, out mainBundle))
        {
            mainBundle = LoadAssetBundle(StaticValue.BundleCache);
        }
        AssetBundleManifest mainManifest = LoadObject<AssetBundleManifest>(mainBundle, StaticValue.AssetBundleManifest);
        string[] dependencies = mainManifest.GetAllDependencies(bundleName);
        return dependencies;
    }

    private void OnDestroy()
    {
        loadedBundles.Clear();
    }

}
