using System.Collections.Generic;

public class BuffSystem
{
    /// <summary>
    /// 激活的BUFF列表
    /// </summary>
    private static readonly List<PlayerBuff> ActiveBuffContainer = new List<PlayerBuff>();
    
    /// <summary>
    /// 未激活的BUFF列表
    /// </summary>
    private static readonly List<PlayerBuff> InactiveBuffContainer = new List<PlayerBuff>();

    public static void Update(PlayerEntity playerEntity, BattleEntity battleEntity)
    {

    }

    public static List<PlayerBuff> TryEnableBuff(PlayerEntity playerEntity, BattleEntity battleEntity, EBuffTriggerType triggerType)
    {
        ActiveBuffContainer.Clear();

        var inactiveBuffs = playerEntity.RuntimeProperty.inactiveBuffs;
        for (var index = 0; index < inactiveBuffs.Count; index++)
        {
            var buff = inactiveBuffs[index];
            if ((int)triggerType != buff.triggleType || !buff.Trigger(playerEntity, battleEntity)) continue;
            
            ActiveBuffContainer.Add(buff);
            inactiveBuffs.RemoveAt(index);
        }

        var activeBuffs = playerEntity.RuntimeProperty.activeBuffs;
        using (var iterator = activeBuffs.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                var buff = iterator.Current;
                if (buff == null) continue;
                
                buff.Enable(playerEntity, battleEntity);
                if (!activeBuffs.Exists(e => e.buffId == buff.buffId))
                {
                    activeBuffs.Add(buff);
                }
            }
        }

        return ActiveBuffContainer;
    }

    public static List<PlayerBuff> TryDisableBuff(PlayerEntity playerEntity, BattleEntity battleEntity, EBuffTriggerType triggerType)
    {
        InactiveBuffContainer.Clear();

        var activeBuffs = playerEntity.RuntimeProperty.activeBuffs;
        for (var index = 0; index < activeBuffs.Count; index++)
        {
            var buff = activeBuffs[index];
            if ((int)triggerType != buff.triggleType) continue;
            
            InactiveBuffContainer.Add(buff);
            activeBuffs.RemoveAt(index);
        }

        var inactiveBuffs = playerEntity.RuntimeProperty.inactiveBuffs;
        using (var iterator = inactiveBuffs.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                var buff = iterator.Current;
                if (buff == null) continue;
                
                buff.Disable(playerEntity, battleEntity);
                if (!inactiveBuffs.Exists(e => e.buffId == buff.buffId))
                {
                    inactiveBuffs.Add(buff);
                }
            }
        }

        return InactiveBuffContainer;
    }

}
