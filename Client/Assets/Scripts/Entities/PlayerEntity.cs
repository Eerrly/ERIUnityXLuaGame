﻿public class PlayerEntity : BaseEntity
{

    #region Components
    public InputComponent input = new InputComponent();
    public AttackComponent attack = new AttackComponent();
    #endregion

    internal override void Init(BattlePlayerCommonData data)
    {
        ID = data.pos;

        input.yaw = MathManager.YawStop;
        input.key = 0;

        animation.fixedTransitionDuration = 0.0f;
        animation.layer = -1;
        animation.fixedTimeOffset = 0.0f;
        animation.normalizedTransitionTime = 0.0f;

        attack.atk = PlayerPropertyConstant.Attack;
        attack.attackDistance = PlayerPropertyConstant.AttackDistance;
        attack.lastAttackTime = -1;

        runtimeProperty.seed = BattleConstant.randomSeed;

        property.hp = PlayerPropertyConstant.HP;
        property.camp = (ECamp)data.camp;
        property.collsionSize = PlayerPropertyConstant.CollisionRadius;

        state.curStateId = (int)EPlayerState.None;
        state.nextStateId = (int)EPlayerState.Idle;

        movement.moveSpeed = PlayerPropertyConstant.MoveSpeed;
        movement.turnSpeed = PlayerPropertyConstant.TurnSpeed;

        InitBuffs();
    }

    void InitBuffs()
    {
        runtimeProperty.activeBuffs.Add(new PlayerBuff(1));
    }

    public override float GetCollisionRadius(BattleEntity battleEntity)
    {
        return property.collsionSize;
    }

}
