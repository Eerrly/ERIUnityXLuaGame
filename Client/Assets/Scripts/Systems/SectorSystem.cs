using UnityEngine;
using System.Collections.Generic;

public class SectorSystem
{

    public static List<BaseEntity> GetWithinRangeOfTheAttack(BaseEntity playerEntity)
    {
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
        List<BaseEntity> entities = new List<BaseEntity>();

        for (int i = 0; i < aroundCellList.Count; i++)
        {
            var cellEntities = aroundCellList[i].entities;
            for (int j = 0; j < cellEntities.Count; j++)
            {
                var other = cellEntities[j];
                if (other.property.camp == playerEntity.property.camp)
                    continue;
                var playerPos = MathManager.ToVector3(playerEntity.transform.pos);
                var otherPos = MathManager.ToVector3(other.transform.pos);
                var distance = (otherPos - playerPos).sqrMagnitude;
                if (distance <= PlayerPropertyConstant.AttackDistance + EnemyPropertyConstant.CollisionRadius)
                {
                    var playerRot = MathManager.ToQuaternion(playerEntity.transform.rot);
                    var forward = playerRot * Vector3.forward;
                    var angle = Vector3.Angle(forward, otherPos - playerPos);
                    if(angle <= BattleConstant.angle * 0.5)
                    {
                        entities.Add(other);
                    }
                }
            }
        }

        return entities;
    }

}
