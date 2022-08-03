using System.Collections.Generic;

[EntitySystem]
public class PlayerStateSystem
{
    private static int minStateId;
    private static int maxStateId;

    [EntitySystem.Initialize]
    public static void Initialize()
    {
        minStateId = (int)EPlayerState.None;
        maxStateId = (int)EPlayerState.Count;
    }

    public static bool HasAttackTarget(PlayerEntity playerEntity)
    {
        return playerEntity.attack.targetId != -1;
    }

    public static void ChangePlayerState(PlayerEntity playerEntity, EPlayerState state)
    {
        ChangePlayerState(playerEntity, (int)state);
    }

    public static void ChangePlayerState(PlayerEntity playerEntity, int stateId)
    {
        if (stateId > minStateId && stateId < maxStateId)
        {
            playerEntity.state.nextStateId = stateId;
        }
    }

}
