using System.Diagnostics;

[EntitySystem]
public static class TimeSystem
{
    private static Stopwatch _watch;

    [EntitySystem.Initialize]
    public static void Initialize()
    {
        _watch = new Stopwatch();
        _watch.Start();
    }

    [EntitySystem.Release]
    public static void Release()
    {
        if(_watch != null)
        {
            _watch.Stop();
            _watch = null;
        }
    }

    public static long GetElapsedMilliseconds()
    {
        if(_watch != null)
        {
            return _watch.ElapsedMilliseconds;
        }
        return -1;
    }

}
