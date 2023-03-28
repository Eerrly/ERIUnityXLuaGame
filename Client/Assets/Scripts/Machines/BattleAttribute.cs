/// <summary>
/// 玩家状态属性类
/// </summary>
public class PlayerStateAttribute : System.Attribute
{
    public EPlayerState _state;

    public PlayerStateAttribute(EPlayerState state)
    {
        _state = state;
    }
}

/// <summary>
/// 战斗状态属性类
/// </summary>
public class BattleStateAttribute : System.Attribute
{
    public EBattleState _state;

    public BattleStateAttribute(EBattleState state)
    {
        _state = state;
    }
}

/// <summary>
/// 实体属性类
/// </summary>
public class EntitySystem : System.Attribute
{
    public class Initialize : System.Attribute
    {
    }

    public class Release : System.Attribute
    {
    }
}
