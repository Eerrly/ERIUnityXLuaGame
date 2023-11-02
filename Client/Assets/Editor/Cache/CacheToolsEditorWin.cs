using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CacheToolsEditorWin : OdinEditorWindow
{
    public static CacheToolsEditorWin Open()
    {
        var win = GetWindow<CacheToolsEditorWin>("Cache Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    [Button("清除本地缓存")]
    public void CleanPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        EditorUtility.DisplayDialog("提示", "已完成!", "确认");
    }

    [Button("打开 [PersistentDataPath] 目录")]
    public void OpenPersistentDataFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    [Button("打开 [BundleCache] 目录")]
    public static void OpenBundleCacheFolder()
    {
        Application.OpenURL("BundleCache");
    }

    [Button("打开 [Patch] 目录")]
    public static void OpenPatchFolder()
    {
        Application.OpenURL("Patch");
    }

    [Button("删除 [persistentDataPath] 目录")]
    public void CleanPersistentDataPath()
    {
        if (EditorUtility.DisplayDialog("提示", "确认删除 [persistentDataPath] 目录吗？", "确认", "取消"))
        {
            if (Directory.Exists(Application.persistentDataPath))
            {
                Directory.Delete(Application.persistentDataPath, true);
            }
        }
    }
}
