public class PlayerPropertyConstant
{
    /// <summary>
    /// 移动速度
    /// </summary>
    public static readonly FixedNumber MoveSpeed = FixedNumber.MakeFixNum(5 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    /// <summary>
    /// 转向速度
    /// </summary>
    public const float TurnSpeed = 180f;

    /// <summary>
    /// 碰撞半径
    /// </summary>
    public static readonly FixedNumber CollisionRadius = FixedNumber.MakeFixNum(FixedMath.DataConrvertScale / 2, FixedMath.DataConrvertScale);
}
