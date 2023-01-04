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
                var sqrMagnitude = (MathManager.ToVector3(target.transform.pos) - MathManager.ToVector3(source.transform.pos)).sqrMagnitude;
                if (sqrMagnitude <= radius * radius)
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
        var sMove = MathManager.ToVector3(source.movement.position);
        var tMove = MathManager.ToVector3(target.movement.position);

        var vecS2T = MathManager.ToVector3(source.transform.pos) - MathManager.ToVector3(target.transform.pos);
        var vecT2S = MathManager.ToVector3(target.transform.pos) - MathManager.ToVector3(source.transform.pos);

        if (Vector3.Dot(vecT2S, tMove) > 0.0f)
        {
            sMove = CombineForce(tMove, sMove);
        }
        else
        {
            vecS2T = -vecS2T;
            if (Vector3.Dot(sMove, vecS2T) > 0.0f)
            {
                sMove -= Vector3.Project(sMove, vecS2T);
            }
        }
        source.movement.position = MathManager.ToFloat3(sMove);

        var state = PlayerStateMachine.Instance.GetState(source.state.curStateId) as PlayerBaseState;
        if (state != null)
        {
            if (Vector3.Dot(vecT2S, sMove) > 0.0f)
            {
                state.OnCollision(source, target, battleEntity);
            }
            state.OnPostCollision(source, target, battleEntity);
        }
    }

    private static Vector3 CombineForce(Vector3 aDeltaMove, Vector3 bDeltaMove)
    {
        var lenB = bDeltaMove.magnitude;
        var lenA = aDeltaMove.magnitude;

        var normalB = bDeltaMove.normalized;
        var normalA = aDeltaMove.normalized;

        var dot = Vector3.Dot(normalB, normalA);
        var lenC = dot * lenA;
        var cDeltaMove = normalA * lenC;

        lenC = Mathf.Max(lenB, lenC);
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
        var direction = MathManager.ToVector3(target.transform.pos) - MathManager.ToVector3(source.transform.pos);
        var angle = Vector3.SignedAngle(MathManager.ToVector3(source.transform.fwd), direction.normalized, Vector3.up);
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
