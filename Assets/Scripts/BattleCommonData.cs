using System;

[Serializable]
public class BattlePlayerCommonData
{
    public int camp;
    public string name;
    public int level;
    public int pos;
}

[Serializable]
public class BattleCommonData
{
    public int mode;
    public BattlePlayerCommonData[] players;
}
