/// <summary>
/// 加载任务的状态
/// </summary>
public enum ELoadingState
{
    None,
    /// <summary>
    /// 准备加载
    /// </summary>
    Ready,

    /// <summary>
    /// 正在加载
    /// </summary>
    Loading,

    /// <summary>
    /// 加载完成
    /// </summary>
    Finished,
}
