using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 缓存路径工具
/// </summary>
public class OtherToolsEditorWin : OdinEditorWindow
{
    public static OtherToolsEditorWin Open()
    {
        var win = GetWindow<OtherToolsEditorWin>("Other Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        processId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
    }

    [InfoBox("调试时如果开启了多个Untiy编辑器，可以通过进程ID来判断要连接的编辑器是哪一个")]
    [LabelText("进程ID")]
    public string processId;

    [Button("清除本地缓存")]
    [HorizontalGroup("One")]
    public void CleanPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        EditorUtility.DisplayDialog("提示", "已完成!", "确认");
    }

    [Button("打开 [CacheBundleRoot] 目录")]
    [HorizontalGroup("Two")]
    public void OpenPersistentBundleRootFolder()
    {
        Application.OpenURL(Setting.CacheBundleRoot);
    }

    [Button("打开 [PersistentDataPath] 目录")]
    [HorizontalGroup("Two")]
    public void OpenPersistentDataFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    [Button("打开 [BundleCache] 目录")]
    [HorizontalGroup("Two")]
    public static void OpenBundleCacheFolder()
    {
        Application.OpenURL("BundleCache");
    }

    [Button("打开 [Patch] 目录")]
    [HorizontalGroup("Two")]
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

    [Button("删除 [CacheBundleRoot] 目录下的全部文件")]
    public void CleanPersistentBundleRootPath()
    {
        if (EditorUtility.DisplayDialog("提示", "确认删除 [CacheBundleRoot] 目录下的全部文件吗？", "确认", "取消"))
        {
            string[] filePaths = Directory.GetFiles(Setting.CacheBundleRoot);
            for (int i = 0; i < filePaths.Length; i++)
            {
                FileUtil.DeleteFile(filePaths[i]);
            }
        }
    }
}
