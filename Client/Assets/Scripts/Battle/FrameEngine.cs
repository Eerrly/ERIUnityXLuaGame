using System;
using System.Threading;

public class FrameEngine
{
    /// <summary>
    /// 暂停
    /// </summary>
    public static bool Pause { get; private set; } = false;

    /// <summary>
    /// 一帧多少MS
    /// </summary>
    public static float FrameInterval { get; private set; }

    private Action _frameUpdateListeners = null;
    private Action _netUpdateListeners = null;

    private bool _threadStop = false;
    private Thread _logicThread;
    private Thread _netThread;

    public static void SetFrameInterval(float frameInterval)
    {
        FrameInterval = frameInterval;
    }

    public void StartEngine(float frameInterval)
    {
        SetFrameInterval(frameInterval);
        BattleManager.MainThreadId = Thread.CurrentThread.ManagedThreadId;
        _logicThread = new Thread(new ThreadStart(LogicThreadUpdate))
        {
            IsBackground = true
        };
        _logicThread.Start();
        _netThread = new Thread(new ThreadStart(NetThreadUpdate))
        {
            IsBackground = true
        };
        _netThread.Start();
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
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Release), false);
    }

    private void NetThreadUpdate()
    {
        while (!_threadStop)
        {
            if(_netUpdateListeners != null && !Pause)
            {
                _netUpdateListeners();
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

    public void RegisterNetUpdateListener(Action listener)
    {
        _netUpdateListeners = listener;
    }

    public void UnRegisterNetUpdateListener()
    {
        _netUpdateListeners = null;
    }

    public void StopEngine()
    {
        _threadStop = true;
        if(_logicThread != null)
        {
            _logicThread.Join();
            _logicThread = null;
        }
        if(_netThread != null)
        {
            _netThread.Join();
            _netThread = null;
        }
        _threadStop = false;
    }

}
