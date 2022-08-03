using System;
using System.Collections.Generic;

public class BattleEntity : BaseEntity
{

    public float deltaTime;

    public float time;

    public float timeScale;

    public List<PlayerEntity> playerList = new List<PlayerEntity>();

    #region Components
    public StateComponent state = new StateComponent();
    #endregion

    public EBattleState curStateId
    {
        get { return (EBattleState)state.curStateId; }
    }

    public EBattleState nextStateId
    {
        get { return (EBattleState)state.nextStateId; }
    }

    public EBattleState preStateId
    {
        get { return (EBattleState)state.preStateId; }
    }

    public PlayerEntity selfPlayerEntity
    {
        get { return playerList[BattleManager.Instance.selfPlayerId]; }
    }

    internal void Init()
    {
        deltaTime = 0.0f;
        time = 0.0f;
        timeScale = 1.0f;
        state.curStateId = (int)EBattleState.None;
        state.nextStateId = (int)EBattleState.RoundPlaying;
    }

    public PlayerEntity FindPlayer(int playerId)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].ID == playerId)
                return playerList[i];
        }
        return null;
    }
}
