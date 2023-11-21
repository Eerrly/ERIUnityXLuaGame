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
        get => common.seed;
        set => common.seed = value;
    }

    private List<PlayerBuff> _unActiveBuffs = new List<PlayerBuff>();
    /// <summary>
    /// 未激活的BUFF
    /// </summary>
    public List<PlayerBuff> unActiveBuffs
    {
        get => _unActiveBuffs;
        set => _unActiveBuffs = value;
    }

    private List<PlayerBuff> _activeBuffs = new List<PlayerBuff>();
    /// <summary>
    /// 激活的BUFF
    /// </summary>
    public List<PlayerBuff> activeBuffs
    {
        get => _activeBuffs;
        set => _activeBuffs = value;
    }

    private List<PlayerBuffExtra> _buffExtra = new List<PlayerBuffExtra>();
    /// <summary>
    /// BUFF属性
    /// </summary>
    public List<PlayerBuffExtra> buffExtra
    {
        get => _buffExtra;
        set => _buffExtra = value;
    }

    private List<PhysisPlayer> _closedPlayers = new List<PhysisPlayer>();
    /// <summary>
    /// 有碰撞的玩家集合
    /// </summary>
    public List<PhysisPlayer> closedPlayers
    {
        get => _closedPlayers;
        set => _closedPlayers = value;
    }

}
