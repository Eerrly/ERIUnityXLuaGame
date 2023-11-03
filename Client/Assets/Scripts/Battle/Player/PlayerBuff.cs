using System.Collections.Generic;

public struct PlayerBuffExtra
{
    public int type;
    public float value;
}

public class PlayerBuff
{
    public enum EBuffStateFlag
    {
        IsEnable = 1 << 0,
        IsDisable = 1 << 1,
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int buffId;
        public int triggleType;
        public int exitType;
        public int stateFlag;
        public float activeTime;
        public float endTime;
    }

    private Common common;

    public int buffId
    {
        get { return common.buffId; }
        set { common.buffId = value; }
    }

    public int triggleType
    {
        get { return common.triggleType; }
        set { common.triggleType = value; }
    }

    public int exitType
    {
        get { return common.exitType; }
        set { common.exitType = value; }
    }

    public int stateFlag
    {
        get { return common.stateFlag; }
        set { common.stateFlag = value; }
    }

    public float activeTime
    {
        get { return common.activeTime; }
        set { common.activeTime = value; }
    }

    public float endTime
    {
        get { return common.endTime; }
        set { common.endTime = value; }
    }

    public PlayerBuff(int buffId)
    {
        this.buffId = buffId;
        activeTime = 0;
        endTime = 0;
        triggleType = (int)EBuffTriggerType.Default;
    }

    public bool CheckFlag(EBuffStateFlag flag)
    {
        return (stateFlag & (int)flag) != 0;
    }

    public void AddFlag(EBuffStateFlag flag)
    {
        stateFlag |= (int)flag;
    }

    public void ClearFlag(EBuffStateFlag flag)
    {
        stateFlag &= ~(int)flag;
    }

    public bool Trigger(PlayerEntity playerEntity, BattleEntity battleEntity, EBuffTriggerType type = EBuffTriggerType.Default, int param = 0)
    {
        if(type >= EBuffTriggerType.Max || type <= EBuffTriggerType.None)
        {
            return false;
        }
        return true;
    }

    public void Enable(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        ClearFlag(EBuffStateFlag.IsDisable);
        AddFlag(EBuffStateFlag.IsEnable);
        int foundIndex = -1;
        var buffExtra = playerEntity.runtimeProperty.buffExtra;
        for (int i = 0; i < buffExtra.Count; i++)
        {
            if(buffExtra[i].type == BuffConstant.attackBuffType)
            {
                foundIndex = i;
                break;
            }
        }
        if(foundIndex == -1)
        {
            buffExtra.Add(new PlayerBuffExtra() { type = BuffConstant.attackBuffType, value = BuffConstant.attackBuffValue });
        }
        else
        {
            var data = buffExtra[foundIndex];
            data.value += BuffConstant.attackBuffValue;
            buffExtra[foundIndex] = data;
        }
#if UNITY_EDITOR && UNITY_DEBUG
        Logger.Log(LogLevel.Info, string.Format("buffExtra count:{0} [0].type:{1} [0].value:{2}", buffExtra.Count, buffExtra[0].type, buffExtra[0].value));
#endif
    }

    public void Disable(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        ClearFlag(EBuffStateFlag.IsEnable);
        AddFlag(EBuffStateFlag.IsDisable);
        int foundIndex = -1;
        var buffExtra = playerEntity.runtimeProperty.buffExtra;
        for (int i = 0; i < buffExtra.Count; i++)
        {
            if (buffExtra[i].type == BuffConstant.attackBuffType)
            {
                foundIndex = i;
                break;
            }
        }
        if(foundIndex != -1)
        {
            buffExtra.RemoveAt(foundIndex);
        }
    }

}
