using System.Linq;

[EntitySystem]
public class KeySystem
{
    /// <summary>
    /// 清理输入
    /// </summary>
    /// <param name="battleEntity"></param>
    public static void Clear(BattleEntity battleEntity)
    {
        var players = battleEntity.Entities;
        foreach (var player in players.Cast<PlayerEntity>())
        {
            player.Input.pos = player.ID;
            player.Input.yaw = FixedMath.YawStop;
            player.Input.key = 0;
        }
    }

    /// <summary>
    /// 是否没有摇杆
    /// </summary>
    /// <param name="yaw">摇杆</param>
    /// <returns></returns>
    public static bool IsYawTypeStop(int yaw)
    {
        return yaw == FixedMath.YawStop;
    }

    public static int GetLogicKeyDown(PlayerEntity playerEntity)
    {
        var input = playerEntity.Input;
        var key = (int)ELogicInputKey.None;
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
        var result = (playerEntity.Input.key & 0x1) > 0;
        return result;
    }

    public static bool CheckKeyCodeKDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.Input.key & 0x2) > 0;
        return result;
    }

    public static bool CheckKeyCodeLDown(PlayerEntity playerEntity)
    {
        var result = (playerEntity.Input.key & 0x4) > 0;
        return result;
    }

}
