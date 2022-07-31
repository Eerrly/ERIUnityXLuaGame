[EntitySystem]
public class AttackSystem
{
    private static float attackCdTimeFrame = 0.0f;
 
    [EntitySystem.Initialize]
    public static void Initialize()
    {
        attackCdTimeFrame = BattleConstant.attackCdTime * BattleConstant.FrameInterval;
    }

    public static bool CheckAttackCdTime(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (playerEntity.attack.lastAttackTime < 0)
            return true;
        return battleEntity.time - playerEntity.attack.lastAttackTime >= attackCdTimeFrame;
    }
    
}
