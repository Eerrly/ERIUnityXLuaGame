using System;

[Serializable]
public class BattlePlayerCommonData
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public int pos;
}

[Serializable]
public class BattleCommonData
{
    /// <summary>
    /// 玩家列表
    /// </summary>
    public BattlePlayerCommonData[] players;
}
