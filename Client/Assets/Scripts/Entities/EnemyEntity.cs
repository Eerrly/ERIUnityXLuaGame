
public class EnemyEntity : BaseEntity
{

    internal override void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;

        animation.fixedTransitionDuration = 0.0f;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;
        animation.enable = true;

        attack.targets = new int[EnemyPropertyConstant.atkMaxCount];
        attack.atk = EnemyPropertyConstant.Attack;
        attack.attackDistance = EnemyPropertyConstant.AttackDistance;
        attack.lastAttackTime = -1;

        runtimeProperty.seed = BattleConstant.randomSeed;

        property.hp = EnemyPropertyConstant.HP;
        property.camp = (ECamp)data.camp;

        collision.collsionSize = EnemyPropertyConstant.CollisionRadius;

        state.curStateId = (int)EEnemyState.None;
        state.nextStateId = (int)EEnemyState.Idle;
        state.count = (int)EEnemyState.Count;

        movement.moveSpeed = EnemyPropertyConstant.MoveSpeed;
        movement.turnSpeed = EnemyPropertyConstant.TurnSpeed;

        InitBuffs();
    }

    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    public override float GetCollisionRadius(BattleEntity battleEntity)
    {
        return collision.collsionSize;
    }

}
