public class AnimationConstant
{
    public static readonly string[] aniamtionNames = new string[]
    {
        "None",
        "Idle",
        "Walk",
        "Run",
        "Attack",
    };
}

public enum EAnimationID
{
    None = 0,
    Idle = 1,
    Walk = 2,
    Run = 3,
    Attack = 4,
    Count,
}
