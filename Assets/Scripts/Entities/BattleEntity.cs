using System;
using System.Collections.Generic;

public class BattleEntity : BaseEntity
{

    public float deltaTime = 0.0f;

    public float time = 0.0f;

    public float timeScale = 1.0f;

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
