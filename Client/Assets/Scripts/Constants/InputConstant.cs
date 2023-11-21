public class InputConstant
{
    public const string Vertical = "Vertical";

    public const string Horizontal = "Horizontal";

    public const string KeyCodeSpace = "tab";

    public const string KeyCodeJ = "j";

    public const string KeyCodeK = "k";

    public const string KeyCodeL = "l";
}

public enum ELogicInputKey
{
    None = -1,
    Tab = 1 << 0,
    J = 1 << 1,
    K = 1 << 2,
    L = 1 << 3,
    KeyCount,
}