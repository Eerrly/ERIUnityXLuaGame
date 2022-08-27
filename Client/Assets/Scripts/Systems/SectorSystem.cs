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
                var distance = (otherPos - playerPos).magnitude;
                if (distance <= Mathf.Pow(PlayerPropertyConstant.AttackDistance + EnemyPropertyConstant.CollisionRadius, 2))
                {
                    var forward = MathManager.ToVector3(entity.transform.fwd);
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

    public static int[] GetAroundEntities(BaseEntity entity, int maxCount, bool isActive = false)
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
                if (isActive)
                {
                    if(other.property.hp > 0)
                    {
                        entities.Add(other);
                    }
                }
                else
                {
                    entities.Add(other);
                }
            }
        }

        var results = entities.GetRange(0, Mathf.Min(entities.Count, maxCount)).Select((e) => { return e.ID; }).ToArray();
        return results;
    }

}
