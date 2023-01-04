[EntitySystem]
public class KeySystem
{
    public static void Clear(BattleEntity battleEntity)
    {
        var players = battleEntity.entities;
        for (int i = 0; i < players.Count; i++)
        {
            var player = (PlayerEntity)players[i];
            player.input.pos = player.ID;
            player.input.yaw = MathManager.YawStop;
            player.input.key = 0;
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

    public static bool CheckKeyCodeJDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.input.key & 0x2) > 0;
        return result;
    }

    public static bool CheckKeyCodeKDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.input.key & 0x4) > 0;
        return result;
    }

    public static bool CheckKeyCodeLDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.input.key & 0x8) > 0;
        return result;
    }

}
