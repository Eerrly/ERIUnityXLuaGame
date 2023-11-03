/// <summary>
/// 战斗执行器基类
/// </summary>
public abstract class IBattleController
{
    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool Paused { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// 逻辑轮询
    /// </summary>
    public abstract void LogicUpdate();

    /// <summary>
    /// 渲染轮询
    /// </summary>
    public abstract void RenderUpdate();

    /// <summary>
    /// 释放
    /// </summary>
    public abstract void Release();

    public void SwitchProceedingStatus(bool pause) { Paused = pause; }

    /// <summary>
    /// 结束
    /// </summary>
    public abstract void GameOver();

}
