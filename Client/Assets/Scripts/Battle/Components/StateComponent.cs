/// <summary>
/// 状态组件
/// </summary>
public class StateComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int curStateId;
        public int nextStateId;
        public int prevStateId;
        public float enterTime;
        public float exitTime;
        public int count;

        public Common(int no)
        {
            curStateId = default(int);
            nextStateId = default(int);
            prevStateId = default(int);
            enterTime = default(float);
            exitTime = default(float);
            count = default(int);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 当前状态ID
    /// </summary>
    public int curStateId
    {
        get { return common.curStateId; }
        set { common.curStateId = value; }
    }

    /// <summary>
    /// 下一个状态ID
    /// </summary>
    public int nextStateId
    {
        get { return common.nextStateId; }
        set { common.nextStateId = value; }
    }

    /// <summary>
    /// 上一个状态ID
    /// </summary>
    public int preStateId
    {
        get { return common.prevStateId; }
        set { common.prevStateId = value; }
    }

    /// <summary>
    /// 进入时间
    /// </summary>
    public float enterTime
    {
        get { return common.enterTime; }
        set { common.enterTime = value; }
    }

    /// <summary>
    /// 退出时间
    /// </summary>
    public float exitTime
    {
        get { return common.exitTime; }
        set { common.exitTime = value; }
    }

    /// <summary>
    /// 状态数量
    /// </summary>
    public int count
    {
        get { return common.count; }
        set { common.count = value; }
    }

}
