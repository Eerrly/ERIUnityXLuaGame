using System;
using System.Threading;

/// <summary>
/// 管理EngineUpdate
/// </summary>
public class FrameEngine
{
    /// <summary>
    /// 暂停
    /// </summary>
    private bool _pause = false;
    public bool Pause
    {
        get { return _pause; }
        set { _pause = value; }
    }

    /// <summary>
    /// 时间缩放
    /// </summary>
    private float _timeScale = 0.0f;
    public float TimeScale
    {
        get { return _timeScale; }
        set { _timeScale = value; }
    }

    private Action _frameUpdateListeners = null;

    private bool _threadStop = false;
    private Thread _logicThread;

    public void StartEngine()
    {
        BattleManager.mainThreadId = Thread.CurrentThread.ManagedThreadId;
        _logicThread = new Thread(new ThreadStart(LogicThreadUpdate));
        _logicThread.IsBackground = true;
        _logicThread.Start();
    }

    private void LogicThreadUpdate()
    {
        while (!_threadStop)
        {
            if(_frameUpdateListeners != null && !Pause)
            {
                _frameUpdateListeners();
            }
            Thread.Sleep(1);
        }
    }

    public void RegisterFrameUpdateListener(Action listener)
    {
        _frameUpdateListeners = listener;
    }

    public void UnRegisterFrameUpdateListener()
    {
        _frameUpdateListeners = null;
    }

}
