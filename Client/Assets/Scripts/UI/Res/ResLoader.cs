using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class ResLoader : IEnumerator, System.IDisposable
{
    private Resource _resource;
    private bool _disposed = false;

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
        if (_disposed)
        {
            return;
        }
        if (_resource != null)
        {
            _resource.Release();
            _resource = null;
        }
        _disposed = true;
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
        _disposed = false;
    }

}

public partial class ResLoader
{
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
            task.file = new Resource(task.path, (from t in files where !t.EndsWith(".meta") select AssetDatabase.LoadMainAssetAtPath(t)).ToArray(), null, null);
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
                    foreach (var t in assets)
                    {
                        if (t.name == task.name)
                        {
                            task.file = new Resource(task.path, task.name, t, null, null);
                        }
                    }
                    if (assets.Length == 0)
                    {
                        var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        foreach (var f in files)
                        {
                            if (f.EndsWith(".meta") || !f.Contains(task.name)) continue;
                            
                            var asset = AssetDatabase.LoadMainAssetAtPath(FileUtil.CombinePaths(path, task.name + System.IO.Path.GetExtension(f)));
                            task.file = new Resource(task.path, task.name, asset, null, null);
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
        var loadingBundles = Global.Instance.ResManager.LoadingBundles;
        var loadedBundles = Global.Instance.ResManager.LoadedBundles;
        var unloadBundleMap = Global.Instance.ResManager.UnloadBundleMap;
        var manifest = Global.Instance.ResManager.Manifest;

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
        if(!loadedBundles.TryGetValue(task.hash, out var bundle)){
            loadingBundles.Add(task.hash);

            // 依赖资源是否加载完了都
            var dependencies = manifest.GetDependencies(task.hash);
            while (true)
            {
                var loadedCount = dependencies.Count(t => loadedBundles.ContainsKey(t));
                if(loadedCount == dependencies.Count)
                {
                    break;
                }
                yield return null;
            }

            string error = null;
            var location = Global.Instance.ResManager.ToLocation(task.hash);
            if(unloadBundleMap.TryGetValue(task.hash, out var bundleGroup))
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
                        var item = manifest.GetItem(task.hash);
                        if(item != null && item.packageItem != null)
                        {
                            var packageLocation = Global.Instance.ResManager.ToLocation(item.packageItem);
                            try
                            {
                                packageAssetBundleRequest = AssetBundle.LoadFromFileAsync(packageLocation.path, 0, item.packageItem.offset);
                            }
                            catch
                            {
                                error = $"加载资源发生错误! 路径：{packageLocation.path} Hash值{item.packageItem.hash}";
                            }
                        }
                    }
                    catch
                    {
                        error = $"加载资源发生错误! 路径：{task.path} Hash值{task.hash}";
                    }

                    if(assetBundleRequest != null)
                    {
                        while (!assetBundleRequest.isDone)
                        {
                            yield return null;
                        }
                        bundleGroup.RawBundle = assetBundleRequest.assetBundle;
                        if(packageAssetBundleRequest != null)
                        {
                            while (!packageAssetBundleRequest.isDone)
                            {
                                yield return null;
                            }
                            bundleGroup.PackageBundle = packageAssetBundleRequest.assetBundle;
                        }
                    }
                }
                else
                {
                    try
                    {
                        bundleGroup.RawBundle = AssetBundle.LoadFromFile(location.path, 0, task.offset);
                        var item = manifest.GetItem(task.hash);
                        if(item?.packageItem != null)
                        {
                            var packageLocation = Global.Instance.ResManager.ToLocation(item.packageItem);
                            try
                            {
                                bundleGroup.PackageBundle = AssetBundle.LoadFromFile(packageLocation.path, 0, item.packageItem.offset);
                            }
                            catch
                            {
                                error = $"加载资源发生错误! 路径：{packageLocation.path} Hash值{item.packageItem.hash}";
                            }
                        }
                    }
                    catch
                    {
                        error = $"加载资源发生错误! 路径：{task.path} Hash值{task.hash}";
                    }
                }
            }
            if(bundleGroup.RawBundle == null && bundleGroup.PackageBundle == null)
            {
                task.file = new Resource(task.path, task.name, null, null, error ?? $"加载资源发生错误! 路径：{task.path} Hash值{task.hash}");
                task.state = ELoadingState.Finished;
                yield break;
            }
            bundle = new ResourceBundle(task.hash, bundleGroup.RawBundle, bundleGroup.PackageBundle, Global.Instance.ResManager);
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

        for (var i = 0; i < task.dependencyRefCount; i++)
        {
            bundle.Release();
        }

        if (task.isDependency)
        {
            task.state = ELoadingState.Finished;
            yield break;
        }

        string name = null;
        var stream = bundle.isStreamedSceneAssetBundle;
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
                    var request = bundle.LoadAssetAsync(name);
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
                            var request = bundle.LoadAssetAsync(kv.Key + task.extension);
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
                    var loadAllRequest = bundle.LoadAllAssetsAsync(null, out var request);
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
