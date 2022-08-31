public class PlayerStateAttribute : System.Attribute
{
    public EPlayerState _state;

    public PlayerStateAttribute(EPlayerState state)
    {
        _state = state;
    }
}

public class EnemyStateAttribute : System.Attribute
{
    public EEnemyState _state;

    public EnemyStateAttribute(EEnemyState state)
    {
        _state = state;
    }
}

public class BattleStateAttribute : System.Attribute
{
    public EBattleState _state;

    public BattleStateAttribute(EBattleState state)
    {
        _state = state;
    }
}

public class EntitySystem : System.Attribute
{
    public class Initialize : System.Attribute
    {
    }

    public class Release : System.Attribute
    {
    }
}
