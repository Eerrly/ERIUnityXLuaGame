using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SectorSystem
{

    public static List<BaseEntity> GetWithinRangeOfTheAttack(BaseEntity entity)
    {
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(entity);
        List<BaseEntity> entities = new List<BaseEntity>();

        for (int i = 0; i < aroundCellList.Count; i++)
        {
            var cellEntities = aroundCellList[i].entities;
            for (int j = 0; j < cellEntities.Count; j++)
            {
                var other = cellEntities[j];
                if (other.property.camp == entity.property.camp)
                    continue;
                var playerPos = MathManager.ToVector3(entity.transform.pos);
                var otherPos = MathManager.ToVector3(other.transform.pos);
                var distance = (otherPos - playerPos).sqrMagnitude;
                if (distance <= PlayerPropertyConstant.AttackDistance + EnemyPropertyConstant.CollisionRadius)
                {
                    var playerRot = MathManager.ToQuaternion(entity.transform.rot);
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

    public static int[] GetAroundEntities(BaseEntity entity, int maxCount)
    {
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(entity);
        List<BaseEntity> entities = new List<BaseEntity>();

        for (int i = 0; i < aroundCellList.Count; i++)
        {
            var cellEntities = aroundCellList[i].entities;
            for (int j = 0; j < cellEntities.Count; j++)
            {
                var other = cellEntities[j];
                if (other.property.camp == entity.property.camp)
                    continue;
                entities.Add(other);
            }
        }

        var results = entities.GetRange(0, Mathf.Min(entities.Count, maxCount)).Select((e) => { return e.ID; }).ToArray();
        return results;
    }

}
