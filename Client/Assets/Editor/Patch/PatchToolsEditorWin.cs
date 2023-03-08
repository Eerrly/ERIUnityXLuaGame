using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
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
        startVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_START_VERSION", "");
        endVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_END_VERSION", "");
    }

    [HideLabel, ReadOnly, LabelWidth(150)]
    [LabelText("��ʼ�汾��StartVersion��")]
    public string startVersion = "";

    [HideLabel, HorizontalGroup("EndVersion"), LabelWidth(150)]
    [LabelText("�����汾��EndVersion��")]
    public string endVersion = "";

    [HorizontalGroup("EndVersion")]
    [Button("Head")]
    public void GetGitHeadVersion()
    {
        endVersion = PatchUtil.GetGitVersion();
    }

    [HideLabel, ReadOnly]
    [Title("�ȸ��ļ��б�")]
    [LabelText("�б�")]
    public string[] patchFiles = new string[] { };

    [Button("��ȡ�ȸ��ļ��б�", ButtonSizes.Large)]
    public void GetPatchFiles()
    {
        patchFiles = PatchUtil.GetPatchFiles(startVersion, endVersion);
        if (patchFiles.Length > 0)
        {
            List<string> fileList = patchFiles
            .Where((path) =>
            {
                return
                !path.EndsWith(".meta") ||
                path.EndsWith(".meta") && !patchFiles.Contains(path.Replace(".meta", "")) && System.IO.File.Exists(path.Replace(".meta", ""));
            })
            .Select((path) => {
                if (path.StartsWith("Client/Assets/Sources/LuaScripts/Lua"))
                {
                    path.Replace(".bytes", ".lua");
                }
                return path.Replace("Client/Assets/Sources/", "").ToLower();
            }).ToList();
            patchFiles = fileList.ToArray();
        }
        if (patchFiles != null)
        {
            this.ShowTip("��ȡ�ɹ���");
        }
    }

    [Button("��ʼ�ȸ�", ButtonSizes.Large)]
    public void PatchFiles()
    {
        HashSet<string> patchList = new HashSet<string>(patchFiles);
        ResUtil.Patch(patchList);

        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_START_VERSION", startVersion);
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_END_VERSION", endVersion);

        this.ShowTip("�����ɹ���");
    }

}