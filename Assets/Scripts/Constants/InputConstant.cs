public class InputConstant
{
    public static readonly string Vertical = "Vertical";

    public static readonly string Horizontal = "Horizontal";

    public static readonly string KeyCodeSpace = "space";

    public static readonly string KeyCodeJ = "j";

    public static readonly string KeyCodeK = "k";

    public static readonly string KeyCodeL = "l";
}

public enum ELogicInputKey
{
    Space = 1 << 0,
    J = 1 << 1,
    K = 1 << 2,
    L = 1 << 3,
    KeyCount,
}