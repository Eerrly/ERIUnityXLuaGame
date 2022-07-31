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

    public static void ChangePlayerState(PlayerEntity playerEntity, EPlayerState state)
    {
        int stateId = (int)state;
        if(stateId > minStateId && stateId < maxStateId)
        {
            playerEntity.state.nextStateId = stateId;
        }
    }

}
