
/// <summary>
/// 玩家状态特性
/// </summary>
public class PlayerStateAttribute : System.Attribute
{
    public EPlayerState _state;

    public PlayerStateAttribute(EPlayerState state)
    {
        _state = state;
    }
}
