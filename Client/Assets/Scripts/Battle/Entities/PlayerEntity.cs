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

        Input.pos = ID;
        Input.yaw = MathManager.YawStop;
        Input.key = 0;

        RuntimeProperty.seed = BattleConstant.randomSeed;

        Property.pos = data.pos;

        Collision.collisionSize = PlayerPropertyConstant.CollisionRadius;

        State.curStateId = (int)EPlayerState.None;
        State.nextStateId = (int)EPlayerState.Idle;
        State.count = (int)EPlayerState.Count;

        Movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        Movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;

        InitBuffs();
    }

    /// <summary>
    /// 初始化Buff列表
    /// </summary>
    private void InitBuffs()
    {
        RuntimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    /// <summary>
    /// 获取碰撞半径
    /// </summary>
    /// <param name="battleEntity">战斗实体</param>
    /// <returns>半径</returns>
    public override FixedNumber GetCollisionRadius(BattleEntity battleEntity)
    {
        return Collision.collisionSize;
    }

}
