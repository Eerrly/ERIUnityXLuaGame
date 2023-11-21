/// <summary>
/// 战斗状态
/// </summary>
public enum EBattleState
{
    None = 0,

    /// <summary>
    /// 回合准备
    /// </summary>
    RoundReady = 1,

    /// <summary>
    /// 回合中
    /// </summary>
    RoundPlaying = 2,

    /// <summary>
    /// 回合结束
    /// </summary>
    RoundOver = 3,

    /// <summary>
    /// 结束
    /// </summary>
    End = 4,

    Count,
}
