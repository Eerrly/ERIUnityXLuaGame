using System;
using System.Collections.Generic;
using System.Linq;

public class BattleEntity : BaseEntity
{
    public int Frame;

    public float DeltaTime;

    public float Time;

    public float TimeScale;

    public readonly List<BaseEntity> Entities = new List<BaseEntity>();

    internal void Init()
    {
        Frame = -1;
        DeltaTime = 0.0f;
        Time = 0.0f;
        TimeScale = 1.0f;
        State.curStateId = (int)EBattleState.None;
        State.nextStateId = (int)EBattleState.RoundPlaying;
    }

    public BaseEntity FindEntity(int entityId)
    {
        return Entities.FirstOrDefault(t => t.ID == entityId);
    }

}
