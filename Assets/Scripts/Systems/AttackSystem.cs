[EntitySystem]
public class AttackSystem
{
    private static float attackCdTimeFrame = 0.0f;
 
    [EntitySystem.Initialize]
    public static void Initialize()
    {
        attackCdTimeFrame = BattleConstant.attackCdTime * BattleConstant.FrameInterval;
    }

    public static bool HasAttackTarget(PlayerEntity playerEntity)
    {
        return playerEntity.attack.targetId != -1;
    }

    public static bool CheckAttackDistance(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        var target = MathManager.ToVector3(battleEntity.FindPlayer(playerEntity.attack.targetId).transform.pos);
        var source = MathManager.ToVector3(playerEntity.transform.pos);
        var distance = (target - source).magnitude;
#if UNITY_DEBUG
        UnityEngine.Debug.Log("CheckAttackDistance distance:" + distance + ", result:" + (distance <= BattleConstant.attackDistance));
#endif
        return distance <= BattleConstant.attackDistance;
    }

    public static bool CheckAttackCdTime(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (playerEntity.attack.lastAttackTime < 0)
            return true;
        return battleEntity.time - playerEntity.attack.lastAttackTime >= attackCdTimeFrame;
    }
    
    public static void SelectAttackTarget(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        var playerList = battleEntity.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
            if(playerList[i].property.camp != playerEntity.property.camp && playerEntity.attack.targetId != playerList[i].ID)
            {
                playerEntity.attack.targetId = playerList[i].ID;
                break;
            }
        }
    }

}
