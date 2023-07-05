using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class ResLoader : IEnumerator, System.IDisposable
{
    private Resource _resource;
    private bool disposed = false;

    public ResLoader(string path, string name, bool isAsync)
    {
        if (isAsync)
        {
            Global.Instance.ResManager.LoadAsync(path, name, OnLoaded);
        }
        else
        {
            Global.Instance.ResManager.LoadSync(path, name, OnLoaded);
        }
    }

    ~ResLoader()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }
        if (_resource != null)
        {
            _resource.Release();
            _resource = null;
        }
        disposed = true;
    }

    private void OnLoaded(Resource resource)
    {
        _resource = resource;
        _resource.Retain();
    }

    public object Current => _resource;

    public Resource Res => _resource;

    public bool MoveNext()
    {
        return _resource == null;
    }

    public void Reset()
    {
        if (_resource != null)
        {
            _resource.Release();
            _resource = null;
        }
        disposed = false;
    }

}

public partial class ResLoader
{
    public static List<AssetBundleCreateRequest> requestList = new List<AssetBundleCreateRequest>();

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下加载资源核心方法
    /// </summary>
    /// <param name="task">加载资源的任务</param>
    /// <returns></returns>
    public static IEnumerator CoEditorLoadTask(LoadingTask task)
    {
        var path = task.path;
        if (!task.path.Contains(Setting.EditorBundlePath))
        {
            path = FileUtil.CombinePaths(Setting.EditorBundlePath, task.path);
        }
        if (System.IO.Directory.Exists(path) && task.namesDict != null && task.namesDict.Count > 0)
        {
            var loadedFiles = new List<Object>();
            foreach (var kv in task.namesDict)
            {
                var filePath = FileUtil.CombinePaths(path, kv.Key + task.extension);
                loadedFiles.Add(AssetDatabase.LoadMainAssetAtPath(filePath));
            }
            task.file = new Resource(task.path, loadedFiles.ToArray(), null, null);
        }
        else if (System.IO.Directory.Exists(path) && string.IsNullOrEmpty(task.name))
        {
            var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            var loadedFiles = new List<Object>();
            for (var i = 0; i < files.Length; ++i)
            {
                if (!files[i].EndsWith(".meta"))
                {
                    loadedFiles.Add(AssetDatabase.LoadMainAssetAtPath(files[i]));
                }
            }
            task.file = new Resource(task.path, loadedFiles.ToArray(), null, null);
        }
        else
        {
            if (!string.IsNullOrEmpty(task.name))
            {
                var filePath = FileUtil.CombinePaths(path, task.name);
                if (System.IO.File.Exists(filePath))
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(filePath);
                    task.file = new Resource(task.path, task.name, asset, null, null);
                }
                else
                {
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    for (int i = 0; i < assets.Length; i++)
                    {
                        if (assets[i].name == task.name)
                        {
                            task.file = new Resource(task.path, task.name, assets[i], null, null);
                        }
                    }
                    if (assets == null || assets.Length == 0)
                    {
                        var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        foreach (var f in files)
                        {
                            if (!f.EndsWith(".meta") && f.Contains(task.name))
                            {
                                var asset = AssetDatabase.LoadMainAssetAtPath(FileUtil.CombinePaths(path, task.name + System.IO.Path.GetExtension(f)));
                                task.file = new Resource(task.path, task.name, asset, null, null);
                            }
                        }
                    }
                }
            }
            else
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                task.file = new Resource(task.path, task.name, asset, null, null);
            }
        }
        task.state = ELoadingState.Finished;
        yield return null;
    }

#endif

    /// <summary>
    /// 非编辑器模式下加载资源的核心代码
    /// </summary>
    /// <param name="task">加载资源的任务</param>
    /// <returns></returns>
    public static IEnumerator CoLoadTask(LoadingTask task)
    {
        var loadingBundles = Global.Instance.ResManager.loadingBundles;
        var loadedBundles = Global.Instance.ResManager.loadedBundles;
        var unloadBundleMap = Global.Instance.ResManager.unloadBundleMap;
        var manifest = Global.Instance.ResManager.manifest;

        ResourceBundle bundle = null;
        // 正在加载的就等加载完
        if (loadingBundles.Contains(task.hash)) {
            while (true)
            {
                if (!loadingBundles.Contains(task.hash))
                {
                    break;
                }
                yield return null;
            }
        }
        // 没有加载过的
        if(!loadedBundles.TryGetValue(task.hash, out bundle)){
            loadingBundles.Add(task.hash);

            // 依赖资源是否加载完了都
            var dependencies = manifest.GetDependencies(task.hash);
            while (true)
            {
                var loadedCount = 0;
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (loadedBundles.ContainsKey(dependencies[i]))
                    {
                        loadedCount++;
                    }
                }
                if(loadedCount == dependencies.Length)
                {
                    break;
                }
                yield return null;
            }
            BundleGroup bundleGroup;
            string error = null;
            var location = Global.Instance.ResManager.ToLocation(task.hash);
            if(unloadBundleMap.TryGetValue(task.hash, out bundleGroup))
            {
                unloadBundleMap.Remove(task.hash);
            }
            // 加载AssetBundle
            else if (task.hash != default(uint))
            {
                if (task.async)
                {
                    AssetBundleCreateRequest assetBundleRequest = null;
                    AssetBundleCreateRequest packageAssetBundleRequest = null;
                    try
                    {
                        assetBundleRequest = AssetBundle.LoadFromFileAsync(location.path, 0, task.offset);
                        requestList.Add(assetBundleRequest);
                        var item = manifest.GetItem(task.hash);
                        if(item != null && item.packageItem != null)
                        {
                            var packageLocation = Global.Instance.ResManager.ToLocation(item.packageItem);
                            try
                            {
                                packageAssetBundleRequest = AssetBundle.LoadFromFileAsync(packageLocation.path, 0, item.packageItem.offset);
                                requestList.Add(packageAssetBundleRequest);
                            }
                            catch
                            {
                                error = string.Format("[ResManager] CRC or Load Error!{0}-{1}", packageLocation.path, item.packageItem.hash);
                            }
                        }
                    }
                    catch
                    {
                        error = string.Format("[ResManager] CRC or Load Error!{0}-{1}", task.path, task.hash);
                    }

                    if(assetBundleRequest != null)
                    {
                        while (!assetBundleRequest.isDone)
                        {
                            yield return null;
                        }
                        bundleGroup.rawBundle = assetBundleRequest.assetBundle;
                        requestList.Remove(assetBundleRequest);
                        if(packageAssetBundleRequest != null)
                        {
                            while (!packageAssetBundleRequest.isDone)
                            {
                                yield return null;
                            }
                            bundleGroup.packageBundle = packageAssetBundleRequest.assetBundle;
                            requestList.Remove(packageAssetBundleRequest);
                        }
                    }
                }
                else
                {
                    try
                    {
                        bundleGroup.rawBundle = AssetBundle.LoadFromFile(location.path, 0, task.offset);
                        var item = manifest.GetItem(task.hash);
                        if(item != null && item.packageItem != null)
                        {
                            var packageLocation = Global.Instance.ResManager.ToLocation(item.packageItem);
                            try
                            {
                                bundleGroup.packageBundle = AssetBundle.LoadFromFile(packageLocation.path, 0, item.packageItem.offset);
                            }
                            catch
                            {
                                error = string.Format("[ResourceManager] CRC or Load Error!{0}-{1}", packageLocation.path, item.packageItem.hash);
                            }
                        }
                    }
                    catch
                    {
                        error = string.Format("[ResManager] CRC or Load Error!{0}-{1}", task.path, task.hash);
                    }
                }
            }
            if(bundleGroup.rawBundle == null && bundleGroup.packageBundle == null)
            {
                task.file = new Resource(task.path, task.name, null, null, error != null ? error : string.Format("[ResourceManager] Load Error!{0}-{1}", task.path, task.hash));
                task.state = ELoadingState.Finished;
                yield break;
            }
            bundle = new ResourceBundle(task.hash, bundleGroup.rawBundle, bundleGroup.packageBundle, Global.Instance.ResManager);
            loadedBundles.Add(task.hash, bundle);
            loadingBundles.Remove(task.hash);
        }
        // 已经加载过有缓存的
        else
        {
            bundle.Retain();
            var dependencies = manifest.GetDependencies(bundle.Hash);
            foreach (var dep in dependencies)
            {
                loadedBundles[dep].Release();
            }
        }

        for (int i = 0; i < task.dependencyRefCount; i++)
        {
            bundle.Release();
        }

        if (task.isDependency)
        {
            task.state = ELoadingState.Finished;
            yield break;
        }

        string name = null;
        bool stream = bundle.isStreamedSceneAssetBundle;
        if (!stream)
        {
            if (!string.IsNullOrEmpty(task.name))
            {
                name = task.name;
            }
            else
            {
                var item = manifest.GetItem(task.hash);
                if(bundle.isSingleObject && item != null && !item.directories)
                {
                    name = "_";
                }
            }
        }
        if (task.async)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var item = manifest.GetItem(task.hash);
                if (item != null && item.directories)
                {
                    task.file = new Resource(task.path, task.name, bundle.LoadAsset(name), bundle, null);
                }
                else
                {
                    AssetBundleRequest request = bundle.LoadAssetAsync(name);
                    while (!request.isDone)
                    {
                        yield return null;
                    }
                    task.file = new Resource(task.path, task.name, request.asset, bundle, null);
                }
            }
            else
            {
                if (stream)
                {
                    task.file = new Resource(task.path, null, bundle, null);
                }
                else if(task.namesDict != null && task.namesDict.Count > 0)
                {
                    var loadedFiles = new Object[task.namesDict.Count];
                    var index = 0;
                    var item = manifest.GetItem(task.hash);
                    foreach (var kv in task.namesDict)
                    {
                        if(item != null && item.directories)
                        {
                            loadedFiles[index++] = bundle.LoadAsset(kv.Key + task.extension);
                        }
                        else
                        {
                            AssetBundleRequest request = bundle.LoadAssetAsync(kv.Key + task.extension);
                            while (!request.isDone)
                            {
                                yield return null;
                            }
                            loadedFiles[index++] = request.asset;
                        }
                    }
                    task.file = new Resource(task.path, loadedFiles, bundle, null);
                }
                else
                {
                    AssetBundleRequest request = null;
                    var loadAllRequest = bundle.LoadAllAssetsAsync(null, out request);
                    yield return loadAllRequest;
                    if(request != null && !request.isDone)
                    {
                        yield return request;
                    }
                    task.file = new Resource(task.path, bundle.LoadAllAssetsFromRequest(loadAllRequest, request), bundle, null);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(name))
            {
                task.file = new Resource(task.path, task.name, bundle.LoadAsset(name), bundle, null);
            }
            else
            {
                if (stream)
                {
                    task.file = new Resource(task.path, null, bundle, null);
                }
                else if(task.namesDict != null && task.namesDict.Count > 0)
                {
                    var loadedFiles = new Object[task.namesDict.Count];
                    var index = 0;
                    foreach (var kv in task.namesDict)
                    {
                        loadedFiles[index++] = bundle.LoadAsset(kv.Key + task.extension);
                    }
                    task.file = new Resource(task.path, loadedFiles, bundle, null);
                }
                else
                {
                    task.file = new Resource(task.path, bundle.LoadAllAssets(), bundle, null);
                }
            }
        }
        if(task.file != null)
        {
            bundle.Release();
        }
        task.state = ELoadingState.Finished;
    }

}
