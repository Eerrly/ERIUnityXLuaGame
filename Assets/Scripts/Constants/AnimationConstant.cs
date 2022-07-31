public class AnimationConstant
{
    public static readonly string[] aniamtionNames = new string[]
    {
        "Idle",
        "Run",
        "Attack",
    };
}

public enum EAnimationID
{
    None = -1,
    Idle = 0,
    Move = 1,
    Attack = 2,
}
