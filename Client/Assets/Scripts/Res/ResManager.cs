using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// 资源管理器
/// </summary>
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

    /// <summary>
    /// 将路径转化为Hash值
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>转换后的Hash值</returns>
    uint ConvertPath(string path)
    {
        uint result = 0;
        if (!string.IsNullOrEmpty(path))
        {
            path = FileUtil.Normalized(path).ToLower().Replace(ResUtil.ASSETS_SOURCES_LOWER_PATH, "");
            if (!cacheFileMap.TryGetValue(path, out result))
            {
                result = Util.HashPath(path);
                cacheFileMap.Add(path, result);
            }
        }
        return result;
    }

    /// <summary>
    /// 当引用计数为0时触发
    /// </summary>
    /// <param name="bundle"></param>
    public void OnReferenceBecameInvalid(ResourceBundle bundle)
    {
        lock (unloadResourceBundlesLock)
        {
            unloadResourceBundles.Enqueue(bundle);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
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

    /// <summary>
    /// 初始化AB资源配置文件
    /// </summary>
    private void InitializeClientConfig()
    {
        ManifestConfig config = Util.LoadConfig<ManifestConfig>(Constant.ASSETBUNDLES_CONFIG_NAME);

        manifest = new Manifest()
        {
            ManifestDict = new Dictionary<uint, ManifestItem>(config.items.Length + 1),
        };

        ManifestConfig patchConfig = null;
        if (Setting.Config.enablePatching)
        {
            var patchRcFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "rc.bytes");
            if (System.IO.File.Exists(patchRcFilePath))
            {
                var confJson = System.Text.ASCIIEncoding.Default.GetString(System.IO.File.ReadAllBytes(patchRcFilePath));
                patchConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(confJson);
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"Enabled Patching But Not Found {patchRcFilePath}");
            }
        }

        var strMainRootPath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
        // 整包资源相关配置
        if (config != null)
        {
            for (int i = 0; i < config.items.Length; i++)
            {
                var item = config.items[i];
                item.packageResource = true;
                item.packageResourcePath = strMainRootPath;
                manifest.ManifestDict.Add(item.hash, item);
            }
        }

        // 热更资源相关配置
        if(patchConfig != null)
        {
            for (int i = 0; i < patchConfig.items.Length; i++)
            {
                var item = patchConfig.items[i];
                var patchFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s");
                if (!System.IO.File.Exists(patchFilePath))
                {
                    continue;
                }

                item.packageResource = false;
                if (manifest.ManifestDict.ContainsKey(item.hash))
                {
                    if (item.directories)
                    {
                        item.packageItem = manifest.ManifestDict[item.hash];
                    }
                    manifest.ManifestDict[item.hash] = item;
                }
                else
                {
                    manifest.ManifestDict.Add(item.hash, item);
                }
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
            if (item.offset == 0)
            {
                location.path = FileUtil.CombinePaths(Setting.StreamingBundleRoot, item.hash + ".s");
            }
            else
            {
                location.path = item.packageResourcePath;
            }
        }
        else
        {
            location.location = ELoadingLocation.Cache;
            location.path = FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s");
        }
        return location;
    }

    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="name">资源名</param>
    /// <param name="onLoaded">加载完成回调方法</param>
    public void LoadSync(string path, string name, System.Action<Resource> onLoaded)
    {
        Load(path, name, ConvertPath(path), false, false, onLoaded);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="name">资源名</param>
    /// <param name="onLoaded">加载完成回调方法</param>
    public void LoadAsync(string path, string name, System.Action<Resource> onLoaded)
    {
        Load(path, name, ConvertPath(path), true, false, onLoaded);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="onLoaded">加载完成回调方法</param>
    public void LoadAsync(string path, System.Action<Resource> onLoaded)
    {
        Load(path, null, ConvertPath(path), true, false, onLoaded);
    }

    /// <summary>
    /// 将需要加载的资源加到 加载资源的任务列表中
    /// </summary>
    private void Load(string path, string name, uint hash, bool async, bool isDependency, System.Action<Resource> onLoaded, Dictionary<string, bool> namesDict = null, string extension = null)
    {
        if (preInitialized)
        {
            if (Setting.Config.useAssetBundle && (manifest == null || !manifest.Exist(hash)))
            {
                if (onLoaded != null) onLoaded(errorResource);
            }
        }
        name = name == null ? "" : name.ToLower();
        LoadingTask task = null;
        for (int i = 0; i < loadingTasks.Count; i++)
        {
            if (loadingTasks[i].state == ELoadingState.None)
            {
                task = loadingTasks[i];
                break;
            }
        }
        if (task == null)
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

        // 在下一帧开始LoadingTask之前，先加载依赖
        if (manifest != null)
        {
            var dependencies = manifest.GetDependencies(task.hash, out task.offset);
            for (int i = 0; i < dependencies.Length; i++)
            {
                var found = false;
                for (int j = 0; j < loadingTasks.Count; j++)
                {
                    var tmpTask = loadingTasks[j];
                    if ((tmpTask.state == ELoadingState.Ready || tmpTask.state == ELoadingState.Loading) && task.hash == dependencies[i])
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
                            if (unloadBundle.Hash == dependencies[i])
                            {
                                unload = true;
                                break;
                            }
                        }
                    }
                    ResourceBundle bundle = null;
                    if (unload || !loadedBundles.TryGetValue(hash, out bundle))
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
        if (onLoaded != null)
        {
            task.onLoadedCallbacks.Add(onLoaded);
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="hash">AB的Hash值</param>
    public void Unload(uint hash)
    {
        if (!preInitialized)
        {
            return;
        }

        ResourceBundle bundle;
        if (loadedBundles.TryGetValue(hash, out bundle))
        {
            loadedBundles.Remove(hash);
            if (manifest == null)
            {
                return;
            }
            var dependencies = manifest.GetDependencies(hash);
            for (int i = 0; i < dependencies.Length; i++)
            {
                ResourceBundle tmpBundle;
                if (loadedBundles.TryGetValue(dependencies[i], out tmpBundle))
                {
                    if (tmpBundle != null)
                    {
                        tmpBundle.Release();
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!preInitialized)
        {
            return;
        }

        ReleaseUnloadResourceBundles();

        int loadingCount = 0;
        for (int i = 0; i < loadingTasks.Count; i++)
        {
            var task = loadingTasks[i];
            if (task.state == ELoadingState.Loading)
            {
                loadingCount++;
            }
            if (loadingCount > Constant.MaxLoadingTaskCount)
            {
                break;
            }
            if (task.state == ELoadingState.Ready)
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

        CallBackFinishedResourceBundles();
    }

    /// <summary>
    /// 触发所有已经完成的资源的完成回调
    /// </summary>
    private void CallBackFinishedResourceBundles()
    {
        if(finishedList.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < finishedList.Count; i++)
        {
            var task = finishedList[i];
            for (int j = 0; j < task.onLoadedCallbacks.Count; j++)
            {
                if (task.file != null)
                {
                    task.file.Retain();
                    task.onLoadedCallbacks[j](task.file);
                    task.file.Release();
                }
            }
        }
        finishedList.Clear();
    }

    /// <summary>
    /// 释放掉所有需要被卸载的资源
    /// </summary>
    private void ReleaseUnloadResourceBundles()
    {
        if(unloadResourceBundles.Count <= 0)
        {
            return;
        }
        lock (unloadResourceBundlesLock)
        {
            while (unloadResourceBundles.Count > 0)
            {
                var bundle = unloadResourceBundles.Dequeue();
                bundle.RealUnload();
            }
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
            if (bundle != null)
            {
                bundle.Release();
            }
        }
        loadedBundles.Clear();
        loadingBundles.Clear();
        loadingTasks.Clear();
        finishedList.Clear();
        ResLoader.requestList.Clear();
        ReleaseUnloadResourceBundles();
        IsInitialized = false;
    }

    private void OnDestroy()
    {
        OnRelease();
    }

}
