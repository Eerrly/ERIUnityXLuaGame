[EntitySystem]
public class PropertySystem
{
    public static int hpState;

    [EntitySystem.Initialize]
    public static void Initialize()
    {
        hpState = BattleConstant.hpStates.Length - 1;
    }

    public static int CheckEntityHPState(BaseEntity entity)
    {
        var progress = entity.property.hp / (entity.property.camp == 0 ? PlayerPropertyConstant.HP : EnemyPropertyConstant.HP);
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
