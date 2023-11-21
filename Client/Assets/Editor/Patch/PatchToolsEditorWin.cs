using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System.Linq;

/// <summary>
/// 热更工具
/// </summary>
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
        startVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_START_VERSION", "");
        endVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_END_VERSION", "");
    }

    [HideLabel, ReadOnly, LabelWidth(150)]
    [LabelText("Git起始版本号")]
    public string startVersion = "";

    [HideLabel, HorizontalGroup("EndVersion"), LabelWidth(150)]
    [LabelText("Git结束版本号")]
    public string endVersion = "";

    [HorizontalGroup("EndVersion")]
    [Button("Head")]
    public void GetGitHeadVersion()
    {
        endVersion = PatchUtil.GetGitVersion();
    }

    [HideLabel, ReadOnly]
    [Title("热更文件列表")]
    [LabelText("列表")]
    public string[] patchFiles = new string[] { };

    [Button("获取热更文件列表", ButtonSizes.Large)]
    public void GetPatchFiles()
    {
        patchFiles = PatchUtil.GetPatchFiles(startVersion, endVersion);
        if (patchFiles.Length > 0)
        {
            var fileList = new HashSet<string>();
            foreach (var t in patchFiles)
            {
                if (t.StartsWith("Lua/"))
                {
                    // Lua/
                    fileList.Add(t.Substring(4, t.Length - 4).ToLower());
                }
                else
                {
                    var filePath = FileUtil.CombinePaths(UnityEngine.Application.dataPath, t.Replace("Client/Assets", ""));
                    if (System.IO.File.Exists(filePath) || (filePath.EndsWith(".meta") && System.IO.File.Exists(filePath.Replace(".meta", ""))))
                    {
                        // Client/Assets/Sources
                        fileList.Add(t.Substring(22, t.Length - 22).Replace(".meta", "").ToLower());
                    }
                }
            }
            patchFiles = fileList.ToArray();
        }
        if (patchFiles != null)
        {
            this.ShowTip("获取成功！");
        }
    }

    [Button("构建热更", ButtonSizes.Large)]
    public void PatchFiles()
    {
        var patchList = new HashSet<string>(patchFiles);
        ResUtil.Patch(patchList);

        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_START_VERSION", startVersion);
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_END_VERSION", endVersion);
        UnityEngine.PlayerPrefs.Save();
        this.ShowTip("构建成功！");
    }

}
