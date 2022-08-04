[EntitySystem]
public class AttackSystem
{
    private static float attackCdTimeFrame = 0.0f;
 
    [EntitySystem.Initialize]
    public static void Initialize()
    {
        attackCdTimeFrame = PlayerPropertyConstant.AttackCdTime * BattleConstant.FrameInterval;
    }

    public static bool CheckAttackDistance(PlayerEntity playerEntity, PlayerEntity otherEntity, BattleEntity battleEntity)
    {
        var target = MathManager.ToVector3(otherEntity.transform.pos);
        var source = MathManager.ToVector3(playerEntity.transform.pos);
        var distance = (target - source).magnitude;
#if UNITY_DEBUG
        UnityEngine.Debug.Log("[AttackSystem CheckAttackDistance] distance:" + distance + ", result:" + (distance <= PlayerPropertyConstant.AttackDistance));
#endif
        return distance <= PlayerPropertyConstant.AttackDistance;
    }

    public static bool CheckAttackCdTime(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (playerEntity.attack.lastAttackTime < 0)
            return true;
        return battleEntity.time - playerEntity.attack.lastAttackTime >= attackCdTimeFrame;
    }

}
