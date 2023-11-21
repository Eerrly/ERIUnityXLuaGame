using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SectorSystem
{

    public static List<BaseEntity> GetWithinRangeOfTheAttack(BaseEntity entity)
    {
        var aroundCellList = SpacePartition.GetAroundCellList(entity);
        var entities = new List<BaseEntity>();

        foreach (var cellEntities in aroundCellList.Select(t => t.entities))
        {
            foreach (var other in cellEntities)
            {
                var playerPos = entity.Transform.pos;
                var otherPos = other.Transform.pos;
                var distance = (entity.Transform.pos - other.Transform.pos).Magnitude;
                if (distance > PlayerPropertyConstant.CollisionRadius * PlayerPropertyConstant.CollisionRadius)
                    continue;
                
                var angle = FixedVector3.AngleInt(entity.Transform.fwd, otherPos - playerPos);
                if(angle <= BattleConstant.Angle / 2)
                {
                    entities.Add(other);
                }
            }
        }

        return entities;
    }

    public static int[] GetAroundEntities(BaseEntity entity, int maxCount, bool isActive = false)
    {
        var aroundCellList = SpacePartition.GetAroundCellList(entity);
        var entities = new List<BaseEntity>();

        foreach (var cellEntities in aroundCellList.Select(t => t.entities))
        {
            foreach (var t in cellEntities)
            {
                entities.Add(t);
            }
        }

        var results = entities.GetRange(0, Mathf.Min(entities.Count, maxCount)).Select((e) => e.ID).ToArray();
        return results;
    }

}
