﻿using System.IO;
using UnityEditor;

public class ToolbarEditorMenu
{
    [MenuItem("工具/SpriteAtlas Tools", false, 1000)]
    public static void SpriteAtlasTools()
    {
        SpriteAtlasToolsEditorWin.Open();
    }

    [MenuItem("工具/Build Tools", false, 1000)]
    public static void BuildTools()
    {
        BuildToolsEditorWin.Open();
    }

}

public class RightClickEditorMenu
{

    [MenuItem("Assets/工具/SpriteAtlas Tools/添加目录到SpriteAtlas配置", false, 1)]
    static void SpriteAtlasAdd()
    {
        if (Selection.objects.Length != 1)
        {
            EditorUtility.DisplayDialog("错误", "仅支持[单选]的[文件夹]", "OK");
            return;
        }

        var obj = Selection.objects[0];
        var assetPath = AssetDatabase.GetAssetPath(obj);
        if (false == Directory.Exists(assetPath))
        {
            EditorUtility.DisplayDialog("错误", "仅支持[单选]的[文件夹]", "OK");
            return;
        }

        SpriteAtlasToolsUtility.AddSpriteAtlas(assetPath, false);
    }

}