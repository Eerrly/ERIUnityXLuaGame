[EnemyState(EEnemyState.Dead)]
public class EnemyDeadState : EnemyBaseState
{

    public override bool TryEnter(EnemyEntity entity, BattleEntity battleEntity)
    {
        return entity.property.hp <= 0;
    }

    public override void OnEnter(EnemyEntity enemyEntity, BattleEntity battleEntity)
    {
        enemyEntity.animation.loop = false;
        AnimationSystem.EnableAnimator(enemyEntity, false);
    }

}
