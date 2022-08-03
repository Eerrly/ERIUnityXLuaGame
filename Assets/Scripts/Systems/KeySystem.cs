[EntitySystem]
public class KeySystem
{
    public static void Clear(BattleEntity battleEntity)
    {
        var players = battleEntity.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].input.yaw = MathManager.YawStop;
            players[i].input.key = 0;
        }
    }

    public static bool IsYawTypeStop(int yaw)
    {
        return yaw == MathManager.YawStop;
    }

    public static int GetLogicKeyDown(PlayerEntity playerEntity)
    {
        var input = playerEntity.input;
        int key = (int)ELogicInputKey.None;
        while (key < (int)ELogicInputKey.KeyCount)
        {
            if((input.key & key) > 0)
            {
                return key;
            }
            key <<= 1;
        }
        return 0;
    }

    public static bool CheckAttackKeyDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.input.key & 0x2) > 0;
        return result;
    }

    public static bool CheckWarRoarKeyDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.input.key & 0x4) > 0;
        return result;
    }

    public static bool CheckTabKeyDown(PlayerEntity playerEntity)
    {
        return (playerEntity.input.key & 0x1) > 0;
    }

}
