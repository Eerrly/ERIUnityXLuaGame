using UnityEngine;

public struct PhysisPlayer
{
    public int id;

    public override bool Equals(object obj)
    {
        PhysisPlayer b = (PhysisPlayer)obj;
        return id == b.id;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(PhysisPlayer a, PhysisPlayer b)
    {
        return a.id == b.id;
    }

    public static bool operator !=(PhysisPlayer a, PhysisPlayer b)
    {
        return a.id != b.id;
    }
}

[EntitySystem]
public class PhysicsSystem
{

    public static void Update(BattleEntity battleEntity)
    {
        var entities = battleEntity.entities;
        for (int i = 0; i < entities.Count; i++)
        {
            var source = entities[i];
            source.runtimeProperty.closedPlayers.Clear();
            if (source.state.curStateId == (int)EPlayerState.Dead || source.state.curStateId == (int)EEnemyState.Dead)
            {
                continue;
            }
            for (int j = 0; j < entities.Count; j++)
            {
                var target = entities[j];
                if(source.ID == target.ID || target.state.curStateId == (int)EEnemyState.Dead || source.state.curStateId == (int)EPlayerState.Dead)
                {
                    continue;
                }
                var radius = source.GetCollisionRadius(battleEntity) + target.GetCollisionRadius(battleEntity);
                var sqrMagnitudeXZ = (target.transform.pos - source.transform.pos).sqrMagnitudeLongXZ;
                if (sqrMagnitudeXZ <= radius * radius)
                {
                    source.runtimeProperty.closedPlayers.Add(new PhysisPlayer() { id = target.ID });
                }
            }
        }

        for (int i = 0; i < entities.Count; i++)
        {
            var source = entities[i];
            var closedPlayers = source.runtimeProperty.closedPlayers;
            for (int j = 0; j < closedPlayers.Count; j++)
            {
                var target = battleEntity.FindEntity(closedPlayers[j].id);
                UpdateCollision(source, target, battleEntity);
            }
        }
    }

    private static void UpdateCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
        var sMove = source.movement.position;
        var tMove = target.movement.position;

        var vecS2T = (source.transform.pos - target.transform.pos).YZero();
        var vecT2S = (target.transform.pos - source.transform.pos).YZero();

        if (FixedVector3.Dot(ref vecS2T, ref tMove) > FixedNumber.Zero)
        {
            sMove = CombineForce(tMove, sMove);
        }
        else
        {
            vecS2T = -vecS2T;
            if (FixedVector3.Dot(sMove, vecS2T) > FixedNumber.Zero)
            {
                sMove -= FixedVector3.Project(sMove, vecS2T);
            }
        }
        source.movement.position = sMove;

        var state = PlayerStateMachine.Instance.GetState(source.state.curStateId) as PlayerBaseState;
        if (state != null)
        {
            if (FixedVector3.Dot(vecT2S, sMove) > FixedNumber.Zero)
            {
                state.OnCollision(source, target, battleEntity);
            }
            state.OnPostCollision(source, target, battleEntity);
        }
    }

    private static FixedVector3 CombineForce(FixedVector3 aDeltaMove, FixedVector3 bDeltaMove)
    {
        var lenB = bDeltaMove.Magnitude;
        var lenA = aDeltaMove.Magnitude;

        var normalB = bDeltaMove.Normalized;
        var normalA = aDeltaMove.Normalized;

        var dot = FixedVector3.Dot(normalB, normalA);
        var lenC = dot * lenA;
        var cDeltaMove = normalA * lenC;

        lenC = FixedMath.Max(lenB, lenC);
        return normalB * lenC + (bDeltaMove - cDeltaMove);
    }

    /// <summary>
    /// -45  45
    ///    \/
    ///    /\
    /// -135 135
    /// </summary>
    public static void CheckCollisionDir(BaseEntity source, BaseEntity target)
    {
        source.collision.collisionDir = 0;
        var direction = target.transform.pos - source.transform.pos;
        var angle = FixedVector3.AngleIntSingle(source.transform.fwd, direction.YZero().Normalized);
        if(-45 < angle && angle < 45)
        {
            source.collision.collisionDir = (int)ECollisionDir.Forward;
        }
        else if(-135 < angle && angle < -45)
        {
            source.collision.collisionDir = (int)ECollisionDir.Left;
        }
        else if(135 > angle && angle > 45)
        {
            source.collision.collisionDir = (int)ECollisionDir.Right;
        }
        else if(135 < angle || angle < -135)
        {
            source.collision.collisionDir = (int)ECollisionDir.Back;
        }
    }

}
