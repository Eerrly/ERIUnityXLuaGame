using System;

[Serializable]
public class BattlePlayerCommonData
{
    public int pos;
}

[Serializable]
public class BattleCommonData
{
    public BattlePlayerCommonData[] players;
}
