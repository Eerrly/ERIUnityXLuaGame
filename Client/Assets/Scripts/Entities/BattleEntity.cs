using System;
using System.Collections.Generic;

public class BattleEntity : BaseEntity
{

    public float deltaTime;

    public float time;

    public float timeScale;

    public List<BaseEntity> entities = new List<BaseEntity>();

    public PlayerEntity selfPlayerEntity
    {
        get { return (PlayerEntity)entities[BattleManager.Instance.selfPlayerId]; }
    }

    internal void Init()
    {
        deltaTime = 0.0f;
        time = 0.0f;
        timeScale = 1.0f;
        state.curStateId = (int)EBattleState.None;
        state.nextStateId = (int)EBattleState.RoundPlaying;
    }

    public BaseEntity FindEntity(int entityId)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].ID == entityId)
                return entities[i];
        }
        return null;
    }

}
