using System.Collections.Generic;

public class LoadingTask
{
    public ELoadingState state;
    public string path;
    public string name;
    public string extension;
    public uint hash;
    public Resource file;
    public string error;
    public uint crc;
    public uint offset;
    public bool async;
    public bool isDependency;
    public int dependencyRefCount;
    public Dictionary<string, bool> namesDict = new Dictionary<string, bool>();
    public List<System.Action<Resource>> onLoadedCallbacks = new List<System.Action<Resource>>();

    public void Reset()
    {
        state = ELoadingState.None;
        path = default(string);
        name = default(string);
        extension = default(string);
        hash = default(uint);
        file = null;
        error = default(string);
        crc = default(uint);
        offset = default(uint);
        async = default(bool);
        isDependency = default(bool);
        dependencyRefCount = default(int);
        namesDict.Clear();
        onLoadedCallbacks.Clear();
    }
}
