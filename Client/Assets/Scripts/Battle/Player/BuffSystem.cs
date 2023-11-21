using System.Collections.Generic;

public class BuffSystem
{

    private static List<PlayerBuff> _activeBuffContainer = new List<PlayerBuff>();
    private static List<PlayerBuff> _unactiveBuffContainer = new List<PlayerBuff>();

    public static void Update(PlayerEntity playerEntity, BattleEntity battleEntity)
    {

    }

    public static List<PlayerBuff> TryEnableBuff(PlayerEntity playerEntity, BattleEntity battleEntity, EBuffTriggerType triggerType)
    {
        _activeBuffContainer.Clear();

        var unactiveBuffs = playerEntity.RuntimeProperty.unActiveBuffs;
        for (int index = 0; index < unactiveBuffs.Count; index++)
        {
            var buff = unactiveBuffs[index];
            if((int)triggerType == buff.triggleType && buff.Trigger(playerEntity, battleEntity))
            {
                _activeBuffContainer.Add(buff);
                unactiveBuffs.RemoveAt(index);
            }
        }

        var activeBuffs = playerEntity.RuntimeProperty.activeBuffs;
        using (var iterator = activeBuffs.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                var buff = iterator.Current;
                buff.Enable(playerEntity, battleEntity);
                if(!activeBuffs.Exists(e => e.buffId == buff.buffId))
                {
                    activeBuffs.Add(buff);
                }
            }
        }

        return _activeBuffContainer;
    }

    public static List<PlayerBuff> TryDisableBuff(PlayerEntity playerEntity, BattleEntity battleEntity, EBuffTriggerType triggerType)
    {
        _unactiveBuffContainer.Clear();

        var activeBuffs = playerEntity.RuntimeProperty.activeBuffs;
        for (int index = 0; index < activeBuffs.Count; index++)
        {
            var buff = activeBuffs[index];
            if ((int)triggerType == buff.triggleType)
            {
                _unactiveBuffContainer.Add(buff);
                activeBuffs.RemoveAt(index);
            }
        }

        var unactiveBuffs = playerEntity.RuntimeProperty.unActiveBuffs;
        using (var iterator = unactiveBuffs.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                var buff = iterator.Current;
                buff.Disable(playerEntity, battleEntity);
                if (!unactiveBuffs.Exists(e => e.buffId == buff.buffId))
                {
                    unactiveBuffs.Add(buff);
                }
            }
        }

        return _unactiveBuffContainer;
    }

}
