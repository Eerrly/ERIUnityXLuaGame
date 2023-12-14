using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 资源管理器
/// </summary>
public class ResManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 路径转Hash值的缓存字典
    /// </summary>
    internal readonly Dictionary<string, uint> CacheFileMap = new Dictionary<string, uint>();
    private readonly Resource _errorResource = new Resource(null, null, null, "Not Exist!");
    private bool _preInitialized = false;
    private readonly object _unloadResourceBundlesLock = new object();
    private readonly Queue<ResourceBundle> _unloadResourceBundles = new Queue<ResourceBundle>();
    private readonly List<LoadingTask> _loadingTasks = new List<LoadingTask>();
    private readonly List<LoadingTask> _finishedList = new List<LoadingTask>();

    public Manifest Manifest;
    public readonly HashSet<uint> LoadingBundles = new HashSet<uint>();
    public readonly Dictionary<uint, ResourceBundle> LoadedBundles = new Dictionary<uint, ResourceBundle>();
    public readonly Dictionary<uint, BundleGroup> UnloadBundleMap = new Dictionary<uint, BundleGroup>();

    /// <summary>
    /// 将路径转化为Hash值
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>转换后的Hash值</returns>
    private uint ConvertPath(string path)
    {
        uint result = 0;
        if (!string.IsNullOrEmpty(path))
        {
            path = FileUtil.Normalized(path).ToLower().Replace(ResUtil.AssetsSourcesLowerPath, "");
            if (!CacheFileMap.TryGetValue(path, out result))
            {
                result = Util.HashPath(path);
                CacheFileMap.Add(path, result);
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
        lock (_unloadResourceBundlesLock)
        {
            _unloadResourceBundles.Enqueue(bundle);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        _preInitialized = true;
        lock (_unloadResourceBundlesLock) _unloadResourceBundles.Clear();
        _loadingTasks.Clear();
        _finishedList.Clear();
        CacheFileMap.Clear();
        LoadedBundles.Clear();
        LoadingBundles.Clear();
        UnloadBundleMap.Clear();
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

        Manifest = new Manifest()
        {
            ManifestDict = new Dictionary<uint, ManifestItem>(config.items.Count),
        };

        ManifestConfig patchConfig = null;
        if (Setting.Config.enablePatching)
        {
            var patchRcFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "rc.bytes");
            if (System.IO.File.Exists(patchRcFilePath))
            {
                var confJson = System.Text.Encoding.Default.GetString(System.IO.File.ReadAllBytes(patchRcFilePath));
                patchConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(confJson);
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"Enabled Patching But Not Found {patchRcFilePath}");
            }
        }

        var strMainRootPath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
        
        // 整包资源相关配置
        foreach (var item in config.items)
        {
            item.packageResource = true;
            item.packageResourcePath = strMainRootPath;
            Manifest.ManifestDict.Add(item.hash, item);
        }

        if (patchConfig == null) return;
        // 热更资源相关配置
        foreach (var item in patchConfig.items)
        {
            var patchFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s");
            if (!System.IO.File.Exists(patchFilePath))
            {
                continue;
            }

            item.packageResource = false;
            if (Manifest.ManifestDict.ContainsKey(item.hash))
            {
                if (item.directories)
                {
                    item.packageItem = Manifest.ManifestDict[item.hash];
                }
                Manifest.ManifestDict[item.hash] = item;
            }
            else
            {
                Manifest.ManifestDict.Add(item.hash, item);
            }
        }
    }

    public LoadingLocation ToLocation(ManifestItem item)
    {
        var location = new LoadingLocation
        {
            path = item.packageResourcePath,
            location = ELoadingLocation.Package
        };
        return location;
    }

    public LoadingLocation ToLocation(uint hash)
    {
        var item = Manifest.GetItem(hash);
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
    /// <param name="assetName">资源名</param>
    /// <param name="onLoaded">加载完成回调方法</param>
    public void LoadSync(string path, string assetName, System.Action<Resource> onLoaded)
    {
        Load(path, assetName, ConvertPath(path), false, false, onLoaded);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="assetName">资源名</param>
    /// <param name="onLoaded">加载完成回调方法</param>
    public void LoadAsync(string path, string assetName, System.Action<Resource> onLoaded)
    {
        Load(path, assetName, ConvertPath(path), true, false, onLoaded);
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
    private void Load(string path, string assetName, uint hash, bool async, bool isDependency, System.Action<Resource> onLoaded, Dictionary<string, bool> namesDict = null, string extension = null)
    {
        if (_preInitialized)
        {
            if (Setting.Config.useAssetBundle && (Manifest == null || !Manifest.Exist(hash)))
            {
                onLoaded?.Invoke(_errorResource);
            }
        }
        assetName = assetName == null ? "" : assetName.ToLower();
        var task = _loadingTasks.FirstOrDefault(t => t.state == ELoadingState.None);
        if (task == null)
        {
            task = new LoadingTask();
            _loadingTasks.Add(task);
        }
        task.Reset();
        task.state = ELoadingState.Ready;
        task.path = path;
        task.name = assetName;
        task.hash = hash;
        task.extension = extension;
        task.namesDict = namesDict;
        task.async = async;
        task.isDependency = isDependency;

        // 在下一帧开始LoadingTask之前，先加载依赖
        if (Manifest != null)
        {
            var dependencies = Manifest.GetDependencies(task.hash, out task.offset);
            foreach (var t1 in dependencies)
            {
                var found = false;
                foreach (var t in from t in _loadingTasks let tmpTask = t where (tmpTask.state == ELoadingState.Ready || tmpTask.state == ELoadingState.Loading) && task.hash == t1 select t)
                {
                    t.dependencyRefCount++;
                    found = true;
                    break;
                }

                if (found) continue;
                
                var unload = false;
                lock (_unloadResourceBundlesLock)
                {
                    unload = _unloadResourceBundles.Any(unloadBundle => unloadBundle.Hash == t1);
                }
                if (unload || !LoadedBundles.TryGetValue(hash, out var bundle))
                {
                    Load("", null, t1, async, true, null);
                }
                else
                {
                    bundle.Retain();
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
        if (!_preInitialized) return;

        if (!LoadedBundles.TryGetValue(hash, out var bundle)) return;
        LoadedBundles.Remove(hash);
        
        if (Manifest == null) return;

        var dependencies = Manifest.GetDependencies(hash);
        foreach (var t in dependencies)
        {
            if (LoadedBundles.TryGetValue(t, out var tmpBundle))
            {
                tmpBundle?.Release();
            }
        }
    }

    private void LateUpdate()
    {
        if (_preInitialized)
        {
            ReleaseUnloadResourceBundles();

            var loadingCount = 0;
            foreach (var task in _loadingTasks)
            {
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
                    StartCoroutine(Setting.Config.useAssetBundle
                        ? ResLoader.CoLoadTask(task)
                        : ResLoader.CoEditorLoadTask(task));
#else
                    StartCoroutine(ResLoader.CoLoadTask(task));
#endif
                }

                if (task.state != ELoadingState.Finished) continue;
                
                task.state = ELoadingState.None;
                _finishedList.Add(task);
            }

            CallBackFinishedResourceBundles();
        }
        CleanBundleUnloadList();
    }

    /// <summary>
    /// 触发所有已经完成的资源的完成回调
    /// </summary>
    private void CallBackFinishedResourceBundles()
    {
        if(_finishedList.Count <= 0)
        {
            return;
        }
        foreach (var task in _finishedList)
        {
            var task1 = task;
            foreach (var t in task.onLoadedCallbacks.Where(t => task1.file != null))
            {
                task.file.Retain();
                t(task.file);
                task.file.Release();
            }
        }
        _finishedList.Clear();
    }

    /// <summary>
    /// 卸载资源Bundle队列
    /// </summary>
    private void ReleaseUnloadResourceBundles()
    {
        lock (_unloadResourceBundlesLock)
        {
            if(_unloadResourceBundles.Count <= 0)
            {
                return;
            }
        }

        lock (_unloadResourceBundlesLock)
        {
            while (_unloadResourceBundles.Count > 0)
            {
                var bundle = _unloadResourceBundles.Dequeue();
                bundle.RealUnload();
            }
        }
    }

    /// <summary>
    /// 卸载Bundle卸载列表
    /// </summary>
    private void CleanBundleUnloadList()
    {
        using (var e = UnloadBundleMap.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var group = e.Current.Value;
                if (group.RawBundle != null)
                {
                    group.RawBundle.Unload(true);
                }
                if (group.PackageBundle != null)
                {
                    group.PackageBundle.Unload(true);
                }
            }
        }
        UnloadBundleMap.Clear();
    }

    /// <summary>
    /// 清理加载过的Bundle资源
    /// </summary>
    private void CleanBundleLoadedList()
    {
        using (var e = LoadedBundles.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var bundle = e.Current.Value;
                bundle?.UnloadBundle();
            }
        }
        LoadedBundles.Clear();
    }

    public void OnRelease()
    {
        Manifest = null;
        _preInitialized = false;
        IsInitialized = false;
        CleanBundleLoadedList();
        CleanBundleUnloadList();
        LoadingBundles.Clear();
        _loadingTasks.Clear();
        _finishedList.Clear();
        StopAllCoroutines();
        ReleaseUnloadResourceBundles();
    }

}
