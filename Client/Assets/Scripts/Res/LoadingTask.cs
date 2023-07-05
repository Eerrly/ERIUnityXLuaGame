using System.Collections.Generic;

/// <summary>
/// 加载任务
/// </summary>
public class LoadingTask
{
    /// <summary>
    /// 加载任务的状态
    /// </summary>
    public ELoadingState state;
    public string path;
    public string name;
    public string extension;
    public uint hash;
    public Resource file;
    public string error;

    /// <summary>
    /// 字节偏移
    /// </summary>
    public uint offset;

    /// <summary>
    /// 是否为异步加载
    /// </summary>
    public bool async;

    /// <summary>
    /// 是否为依赖资源
    /// </summary>
    public bool isDependency;

    /// <summary>
    /// 多少个依赖引用
    /// </summary>
    public int dependencyRefCount;

    public Dictionary<string, bool> namesDict = new Dictionary<string, bool>();
    public List<System.Action<Resource>> onLoadedCallbacks = new List<System.Action<Resource>>();

    /// <summary>
    /// 重置数据
    /// </summary>
    public void Reset()
    {
        state = ELoadingState.None;
        path = default(string);
        name = default(string);
        extension = default(string);
        hash = default(uint);
        file = null;
        error = default(string);
        offset = default(uint);
        async = default(bool);
        isDependency = default(bool);
        dependencyRefCount = default(int);
        namesDict?.Clear();
        onLoadedCallbacks?.Clear();
    }
}
