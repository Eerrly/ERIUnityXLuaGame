[PlayerState(EPlayerState.Idle)]
public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
    }

    public override void OnLateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        if (!KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.Move);
        }
        if (KeySystem.CheckKeyCodeJDown(playerEntity))
        {
            EntityStateSystem.ChangeEntityState(playerEntity, EPlayerState.AttackReady);
        }
    }

    public override void OnCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
#if UNITY_DEBUG
        Logger.Log(LogLevel.Info, "[PlayerIdleState OnCollision] source:" + source.ID + ", target:" + target.ID);
#endif
    }

    public override void OnPostCollision(BaseEntity source, BaseEntity target, BattleEntity battleEntity)
    {
        PhysicsSystem.CheckCollisionDir(source, target);
#if UNITY_DEBUG
        Logger.Log(LogLevel.Info, "[PlayerIdleState OnPostCollision] source:" + source.ID + ", target:" + target.ID + ", collisionDir:" + System.Enum.GetName(typeof(ECollisionDir), source.collision.collisionDir));
#endif
    }

}
