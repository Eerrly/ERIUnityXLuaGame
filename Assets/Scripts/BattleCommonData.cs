using System;

[Serializable]
public class BattlePlayerCommonData
{
    public string name;
    public int level;
}

[Serializable]
public class BattleCommonData
{
    public int mode;
    public BattlePlayerCommonData[] players;
}
