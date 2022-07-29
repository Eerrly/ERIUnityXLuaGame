
/// <summary>
/// 状态组件
/// </summary>
public class StateComponent : BaseComponent
{
    
    internal class Common
    {
        public int curStateId;
        public int nextStateId;
        public int prevStateId;
        public float enterTime;
        public float exitTime;

        public Common()
        {
            curStateId = default(int);
            nextStateId = default(int);
            prevStateId = default(int);
            enterTime = default(float);
            exitTime = default(float);
        }
    }

    private Common common = new Common();

    /// <summary>
    /// 当前状态Id
    /// </summary>
    public int curStateId => common.curStateId;

    /// <summary>
    /// 下一个状态Id
    /// </summary>
    public int nextStateId => common.nextStateId;

    /// <summary>
    /// 上一个状态Id
    /// </summary>
    public int preStateId => common.prevStateId;

    /// <summary>
    /// 进入状态的时间
    /// </summary>
    public float enterTime => common.enterTime;

    /// <summary>
    /// 退出状态的时间
    /// </summary>
    public float exitTime => common.exitTime;

}
