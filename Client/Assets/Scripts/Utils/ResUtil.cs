using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ResUtil
{
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

    private static void CheckLoop(AssetBundleManifest manifest, Dictionary<string, string> hash2Name, string name, Stack<string> tracker)
    {
        foreach (var sub in tracker)
        {
            if (name == sub)
            {
                var sb = new System.Text.StringBuilder();
                tracker.Push(name);
                while (tracker.Count > 0)
                {
                    sb.AppendLine(hash2Name[tracker.Pop()]);
                }
                throw new System.Exception("loop dependencies!\n" + sb.ToString());
            }
        }

        tracker.Push(name);
        var dependencies = new Queue<string>(manifest.GetAllDependencies(name));
        foreach (var dependency in dependencies)
        {
            var count = tracker.Count;
            CheckLoop(manifest, hash2Name, dependency, tracker);
            while (tracker.Count > count)
            {
                tracker.Pop();
            }
        }
    }

#if UNITY_EDITOR
    private static void BuildLuaScripts()
    {
        var luaDirectory = Path.Combine(Application.dataPath.Replace("/Assets", ""), Setting.EditorScriptRoot);
        var luaTargetDirectory = Path.Combine(Application.dataPath.Replace("Assets", ""), Setting.RuntimeScriptBundleName);
        Directory.Delete(luaTargetDirectory, true);
        var files = Directory.GetFiles(luaDirectory, "*.lua", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            var localFilePath = files[i].Replace(luaDirectory, "").Replace(".lua", ".bytes");
            FileUtil.CopyFile(files[i], luaTargetDirectory + "/" + localFilePath);
        }
        AssetDatabase.Refresh();
    }

    public static void Build()
    {
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
                items = Directory.GetDirectories(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            else
            {
                items = Directory.GetFiles(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            foreach (var item in items)
            {
                var path = FileUtil.Normalized(item).ToLower();
                var keyPath = path.Replace("assets/sources/", "");
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

        // loop check
        for (var i = 0; i < bundleNames.Length; ++i)
        {
            var tracker = new Stack<string>();
            CheckLoop(manifest, hash2Name, bundleNames[i], tracker);
        }

        if (Directory.Exists(Setting.StreamingBundleRoot))
        {
            Directory.Delete(Setting.StreamingBundleRoot, true);
        }
        FileUtil.CreateDirectory(Setting.StreamingRoot);
        FileUtil.CreateDirectory(Setting.StreamingBundleRoot);

        var abMainFilePath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
        var abMainFile = new FileStream(abMainFilePath, FileMode.Create);

        var head = new byte[] { 0xAA, 0xBB, 0x10, 0x12 };
        abMainFile.Write(head, 0, head.Length);

        uint offset = (uint)head.Length;
        for (int i = 0; i < bundleNames.Length; i++)
        {
            if (mainBundleList.Contains(bundleNames[i]))
            {
                var dependencies = manifest.GetAllDependencies(bundleNames[i]);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    if (!mainBundleList.Contains(dependencies[j]))
                    {
                        mainBundleList.Add(dependencies[j]);
                    }
                }
            }
        }

        for (int i = 0; i < bundleNames.Length; i++)
        {
            var hash = bundleNames[i].Substring(0, bundleNames[i].Length - 2);
            var bytes = File.ReadAllBytes(FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, bundleNames[i]));
            if (mainBundleList.Contains(bundleNames[i]))
            {
                abMainFile.Write(bytes, 0, bytes.Length);
            }
            var strDependencies = manifest.GetAllDependencies(bundleNames[i]);
            var uintDependencies = new uint[strDependencies.Length];
            for (int j = 0; j < strDependencies.Length; j++)
            {
                uintDependencies[j] = uint.Parse(strDependencies[j].Replace(".s", ""));
            }
            if (mainBundleList.Contains(bundleNames[i]))
            {
                mainBundleItems.Add(new ManifestItem()
                {
                    hash = uint.Parse(hash),
                    dependencies = uintDependencies,
                    offset = offset,
                    size = bytes.Length,
                    directories = configItemMap[bundleNames[i]].directories,
                    extension = configItemMap[bundleNames[i]].extension,
                    packageResourcePath = hash2Path.ContainsKey(bundleNames[i]) ? hash2Path[bundleNames[i]] : string.Empty,
                });
            }
            offset += (uint)bytes.Length;
        }
        abMainFile.Close();
        AssetDatabase.ImportAsset(Setting.StreamingBundleRoot, ImportAssetOptions.ForceUpdate);

        bundleManifest.items = mainBundleItems.ToArray();
        Util.SaveConfig(bundleManifest, Constant.ASSETBUNDLES_CONFIG_NAME);

        AssetDatabase.Refresh();
    }

#endif

}
