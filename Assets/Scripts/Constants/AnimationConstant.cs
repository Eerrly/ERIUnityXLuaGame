public class AnimationConstant
{
    public static readonly string[] EnemyAniamtionNames = new string[]
    {
        "None",
        "Orc_wolfrider_05_combat_idle",
        "Orc_wolfrider_06_combat_walk",
        "Orc_wolfrider_03_run",
        "Orc_wolfrider_08_attack_B",
    };

    public static readonly string[] PlayerAnimationNames = new string[]
    {
        "None",
        "WK_heavy_infantry_05_combat_idle",
        "WK_heavy_infantry_06_combat_walk",
        "WK_heavy_infantry_04_charge",
        "WK_heavy_infantry_08_attack_B",
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
