/// <summary>
/// 玩家实体
/// </summary>
public class PlayerEntity : BaseEntity
{

    /// <summary>
    /// 初始化玩家实体数据
    /// </summary>
    /// <param name="data">玩家实体数据</param>
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

    /// <summary>
    /// 初始化Buff列表
    /// </summary>
    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    /// <summary>
    /// 获取碰撞半径
    /// </summary>
    /// <param name="battleEntity">战斗实体</param>
    /// <returns>半径</returns>
    public override float GetCollisionRadius(BattleEntity battleEntity)
    {
        return collision.collsionSize;
    }

}
