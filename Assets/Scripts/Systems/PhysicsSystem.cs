using System.Collections.Generic;
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

    public static bool IsCanAttack(PlayerEntity playerEntity)
    {
        return true;
    }

    public static void Update(BattleEntity battleEntity)
    {
        var playerList = battleEntity.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
            var source = playerList[i];
            if(source.state.curStateId != (int)EPlayerState.Move)
            {
                continue;
            }
            source.runtimeProperty.closedPlayers.Clear();
            for (int j = 0; j < playerList.Count; j++)
            {
                var target = playerList[j];
                if(source.ID == target.ID || target.state.curStateId == (int)EPlayerState.None)
                {
                    continue;
                }
                var radius = source.GetCollisionRadius(battleEntity) + target.GetCollisionRadius(battleEntity);
                var sqrMagnitude = (MathManager.ToVector3(target.transform.pos) - MathManager.ToVector3(source.transform.pos)).sqrMagnitude;
                if (sqrMagnitude < radius * radius)
                {
                    source.runtimeProperty.closedPlayers.Add(new PhysisPlayer() { id = target.ID });
                }
            }
        }

        for (int i = 0; i < playerList.Count; i++)
        {
            var source = playerList[i];
            var closedPlayers = source.runtimeProperty.closedPlayers;
            for (int j = 0; j < closedPlayers.Count; j++)
            {
                var target = battleEntity.FindPlayer(closedPlayers[j].id);
                UpdateCollision(source, target, battleEntity);
            }
        }
    }

    private static void UpdateCollision(PlayerEntity source, PlayerEntity target, BattleEntity battleEntity)
    {
        var sMove = MathManager.ToVector3(source.movement.position);
        var tMove = MathManager.ToVector3(target.movement.position);

        var vecS2T = MathManager.ToVector3(source.transform.pos) - MathManager.ToVector3(target.transform.pos);
        var vecT2S = MathManager.ToVector3(target.transform.pos) - MathManager.ToVector3(source.transform.pos);

        if(Vector3.Dot(vecS2T, tMove) > 0.0f)
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

        if (Vector3.Dot(vecT2S, sMove) > 0.0f)
        {
            (PlayerStateMachine.Instance.GetState((int)source.curStateId) as PlayerBaseState).OnCollision(source, target, battleEntity);
            (PlayerStateMachine.Instance.GetState((int)target.curStateId) as PlayerBaseState).OnCollision(target, source, battleEntity);
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
        return normalB * lenC + (cDeltaMove - aDeltaMove);
    }

}
