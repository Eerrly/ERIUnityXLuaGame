public class AnimationConstant
{
    public static readonly string[] aniamtionNames = new string[]
    {
        "None",
        "Idle",
        "Run",
        "Attack",
    };
}

public enum EAnimationID
{
    None = 0,
    Idle = 1,
    Move = 2,
    Attack = 3,
    Count,
}
