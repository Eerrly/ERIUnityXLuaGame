using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System;

/// <summary>
/// 资源工具
/// </summary>
public class ResUtil
{
    /// <summary>
    /// 可热更资源根目录（小写）
    /// </summary>
    public const string AssetsSourcesLowerPath = "assets/sources/";

    public static string GetAtlasPathBySpritePath(string spritePath)
    {
        var atlasName = FileUtil.CombinePaths("Sources", Path.GetDirectoryName(spritePath)).Replace("/", "_").ToLower();
        var atlasPath = FileUtil.CombinePaths(Setting.EditorSpriteAtlasPath, atlasName) + Constant.ATLASSPRITE_EXTENSION;
        return atlasPath;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        return fileName;
    }

    private static string[] GetAddressableNames(string root, string[] rawNames)
    {
        var names = new string[rawNames.Length];
        for (var i = 0; i < names.Length; ++i)
        {
            names[i] = FileUtil.Normalized(rawNames[i]).ToLower().Replace(root + "/", "");
        }
        return names;
    }

    /// <summary>
    /// 检测资源循环依赖
    /// </summary>
    /// <param name="manifest">AB资源清单</param>
    /// <param name="hash2Name">Hash转资源名字典</param>
    /// <param name="name">AB包名</param>
    /// <param name="tracker">AB包名的栈</param>
    /// <exception cref="System.Exception"></exception>
    private static void CheckLoop(AssetBundleManifest manifest, IReadOnlyDictionary<string, string> hash2Name, string name, Stack<string> tracker)
    {
        if (tracker.Any(sub => name == sub))
        {
            var sb = new System.Text.StringBuilder();
            tracker.Push(name);
            while (tracker.Count > 0)
            {
                sb.AppendLine(hash2Name[tracker.Pop()]);
            }
            throw new System.Exception("出现循环依赖！！\n" + sb.ToString());
        }

        // AB包名入栈，如果这个AB包名的依赖关系中又包含此AB包名，则为循环依赖，抛出异常 （如果没有循环依赖，栈顶为依赖关系的最后一环）
        tracker.Push(name);
        var dependencies = new Queue<string>(manifest.GetAllDependencies(name));
        foreach (var dependency in dependencies)
        {
            var count = tracker.Count;
            CheckLoop(manifest, hash2Name, dependency, tracker);
            // 如果走到这，说明不包含循环依赖，由依赖关系的最后一环向前依次出栈
            while (tracker.Count > count)
            {
                tracker.Pop();
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 构建Lua脚本资源
    /// </summary>
    private static void BuildLuaScripts()
    {
        var start = System.DateTime.Now;
        try
        {
            var files = Directory.GetFiles(Setting.EditorLuaScriptRoot, "*.lua", SearchOption.AllDirectories).ToList();
            if (!(CompilingLuaScripts(files, "32", true) && CompilingLuaScripts(files, "64", true)))
            {
                Debug.LogError("构建Lua脚本发生错误！！");
            }
            AssetDatabase.Refresh();
        }
        catch(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            UnityEngine.Debug.Log("构建全部Lua脚本耗时：" + (System.DateTime.Now - start).TotalMilliseconds + " ms");
        }
    }

    /// <summary>
    /// 通过LuaJit编译Lua脚本
    /// </summary>
    /// <param name="files">Lua脚本列表</param>
    /// <param name="tag">系统架构</param>
    /// <param name="checkError">是否检测Lua错误</param>
    /// <returns></returns>
    private static bool CompilingLuaScripts(IReadOnlyList<string> files, string tag, bool checkError)
    {
        var luaTargetDirectory = FileUtil.CombinePaths(Application.dataPath.Replace("/Assets", ""), Setting.EditorScriptBundleName, tag);
        if (!Directory.Exists(luaTargetDirectory))
        {
            Directory.CreateDirectory(luaTargetDirectory);
        }

        var hasError = false;
        var luajit = FileUtil.CombinePaths(Application.dataPath, string.Format("Examples/Tools/LuaJit/luajit{0}.exe", tag));
        // Lua虚拟机（里面封装了Lua解释器对象，用来执行Lua脚本）
        var L = XLua.LuaDLL.Lua.luaL_newstate();
        try
        {
            foreach (var t in files)
            {
                var targetFile = FileUtil.CombinePaths(luaTargetDirectory, t.Replace(".lua", ".bytes").Replace(Setting.EditorLuaScriptRoot, ""));
                var index = targetFile.LastIndexOf("/", StringComparison.Ordinal);
                var targetFileDir = targetFile.Substring(0, index);
                if (!Directory.Exists(targetFileDir))
                {
                    FileUtil.CreateDirectory(targetFileDir);
                }
                if (Directory.Exists(t)) continue;
                
                var bytes = File.ReadAllBytes(t);
                // 如果前3个字节为 0xEF、0xBB、0xBF，说明该文件以UTF-8编码，且包含BOM（用于标识文件字节序的特殊字节序列）。为了去除BOM，则从第4个字节开始
                if (bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                {
                    var temp = new byte[bytes.Length - 3];
                    Array.Copy(bytes, 3, temp, 0, bytes.Length - 3);
                    bytes = temp;
                }
                // 加载Lua脚本 看是否有错误
                if (checkError && XLua.LuaDLL.Lua.xluaL_loadbuffer(L, bytes, bytes.Length, t) != 0)
                {
                    hasError = true;
                    var error = XLua.LuaDLL.Lua.lua_tostring(L, -1);
                    UnityEngine.Debug.LogError(error);
                }
                // 编译Lua脚本
                if (Util.ExecuteBat(Path.GetDirectoryName(luajit), luajit, $"-b {t} {targetFile}") == 1)
                {
                    hasError = true;
                    UnityEngine.Debug.LogError("LuaJit编译脚本发生错误！ 脚本：" + t);
                }
            }

            AssetDatabase.ImportAsset(luaTargetDirectory, ImportAssetOptions.Default);
        }
        catch(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            XLua.LuaDLL.Lua.lua_close(L);
        }

        return !hasError;
    }

    /// <summary>
    /// 构建资源
    /// </summary>
    public static void Build()
    {
        var start = System.DateTime.Now;

        var version = PatchUtil.GetGitVersion();
        var versionPath = FileUtil.CombinePaths(Setting.EditorResourcePath, Setting.EditorConfigPath, Constant.VERSION_TXT_NAME);
        File.WriteAllText(versionPath, version);
        AssetDatabase.ImportAsset(versionPath, ImportAssetOptions.ForceUpdate);

        var cfg = Util.LoadConfig<BuildToolsConfig>(Constant.CLIENT_CONFIG_NAME);
        var bundleList = new List<AssetBundleBuild>();
        var hash2Name = new Dictionary<string, string>();
        var hash2Path = new Dictionary<string, string>();
        var mainBundleList = new List<string>();
        var configItemMap = new Dictionary<string, BuildToolsConfig.BuildToolsConfigItem>();

        BuildLuaScripts();

        foreach (var cur in cfg.itemList)
        {
            string[] items = null;
            if (cur.directories)
            {
                items = Directory.GetDirectories(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchOption);
            }
            else
            {
                items = Directory.GetFiles(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchOption);
            }
            foreach (var item in items)
            {
                var path = FileUtil.Normalized(item).ToLower();
                var keyPath = path.Replace(ResUtil.AssetsSourcesLowerPath, "");
                if (keyPath.EndsWith(".meta"))
                {
                    continue;
                }
                var name = Util.HashPath(keyPath).ToString() + ".s";
                hash2Name.Add(name, path);
                hash2Path.Add(name, keyPath);
                configItemMap.Add(name, cur);
                if (cur.directories)
                {
                    var subItems = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories);
                    var newList = new List<string>();
                    foreach (var subItem in subItems)
                    {
                        if (!Path.GetFileName(subItem).Contains("."))
                        {
                            continue;
                        }

                        if (!subItem.EndsWith(".meta"))
                        {
                            newList.Add(subItem);
                        }
                    }
                    var refItem = newList.ToArray();
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        assetNames = refItem,
                        addressableNames = GetAddressableNames(path, refItem),
                    });
                    mainBundleList.Add(name);
                }
                else
                {
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        addressableNames = new string[] { "_" },
                        assetNames = new string[] { item },
                    });
                    mainBundleList.Add(name);
                }
            }
        }

        AssetBundleManifest manifest = null;
        FileUtil.CreateDirectory(Setting.EditorBundleBuildCachePath);
        manifest = BuildPipeline.BuildAssetBundles(
            Setting.EditorBundleBuildCachePath,
            bundleList.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension,
            EditorUserBuildSettings.activeBuildTarget);

        var bundleNames = manifest.GetAllAssetBundles();
        var bundleManifest = new ManifestConfig();
        var mainBundleItems = new List<ManifestItem>(bundleNames.Length);

        // 循环检测依赖
        foreach (var t in bundleNames)
        {
            var tracker = new Stack<string>();
            CheckLoop(manifest, hash2Name, t, tracker);
        }

        if (Directory.Exists(Setting.StreamingBundleRoot))
        {
            Directory.Delete(Setting.StreamingBundleRoot, true);
        }
        FileUtil.CreateDirectory(Setting.StreamingRoot);
        FileUtil.CreateDirectory(Setting.StreamingBundleRoot);

        var abMainFilePath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
        var abMainFile = new FileStream(abMainFilePath, FileMode.Create);

        // 加密
        var head = new byte[] { 0xAA, 0xBB, 0x10, 0x12 };
        abMainFile.Write(head, 0, head.Length);

        var offset = (uint)head.Length;
        foreach (var t in bundleNames)
        {
            if (!mainBundleList.Contains(t)) continue;
            
            var dependencies = manifest.GetAllDependencies(t);
            foreach (var t1 in dependencies)
            {
                if (!mainBundleList.Contains(t1))
                {
                    mainBundleList.Add(t1);
                }
            }
        }

        // 合包
        foreach (var t in bundleNames)
        {
            var hash = t.Substring(0, t.Length - 2);
            var bytes = File.ReadAllBytes(FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, t));
            if (mainBundleList.Contains(t))
            {
                abMainFile.Write(bytes, 0, bytes.Length);
            }
            var strDependencies = manifest.GetAllDependencies(t);
            var uintDependencies = strDependencies.Select(t1 => uint.Parse(t1.Replace(".s", ""))).ToList();
            if (mainBundleList.Contains(t))
            {
                mainBundleItems.Add(new ManifestItem()
                {
                    hash = uint.Parse(hash),
                    dependencies = uintDependencies,
                    offset = offset,
                    size = bytes.Length,
                    directories = configItemMap[t].directories,
                    packageResourcePath = hash2Path.TryGetValue(t, out var value) ? value : string.Empty,
                    md5 = Util.MD5(bytes),
                });
            }
            offset += (uint)bytes.Length;
        }
        abMainFile.Close();
        AssetDatabase.ImportAsset(Setting.StreamingBundleRoot, ImportAssetOptions.ForceUpdate);

        bundleManifest.items = mainBundleItems;
        Util.SaveConfig(bundleManifest, Constant.ASSETBUNDLES_CONFIG_NAME);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("构建整包资源总耗时：" + (System.DateTime.Now - start).TotalMilliseconds + " ms");
    }

    /// <summary>
    /// 构建热更
    /// </summary>
    /// <param name="patchList">热更文件列表</param>
    public static void Patch(HashSet<string> patchList)
    {
        var start = System.DateTime.Now;

        var patchMap = new Dictionary<string, string>();
        var bundleList = new List<AssetBundleBuild>();
        var configMap = new Dictionary<string, BuildToolsConfig.BuildToolsConfigItem>();
        var hash2Path = new Dictionary<string, string>();

        var resourceVersion = PatchUtil.GetGitVersion();

        BuildLuaScripts();
        var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var assetBundleName in assetBundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(assetBundleName, true);
        }
        var patchingNoteList = new System.Text.StringBuilder();

        foreach (var cur in Setting.Config.itemList)
        {
            string[] files = null;
            if (cur.directories)
            {
                files = Directory.GetDirectories(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchOption);
            }
            else
            {
                files = Directory.GetFiles(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchOption);
            }
            foreach (var item in files)
            {
                var path = FileUtil.Normalized(item).ToLower();
                var keyPath = path.Replace(ResUtil.AssetsSourcesLowerPath, "");
                if (keyPath.EndsWith(".meta"))
                {
                    continue;
                }
                if(!cur.directories && patchList.Contains(keyPath))
                {
                    patchMap.Add(keyPath, "");
                    patchingNoteList.AppendLine("patch:" + keyPath);
                    patchList.Remove(keyPath);
                }

                var name = Util.HashPath(keyPath).ToString() + ".s";
                configMap.Add(name, cur);
                hash2Path.Add(name, keyPath);

                if (cur.directories)
                {
                    var patchItems = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories);
                    var newList = new List<string>();
                    foreach (var patchItem in patchItems)
                    {
                        if (patchItem.EndsWith(".meta"))
                        {
                            continue;
                        }

                        if (!Path.GetFileName(patchItem).Contains("."))
                        {
                            continue;
                        }

                        var patchPath = patchItem.Replace('\\', '/').ToLower().Replace(ResUtil.AssetsSourcesLowerPath, "");
                        if(patchPath.StartsWith("lua/32/", StringComparison.OrdinalIgnoreCase) || patchPath.StartsWith("lua/64/", StringComparison.OrdinalIgnoreCase))
                        {
                            patchPath = patchPath.Substring(7, patchPath.Length - 7).Replace(".bytes", ".lua");
                        }
                        if (patchList.Contains(patchPath))
                        {
                            patchingNoteList.AppendLine($"patch:{patchItem}");
                            newList.Add(patchItem);
                        }
                    }

                    if (newList.Count == 0)
                    {
                        continue;
                    }

                    var refItems = newList.ToArray();
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name + ".p",
                        assetNames = refItems,
                        addressableNames = GetAddressableNames(path, refItems)
                    });
                    patchMap.Add(keyPath, "");
                }
                else
                {
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        addressableNames = new string[] { "_" },
                        assetNames = new string[] { item },
                    });
                }
            }
        }

        if (patchList.Count > 0)
        {
            var withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat" };
            var files = Directory.GetFiles(Setting.EditorBundlePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            var depMap = new Dictionary<string, List<string>>(files.Length);
            foreach (var file in files)
            {
                var rawFile = file.Replace("\\", "/");
                var keyPath = rawFile.ToLower().Replace(ResUtil.AssetsSourcesLowerPath, "");
                var deps = AssetDatabase.GetDependencies(rawFile, false);
                foreach (var dep in deps)
                {
                    var depKeyPath = dep.ToLower().Replace("\\", "/").Replace(ResUtil.AssetsSourcesLowerPath, "");
                    List<string> list = null;
                    if (!depMap.TryGetValue(depKeyPath, out list))
                    {
                        list = new List<string>();
                        depMap.Add(depKeyPath, list);
                    }
                    list.Add(keyPath);
                }
            }
            var checkQueue = new Queue<string>();
            foreach (var patch in patchList)
            {
                checkQueue.Enqueue(patch);
            }
            while (checkQueue.Count > 0)
            {
                var patch = checkQueue.Dequeue();
                if (!depMap.TryGetValue(patch, value: out var list)) continue;
                
                foreach (var parent in list)
                {
                    var key = Util.HashPath(parent) + ".s";
                    if (configMap.TryGetValue(key, out _) && !patchMap.ContainsKey(parent))
                    {
                        patchMap.Add(parent, "");
                        patchingNoteList.AppendLine($"patch:{parent}");
                    }
                }
            }
        }

        AssetBundleManifest manifest = null;
        FileUtil.CreateDirectory(Setting.EditorBundleBuildCachePath);
        manifest = BuildPipeline.BuildAssetBundles(
            Setting.EditorBundleBuildCachePath,
            bundleList.ToArray(),
            //使用lz4的格式压缩|不使用FileName来加载ab|不使用带后缀的文件名来加载ab
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension,
            EditorUserBuildSettings.activeBuildTarget);

        var assetsRootPath = Application.dataPath.Replace("/Assets", "");
        var patchFilePath = FileUtil.CombinePaths(assetsRootPath, UnityEditor.FileUtil.GetUniqueTempPathInProject());
        var versionPatchFilePath = FileUtil.CombinePaths(patchFilePath, resourceVersion.ToString());
        FileUtil.CreateDirectory(patchFilePath);
        FileUtil.CreateDirectory(versionPatchFilePath);

        var bundleNames = manifest.GetAllAssetBundles();
        var bundleManifestFile = new ManifestConfig();
        var items = new List<ManifestItem>(bundleNames.Length);

        for (var i = 0; i < bundleNames.Length; ++i)
        {
            var isPatchAb = bundleNames[i].EndsWith(".p");
            bundleNames[i] = isPatchAb ? bundleNames[i].Replace(".p", "") : bundleNames[i];
            var hash = bundleNames[i].Substring(0, bundleNames[i].Length - 2);
            var finalName = hash + ".s";
            if (!patchMap.ContainsKey(hash2Path[finalName])) continue;
            
            var destFile = FileUtil.CombinePaths(versionPatchFilePath, bundleNames[i]);
            var sourceBytes = File.ReadAllBytes(FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, isPatchAb ? bundleNames[i] + ".p" : bundleNames[i]));
            if (File.Exists(destFile)) File.Delete(destFile);
            File.WriteAllBytes(destFile, sourceBytes);

            var nameDependencies = manifest.GetAllDependencies(bundleNames[i]);
            var dependencies = nameDependencies.Select(t1 => uint.Parse(t1.Replace(".s", "").Replace(".p", ""))).ToList();
            var name = hash + ".s";
            items.Add(new ManifestItem() {
                hash = uint.Parse(hash),
                dependencies = dependencies,
                offset = 0,
                size = sourceBytes.Length,
                directories = configMap[bundleNames[i]].directories,
                md5 = Util.MD5(sourceBytes),
            });
        }

        bundleManifestFile.items = items;
        AssetDatabase.ImportAsset(Setting.StreamingBundleRoot, ImportAssetOptions.ForceUpdate);

        var jsonTexts = JsonUtility.ToJson(bundleManifestFile);
        var listFile = FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, "rc.txt");
        File.WriteAllText(listFile, jsonTexts);

        var manifestFilePath = FileUtil.CombinePaths(versionPatchFilePath, "rc.bytes");
        File.WriteAllText(manifestFilePath, jsonTexts);

        File.WriteAllText(FileUtil.CombinePaths(patchFilePath, "v.bytes"), resourceVersion.ToString() + "," + Util.MD5(File.ReadAllBytes(manifestFilePath)));

        var compressed = new MemoryStream();
        var compressor = new ZipOutputStream(compressed);
        var fileMap = Directory.GetFiles(patchFilePath, "*.*", SearchOption.AllDirectories);
        foreach (var file in fileMap)
        {
            var filename = file.Substring(patchFilePath.Length, file.Length - patchFilePath.Length);
            var entry = new ZipEntry(filename)
            {
                DateTime = new DateTime(),
                DosTime = 0
            };
            compressor.PutNextEntry(entry);
            if (Directory.Exists(file))
            {
                continue;
            }
            var bytes = File.ReadAllBytes(file);
            compressor.Write(bytes, 0, bytes.Length);
        }
        if (patchingNoteList.Length > 0)
        {
            var filename = $"NOTE_{resourceVersion}.txt";
            var entry = new ZipEntry(filename)
            {
                DateTime = new DateTime(),
                DosTime = 0
            };
            compressor.PutNextEntry(entry);
            var bytes = System.Text.Encoding.Default.GetBytes(patchingNoteList.ToString());
            compressor.Write(bytes, 0, bytes.Length);
        }
        
        compressor.Finish();
        compressed.Flush();

        if (!Directory.Exists(Setting.EditorPatchPath))
        {
            Directory.CreateDirectory(Setting.EditorPatchPath);
        }
        var fileBytes = new byte[compressed.Length];
        Array.Copy(compressed.GetBuffer(), fileBytes, fileBytes.Length);
        var fileName = string.Format("{0}/{1}-{2:yyyy.MM.dd_HH.mm.s}-{3}.zip",
            Setting.EditorPatchPath,
            resourceVersion, 
            DateTime.Now,
            Util.MD5(fileBytes));
        using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            fs.Write(fileBytes, 0, fileBytes.Length);
        }

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("构建热更资源总耗时：" + (System.DateTime.Now - start).TotalMilliseconds + " ms");
    }

    private struct DumpCacheNode
    {
        public string Name;
        public ResourceBundle Bundle;
    }

    public static string Dump()
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("[ResourceManager]");
        builder.AppendLine("\tLoaded");

        var list = new List<DumpCacheNode>();
        var r = Global.Instance.ResManager;

        using (var e = r.LoadedBundles.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var name = e.Current.Key.ToString();
                using (var ee = r.CacheFileMap.GetEnumerator())
                {
                    while (ee.MoveNext())
                    {
                        if (ee.Current.Value != e.Current.Key) continue;
                        
                        name = name + "(" + ee.Current.Key + ")";
                        break;
                    }
                }
                list.Add(new DumpCacheNode() { Name = name, Bundle = e.Current.Value });
            }
        }

        list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        using (var e = list.GetEnumerator())
        {
            while (e.MoveNext())
            {
                builder.AppendLine(e.Current.Name.ToString());
            }
        }

        return builder.ToString();
    }

#endif

}
