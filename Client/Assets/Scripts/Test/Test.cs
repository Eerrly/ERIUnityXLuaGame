using UnityEngine;

public class Test : MonoBehaviour
{
    FirstAgent fa;

    private void Awake()
    {
        fa = new FirstAgent();
    }

    private void Start()
    {
        behaviac.Workspace.Instance.FilePath = Application.dataPath + @"\Scripts\AI\exported";
        behaviac.Workspace.Instance.FileFormat = behaviac.Workspace.EFileFormat.EFF_xml;
        fa.btload("FirstTree");
        fa.btsetcurrent("FirstTree");
    }

    private void Update()
    {
        fa.btexec();
    }

}