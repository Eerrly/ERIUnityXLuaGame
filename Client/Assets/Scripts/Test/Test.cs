using UnityEngine;

public class Test : MonoBehaviour
{
    private behaviac.Agent agent;

    private void Start()
    {
        agent = AIManager.Instance.SetTree<FirstAgent>("FirstTree");
    }

    private void Update()
    {
        AIManager.Instance.Exec<FirstAgent>(agent);
        Debug.Log("[Test Update] count:" + AIManager.Instance.LoadedAgentQueueDicCount);
    }

    private void OnDestroy()
    {
        AIManager.Instance.Release();
    }

}