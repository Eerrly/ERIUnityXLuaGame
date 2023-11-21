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
                var playerPos = entity.Transform.pos;
                var otherPos = other.Transform.pos;
                var distance = (entity.Transform.pos - other.Transform.pos).Magnitude;
                if (distance <= PlayerPropertyConstant.CollisionRadius * PlayerPropertyConstant.CollisionRadius)
                {
                    var angle = FixedVector3.AngleInt(entity.Transform.fwd, otherPos - playerPos);
                    if(angle <= BattleConstant.angle / 2)
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
                entities.Add(cellEntities[j]);
            }
        }

        var results = entities.GetRange(0, Mathf.Min(entities.Count, maxCount)).Select((e) => { return e.ID; }).ToArray();
        return results;
    }

}
