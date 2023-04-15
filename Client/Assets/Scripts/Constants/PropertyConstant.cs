public class PlayerPropertyConstant
{

    public static readonly FixedNumber MoveSpeed = FixedNumber.MakeFixNum(5 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    public const float TurnSpeed = 15f;

    public const float Defensiveness = 5.0f;

    public const float Attack = 50.0f;

    public const float AttackDistance = 2.0f;

    public const float AttackCdTime = 1.0f;

    public const float KnockbackCumulativeValue = 30.0f;

    public static readonly FixedNumber CollisionRadius = FixedNumber.MakeFixNum(3 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    public const int atkMaxCount = 3;
}
