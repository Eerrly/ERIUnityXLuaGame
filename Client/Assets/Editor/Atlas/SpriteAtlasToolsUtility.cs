using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Jing;
using LitJson;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

public class SpriteAtlasToolsUtility
{

    public static event Action onAddSpriteAtlas;

    public static event Action onBuildAll;

    public static string ConfigName => Constant.ATLAS_CONFIG_NAME;

    public static string GenerateSpriteAtlasNameByPath(string path)
    {
        if (!Directory.Exists(path))
        {
            return string.Empty;
        }
        var name = FileUtil.Normalized(path).Remove(0, "Assets/".Length).Replace("/", "_").ToLower();
        return name + ".spriteatlas";
    }

    public static void AddSpriteAtlas(string textureDirPath, bool isSubDirSplit)
    {
        var cfg = Util.LoadConfig<SpriteAtlasToolsConfig>(Constant.ATLAS_CONFIG_NAME);
        foreach (var v in cfg.itemList)
        {
            if(v.textureDirPath == textureDirPath)
            {
                Debug.Log("AddSpriteAtlas Failed ：Texture Path is Exist!");
                return;
            }
        }
        var item = new SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem();
        item.isSubDirSplit = isSubDirSplit;
        item.textureDirPath = textureDirPath;
        cfg.itemList.Add(item);
        Util.SaveConfig(cfg, Constant.ATLAS_CONFIG_NAME);
        Debug.Log("AddSpriteAtlas Success!");
        onAddSpriteAtlas?.Invoke();
    }

    public static void BuildAll()
    {
        var cfg = Util.LoadConfig<SpriteAtlasToolsConfig>(Constant.ATLAS_CONFIG_NAME);
        for (int i = 0; i < cfg.itemList.Count; i++)
        {
            var item = cfg.itemList[i];
            EditorUtility.DisplayProgressBar("Progress", "spriteatlas building ...", 0);
            BuildSpriteAtlas(cfg.spriteAtlasSaveDirPath, item, cfg.packingTextureWidthLimit, cfg.packingTextureHeightLimit);
        }
        EditorUtility.ClearProgressBar();
        onBuildAll?.Invoke();
    }

    public static void BuildSpriteAtlas(string spriteAtlasDirPath, SpriteAtlasToolsConfig.SpriteAtlasToolsConfigItem item, int limitWidth, int limitHeight)
    {
        CreateOrUpdateSpriteAtlas(spriteAtlasDirPath, item.textureDirPath, item.isSubDirSplit, limitWidth, limitHeight);
    }

    public static void CreateOrUpdateSpriteAtlas(string spriteAtlasDirPath, string textureDirPath, bool isSubDirSplit, int limitWidth, int limitHeight)
    {
        var name = GenerateSpriteAtlasNameByPath(textureDirPath);
        var filePath = FileUtil.CombinePaths(spriteAtlasDirPath, name);
        string[] files;
        if (isSubDirSplit)
        {
            files = Directory.GetFiles(textureDirPath);
            var dirs = Directory.GetDirectories(textureDirPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                CreateOrUpdateSpriteAtlas(spriteAtlasDirPath, dirs[i], isSubDirSplit, limitWidth, limitHeight);
            }
        }
        else
        {
            files = Directory.GetFiles(textureDirPath, "*", SearchOption.AllDirectories);
        }

        var spriteList = new List<Sprite>();
        for (int i = 0; i < files.Length; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(files[i]);
            if(null == sprite || sprite.texture.width >= limitWidth || sprite.texture.height >= limitHeight)
            {
                continue;
            }
            spriteList.Add(sprite);
        }
        var isNeedCreateAtlas = spriteList.Count > 1;
        var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(filePath);
        if (spriteAtlas == null)
        {
            if (!isNeedCreateAtlas)
            {
                return;
            }
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var packingSettings = new SpriteAtlasPackingSettings();
            packingSettings.enableRotation = false;
            packingSettings.enableTightPacking = false;
            packingSettings.padding = 2;

            spriteAtlas = new SpriteAtlas();
            spriteAtlas.SetPackingSettings(packingSettings);
            AssetDatabase.CreateAsset(spriteAtlas, filePath);
        }
        var oldSpriteList = spriteAtlas.GetPackables();
        spriteAtlas.Remove(oldSpriteList);
        spriteAtlas.Add(spriteList.ToArray());
        AssetDatabase.SaveAssets();
    }

    public static void PackPreview(string spriteAtlasDirPath)
    {
        var files = Directory.GetFiles(spriteAtlasDirPath, "*.spriteatlas");
        var spriteAtlases = new SpriteAtlas[files.Length];
        var selections = new UnityEngine.Object[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(files[i]);
            selections[i] = spriteAtlas;
            spriteAtlases[i] = spriteAtlas;
        }
        try
        {
            Selection.objects = selections;
            SpriteAtlasUtility.PackAtlases(spriteAtlases, EditorUserBuildSettings.activeBuildTarget);
        }
        catch(Exception ex)
        {
            Debug.LogError("PackPreview Error:" + ex.Message);
        }
    }

}
