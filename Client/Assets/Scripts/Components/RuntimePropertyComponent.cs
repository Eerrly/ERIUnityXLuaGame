using System.Collections.Generic;

/// <summary>
/// 运行时属性组件
/// </summary>
public class RuntimePropertyComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public uint seed;

        public Common(int no)
        {
            seed = default(uint);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 随机种子
    /// </summary>
    public uint seed
    {
        get { return common.seed; }
        set { common.seed = value; }
    }

    private List<PlayerBuff> _unactiveBuffs = new List<PlayerBuff>();
    /// <summary>
    /// 未激活的BUFF
    /// </summary>
    public List<PlayerBuff> unactiveBuffs
    {
        get { return _unactiveBuffs; }
        set { _unactiveBuffs = value; }
    }

    private List<PlayerBuff> _activeBuffs = new List<PlayerBuff>();
    /// <summary>
    /// 激活的BUFF
    /// </summary>
    public List<PlayerBuff> activeBuffs
    {
        get { return _activeBuffs; }
        set { _activeBuffs = value; }
    }

    private List<PlayerBuffExtra> _buffExtra = new List<PlayerBuffExtra>();
    /// <summary>
    /// BUFF属性
    /// </summary>
    public List<PlayerBuffExtra> buffExtra
    {
        get { return _buffExtra; }
        set { _buffExtra = value; }
    }

    private List<PhysisPlayer> _closedPlayers = new List<PhysisPlayer>();
    /// <summary>
    /// 有碰撞的玩家集合
    /// </summary>
    public List<PhysisPlayer> closedPlayers
    {
        get { return _closedPlayers; }
        set { _closedPlayers = value; }
    }

}
