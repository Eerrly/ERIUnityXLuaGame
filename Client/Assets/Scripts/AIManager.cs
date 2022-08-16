using UnityEngine;
using behaviac;
using System.Collections.Generic;

public class AIManager
{

    private static AIManager _instance;

    public static AIManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new AIManager();
                InitBehaivac();
            }
            return _instance;
        }
    }

    private Dictionary<string, LinkedList<Agent>> loadedAgentQueueDic = new Dictionary<string, LinkedList<Agent>>();

    public int LoadedAgentQueueDicCount => loadedAgentQueueDic.Count;

    private static void InitBehaivac()
    {
        Workspace.Instance.FilePath = Application.dataPath + @"\Scripts\AI\exported";
        Workspace.Instance.FileFormat = Workspace.EFileFormat.EFF_xml;
    }

    public Agent SetTree<T>(string relativePath) where T : Agent
    {
        Agent agent = GetAgent<T>(relativePath);
        agent.btload(relativePath);
        agent.btsetcurrent(relativePath);
        return agent;
    }

    public EBTStatus Exec<T>(Agent agent) where T : Agent
    {
        EBTStatus status = agent.btexec();
        return status;
    }

    public void Unload(Agent agent, string relativePath)
    {
        agent.btunload(relativePath);
    }

    private Agent GetAgent<T>(string treeName) where T : Agent
    {
        LinkedList<Agent> agentList;
        if(!loadedAgentQueueDic.TryGetValue(treeName, out agentList))
        {
            agentList = new LinkedList<Agent>();
            agentList.AddFirst((Agent)System.Activator.CreateInstance(typeof(T)));
            loadedAgentQueueDic.Add(treeName, agentList);
        }
        Agent _agent = null;
        foreach (var v in agentList)
        {
            if (!v.IsActive())
            {
                _agent = v;
                break;
            }
        }
        if(_agent == null)
        {
            _agent = (Agent)System.Activator.CreateInstance(typeof(T));
            loadedAgentQueueDic[treeName].AddLast(_agent);
        }
        return _agent;
    }

    public void Release()
    {
        foreach (var agentListData in loadedAgentQueueDic)
        {
            foreach (var agent in agentListData.Value)
            {
                agent.btunloadall();
            }
        }
        loadedAgentQueueDic.Clear();
    }


}
