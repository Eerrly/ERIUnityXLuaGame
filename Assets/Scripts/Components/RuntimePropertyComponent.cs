using System.Collections.Generic;

public class RuntimePropertyComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int seed;

        public Common(int no)
        {
            seed = default(int);
        }
    }

    private Common common = new Common(0);

    public int seed
    {
        get { return common.seed; }
        set { common.seed = value; }
    }

    private List<PlayerBuff> _unactiveBuffs = new List<PlayerBuff>();
    public List<PlayerBuff> unactiveBuffs
    {
        get { return _unactiveBuffs; }
        set { _unactiveBuffs = value; }
    }

    private List<PlayerBuff> _activeBuffs = new List<PlayerBuff>();
    public List<PlayerBuff> activeBuffs
    {
        get { return _activeBuffs; }
        set { _activeBuffs = value; }
    }

    private List<PlayerBuffExtra> _buffExtra = new List<PlayerBuffExtra>();
    public List<PlayerBuffExtra> buffExtra
    {
        get { return _buffExtra; }
        set { _buffExtra = value; }
    }

}
