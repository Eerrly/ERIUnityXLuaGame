public abstract class IBattleController
{

    public bool Paused { get; private set; }

    public abstract void Initialize();

    public abstract void LogicUpdate();

    public abstract void RenderUpdate();

    public abstract void Release();

    public void SwitchProceedingStatus(bool pause) { Paused = pause; }

}
