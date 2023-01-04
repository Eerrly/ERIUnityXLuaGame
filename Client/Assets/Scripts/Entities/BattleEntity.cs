using System;
using System.Collections.Generic;

public class BattleEntity : BaseEntity
{
    public int frame;

    public float deltaTime;

    public float time;

    public float timeScale;

    public List<BaseEntity> entities = new List<BaseEntity>();

    internal void Init()
    {
        frame = -1;
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
