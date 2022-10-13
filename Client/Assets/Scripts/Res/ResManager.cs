using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ResManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    internal Dictionary<string, uint> cacheFileMap = new Dictionary<string, uint>();
    internal Resource errorResource = new Resource(null, null, null, "not exist");
    internal bool preInitialized = false;
    internal object unloadResourceBundlesLock = new object();
    internal Queue<ResourceBundle> unloadResourceBundles = new Queue<ResourceBundle>();
    internal List<LoadingTask> loadingTasks = new List<LoadingTask>();
    internal List<LoadingTask> finishedList = new List<LoadingTask>();

    public Manifest manifest;
    public BuildToolsConfig client;
    public HashSet<uint> loadingBundles = new HashSet<uint>();
    public Dictionary<uint, ResourceBundle> loadedBundles = new Dictionary<uint, ResourceBundle>();
    public Dictionary<uint, BundleGroup> unloadBundleMap = new Dictionary<uint, BundleGroup>();

    uint ConvertPath(string path)
    {
        uint result = 0;
        if (!string.IsNullOrEmpty(path))
        {
            path = path.ToLower().Replace("assets/sources/", "");
            if(!cacheFileMap.TryGetValue(path, out result))
            {
                result = Util.HashPath(path);
                cacheFileMap.Add(path, result);
            }
        }
        return result;
    }

    public void OnReferenceBecameInvalid(ResourceBundle bundle)
    {
        lock (unloadResourceBundlesLock)
        {
            unloadResourceBundles.Enqueue(bundle);
        }
    }

    public void OnInitialize()
    {
        preInitialized = true;
        loadingTasks.Clear();
        finishedList.Clear();
        cacheFileMap.Clear();
        unloadResourceBundles.Clear();
        loadedBundles.Clear();
        loadingBundles.Clear();
        unloadBundleMap.Clear();
        LuaUtil.ClearDontDestroyObjs();
        StartCoroutine(nameof(CoWaitInitialize));
    }

    public IEnumerator CoWaitInitialize()
    {
#if UNITY_EDITOR
        if (Setting.Config.useAssetBundle)
        {
            InitializeClientConfig();
        }
#else
        InitializeClientConfig();
#endif
        yield return null;
        IsInitialized = true;
    }

    private void InitializeClientConfig()
    {
        ManifestConfig config = Util.LoadConfig<ManifestConfig>(Constant.ASSETBUNDLES_CONFIG_NAME);
        manifest = new Manifest()
        {
            ManifestDict = new Dictionary<uint, ManifestItem>(config.items.Length + 1),
        };
        if(config != null)
        {
            for (int i = 0; i < config.items.Length; i++)
            {
                var item = config.items[i];
                item.packageResource = true;
                item.packageResourcePath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
                manifest.ManifestDict.Add(item.hash, item);
            }
        }
    }

    public LoadingLocation ToLocation(ManifestItem item)
    {
        var location = new LoadingLocation();
        location.path = item.packageResourcePath;
        location.location = ELoadingLocation.Package;
        return location;
    }

    public LoadingLocation ToLocation(uint hash)
    {
        var item = manifest.GetItem(hash);
        var location = new LoadingLocation();
        if (item.packageResource)
        {
            location.location = ELoadingLocation.Package;
            if(item.offset == 0)
            {
                location.path = System.IO.Path.Combine(Setting.StreamingBundleRoot, item.hash + ".s");
            }
            else
            {
                location.path = item.packageResourcePath;
            }
        }
        else {
            location.location = ELoadingLocation.Cache;
            location.path = System.IO.Path.Combine(Setting.CacheRoot, item.hash + ".s");
        }
        return location;
    }

    public void LoadSync(string path, string name, System.Action<Resource> onLoaded)
    {
        Load(path, name, ConvertPath(path), false, false, onLoaded);
    }

    public void LoadAsync(string path, string name, System.Action<Resource> onLoaded)
    {
        Load(path, name, ConvertPath(path), true, false, onLoaded);
    }

    public void LoadAsync(string path, System.Action<Resource> onLoaded)
    {
        Load(path, null, ConvertPath(path), true, false, onLoaded);
    }

    private void Load(string path, string name, uint hash, bool async, bool isDependency, System.Action<Resource> onLoaded, Dictionary<string, bool> namesDict = null, string extension = null)
    {
        if (preInitialized)
        {
            if(Setting.Config.useAssetBundle && (manifest == null || !manifest.Exist(hash)))
            {
                if(onLoaded != null) onLoaded(errorResource);
            }
        }
        name = name == null ? "" : name.ToLower();
        LoadingTask task = null;
        for (int i = 0; i < loadingTasks.Count; i++)
        {
            if(loadingTasks[i].state == ELoadingState.None)
            {
                task = loadingTasks[i];
                break;
            }
        }
        if(task == null)
        {
            task = new LoadingTask();
            loadingTasks.Add(task);
        }
        task.Reset();
        task.state = ELoadingState.Ready;
        task.path = path;
        task.name = name;
        task.hash = hash;
        task.extension = extension;
        task.namesDict = namesDict;
        task.async = async;
        task.isDependency = isDependency;
        if(manifest != null)
        {
            var dependencies = manifest.GetDependencies(task.hash, out task.offset);
            for (int i = 0; i < dependencies.Length; i++)
            {
                var found = false;
                for (int j = 0; j < loadingTasks.Count; j++)
                {
                    var tmpTask = loadingTasks[j];
                    if((tmpTask.state == ELoadingState.Ready || tmpTask.state == ELoadingState.Loading) && task.hash == dependencies[i])
                    {
                        loadingTasks[j].dependencyRefCount++;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var unload = false;
                    lock (unloadResourceBundlesLock)
                    {
                        foreach (var unloadBundle in unloadResourceBundles)
                        {
                            if(unloadBundle.Hash == dependencies[i])
                            {
                                unload = true;
                                break;
                            }
                        }
                    }
                    ResourceBundle bundle = null;
                    if(unload || !loadedBundles.TryGetValue(hash, out bundle))
                    {
                        Load("", null, dependencies[i], async, true, null);
                    }
                    else
                    {
                        bundle.Retain();
                    }
                }
            }
        }
        if(onLoaded != null)
        {
            task.onLoadedCallbacks.Add(onLoaded);
        }
    }

    public void Unload(uint hash)
    {
        if (preInitialized)
        {
            ResourceBundle bundle;
            if(loadedBundles.TryGetValue(hash, out bundle))
            {
                loadedBundles.Remove(hash);
                if(manifest != null)
                {
                    var dependencies = manifest.GetDependencies(hash);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        ResourceBundle tmpBundle;
                        if(loadedBundles.TryGetValue(dependencies[i], out tmpBundle))
                        {
                            if (tmpBundle != null) tmpBundle.Release();
                        }
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (preInitialized)
        {
            if(unloadResourceBundles.Count > 0)
            {
                lock (unloadResourceBundlesLock)
                {
                    while(unloadResourceBundles.Count > 0)
                    {
                        var bundle = unloadResourceBundles.Dequeue();
                        bundle.RealUnload();
                    }
                }
            }
            int loadingCount = 0;
            for (int i = 0; i < loadingTasks.Count; i++)
            {
                var task = loadingTasks[i];
                if(task.state == ELoadingState.Loading)
                {
                    loadingCount++;
                }
                if (loadingCount > Constant.MaxLoadingTaskCount)
                {
                    break;
                }
                if(task.state == ELoadingState.Ready)
                {
                    task.state = ELoadingState.Loading;
#if UNITY_EDITOR
                    if (Setting.Config.useAssetBundle)
                    {
                        StartCoroutine(ResLoader.CoLoadTask(task));
                    }
                    else
                    {
                        StartCoroutine(ResLoader.CoEditorLoadTask(task));
                    }
#else
                    StartCoroutine(ResLoader.CoLoadTask(task));
#endif
                }
                if (task.state == ELoadingState.Finished)
                {
                    task.state = ELoadingState.None;
                    finishedList.Add(task);
                }
            }
            for (int i = 0; i < finishedList.Count; i++)
            {
                var task = finishedList[i];
                for (int j = 0; j < task.onLoadedCallbacks.Count; j++)
                {
                    if(task.file != null)
                    {
                        task.file.Retain();
                        task.onLoadedCallbacks[j](task.file);
                        task.file.Release();
                    }
                }
            }
            finishedList.Clear();
        }
    }

    public void OnRelease()
    {
        preInitialized = false;
        manifest = null;
        var e = loadedBundles.GetEnumerator();
        while (e.MoveNext())
        {
            var bundle = e.Current.Value;
            if(bundle != null)
            {
                bundle.Release();
            }
        }
        loadedBundles.Clear();
        loadingBundles.Clear();
        loadingTasks.Clear();
        finishedList.Clear();
        ResLoader.requestList.Clear();
        if(unloadResourceBundles.Count > 0)
        {
            lock (unloadResourceBundlesLock)
            {
                while(unloadResourceBundles.Count > 0)
                {
                    var bundle = unloadResourceBundles.Dequeue();
                    bundle.RealUnload();
                }
            }
        }
        IsInitialized = false;
    }

    private void OnDestroy()
    {
        OnRelease();
    }

}
