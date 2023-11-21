using System;

[EntitySystem]
public class EntityStateSystem
{
    private static int minStateId;
    private static int maxStateId;

    [EntitySystem.Initialize]
    public static void Initialize()
    {
        minStateId = (int)EPlayerState.None;
        maxStateId = Math.Max((int)EPlayerState.Count, (int)EEnemyState.Count);
    }

    public static void ChangeEntityState(BaseEntity entity, EPlayerState state)
    {
        ChangeEntityState(entity, (int)state);
    }

    public static void ChangeEntityState(BaseEntity entity, EEnemyState state)
    {
        ChangeEntityState(entity, (int)state);
    }

    public static void ChangeEntityState(BaseEntity entity, int stateId)
    {
        if (stateId > minStateId && stateId < maxStateId)
        {
            entity.State.nextStateId = stateId;
        }
    }

}
