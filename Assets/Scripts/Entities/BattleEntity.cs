using System;
using System.Collections.Generic;

/// <summary>
/// 战斗实体
/// </summary>
public class BattleEntity : BaseEntity
{

    public float deltaTime;

    public float time;

    public float timeScale;

    public List<PlayerEntity> playerList = new List<PlayerEntity>();

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
