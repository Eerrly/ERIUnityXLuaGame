[EntitySystem]
public class AttackSystem
{
    private static float playerAttackCdTimeFrame = 0.0f;
    private static float enemyAttackCdTimeFrame = 0.0f;
 
    [EntitySystem.Initialize]
    public static void Initialize()
    {
        playerAttackCdTimeFrame = PlayerPropertyConstant.AttackCdTime * BattleConstant.FrameInterval;
        enemyAttackCdTimeFrame = EnemyPropertyConstant.AttackCdTime * BattleConstant.FrameInterval;
    }

    public static bool CheckAttackDistance(BaseEntity srouceEntity, BaseEntity targetEntity, BattleEntity battleEntity)
    {
        var target = MathManager.ToVector3(targetEntity.transform.pos);
        var source = MathManager.ToVector3(srouceEntity.transform.pos);
        var distance = (target - source).magnitude;
#if UNITY_DEBUG
        UnityEngine.Debug.Log("[AttackSystem CheckAttackDistance] distance:" + distance + ", result:" + (distance <= PlayerPropertyConstant.AttackDistance));
#endif
        return distance <= PlayerPropertyConstant.AttackDistance;
    }

    public static bool CheckAttackCdTime(BaseEntity entity, BattleEntity battleEntity)
    {
        if (entity.attack.lastAttackTime <= 0)
            return true;
        var cdTimeFrame = entity.property.camp == ECamp.Alliance ? playerAttackCdTimeFrame : enemyAttackCdTimeFrame;
        return battleEntity.time - entity.attack.lastAttackTime >= cdTimeFrame;
    }

    public static bool Attack(BaseEntity sourceEntity, BaseEntity targetEntity)
    {
        var result = true;
        var residualHp = targetEntity.property.hp - sourceEntity.attack.atk;
        if(residualHp < 0)
        {
            residualHp = 0;
        }
        targetEntity.property.hp = residualHp;
        return result;
    }

}
