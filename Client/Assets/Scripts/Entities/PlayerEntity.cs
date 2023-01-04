public class PlayerEntity : BaseEntity
{

    internal override void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;

        input.pos = ID;
        input.yaw = MathManager.YawStop;
        input.key = 0;

        runtimeProperty.seed = BattleConstant.randomSeed;

        property.pos = data.pos;

        collision.collsionSize = PlayerPropertyConstant.CollisionRadius;

        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;
        state.count = (int)EPlayerState.Count;

        movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;

        InitBuffs();
    }

    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    public override float GetCollisionRadius(BattleEntity battleEntity)
    {
        return collision.collsionSize;
    }

}
