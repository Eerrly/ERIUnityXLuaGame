[EntitySystem]
public class PropertySystem
{
    public static int hpState;

    [EntitySystem.Initialize]
    public static void Initialize()
    {
        hpState = BattleConstant.hpStates.Length - 1;
    }

    public static int CheckEntityHPState(PlayerEntity playerEntity)
    {
        var progress = playerEntity.property.hp / PlayerPropertyConstant.HP;
        for (int i = 0; i < BattleConstant.hpStates.Length; i++)
        {
            if(progress >= BattleConstant.hpStates[i])
            {
                hpState = i;
            }
        }
        return hpState;
    }

}
