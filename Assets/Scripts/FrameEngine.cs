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

    /// <summary>
    /// 开启逻辑线程
    /// </summary>
    public void StartEngine()
    {
        BattleManager.mainThreadId = Thread.CurrentThread.ManagedThreadId;
        _logicThread = new Thread(new ThreadStart(LogicThreadUpdate));
        _logicThread.IsBackground = true;
        _logicThread.Start();
    }

    /// <summary>
    /// 逻辑线程轮询
    /// </summary>
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

    /// <summary>
    /// 注册轮询方法
    /// </summary>
    /// <param name="listener">轮询方法</param>
    public void RegisterFrameUpdateListener(Action listener)
    {
        _frameUpdateListeners = listener;
    }

    /// <summary>
    /// 取消注册轮询方法
    /// </summary>
    public void UnRegisterFrameUpdateListener()
    {
        _frameUpdateListeners = null;
    }

    /// <summary>
    /// 停止战斗线程
    /// </summary>
    public void StopEngine()
    {
        _threadStop = true;
        if(_logicThread != null)
        {
            _logicThread.Join();
            _logicThread = null;
        }
        _threadStop = false;
    }

}
