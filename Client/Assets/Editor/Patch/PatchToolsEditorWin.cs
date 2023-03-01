using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;

public class PatchToolsEditorWin : OdinEditorWindow
{

    public static PatchToolsEditorWin Open()
    {
        var win = GetWindow<PatchToolsEditorWin>("Patch Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        startVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_START_VERSION", "202e42c3");
        endVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_END_VERSION", "ce5db4c7");
    }

    private void OnGitVersionValueChanged()
    {
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_START_VERSION", startVersion);
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_END_VERSION", endVersion);
    }

    [HideLabel, LabelWidth(150)]
    [LabelText("起始版本（StartVersion）")]
    [OnValueChanged("OnGitVersionValueChanged")]
    public string startVersion = "";

    [HideLabel, LabelWidth(150)]
    [LabelText("结束版本（EndVersion）")]
    [OnValueChanged("OnGitVersionValueChanged")]
    public string endVersion = "";

    [HideLabel, ReadOnly]
    [Title("热更文件列表")]
    [LabelText("列表")]
    public string[] patchFiles;

    [Button("获取热更文件列表", ButtonSizes.Large)]
    public void GetPatchFiles()
    {
        var dir = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Sources");
        var bat = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/calclist.bat");
        var output = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/diff.txt");
        var arg = string.Format("{0} {1} {2}", startVersion, endVersion, output);
        if(Util.ExecuteBat(dir, bat, arg) == 0)
        {
            System.Array.Clear(patchFiles, 0, patchFiles.Length);
            patchFiles = File.ReadAllLines(output).Where(path=>path.StartsWith("Client/Assets/Sources/")).ToArray();
            this.ShowTip("获取成功！");
        }
        else
        {
            this.ShowTip("获取失败！");
        }
    }

    [Button("开始热更", ButtonSizes.Large)]
    public void PatchFiles()
    {
        this.ShowTip("TODO");
    }

}
