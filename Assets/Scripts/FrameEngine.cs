using System;
using System.Threading;

public class FrameEngine
{

    private bool _pause = false;
    public bool Pause
    {
        get { return _pause; }
        set { _pause = value; }
    }

    private float _timeScale = 0.0f;
    public float TimeScale
    {
        get { return _timeScale; }
        set { _timeScale = value; }
    }

    private static float _frameInterval;
    public static float frameInterval
    {
        get { return _frameInterval; }
        private set { _frameInterval = value; }
    }

    private Action _frameUpdateListeners = null;

    private bool _threadStop = false;
    private Thread _logicThread;

    public static void SetFrameInterval(float frameInterval)
    {
        _frameInterval = frameInterval;
    }

    public void StartEngine(float frameInterval)
    {
        SetFrameInterval(frameInterval);
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
        Util.InvokeAttributeCall(this, typeof(EntitySystem), false, typeof(EntitySystem.Release), false);
    }

    public void RegisterFrameUpdateListener(Action listener)
    {
        _frameUpdateListeners = listener;
    }

    public void UnRegisterFrameUpdateListener()
    {
        _frameUpdateListeners = null;
    }

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
