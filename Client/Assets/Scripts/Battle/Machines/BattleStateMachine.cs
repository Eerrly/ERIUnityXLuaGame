using System;

public class BattleStateMachine : BaseStateMachine<BattleEntity>
{

    private static BattleStateMachine _instance;

    public static BattleStateMachine Instance => _instance ?? (_instance = new BattleStateMachine());

    private BattleStateMachine()
            : base()
    {
        var types = GetType().Assembly.GetTypes();
        _stateDic = new BaseState<BattleEntity>[(int)EBattleState.Count];
        foreach (var singleType in types)
        {
            if (singleType.IsDefined(typeof(BattleStateAttribute), false))
            {
                var attribute = (BattleStateAttribute)(singleType.GetCustomAttributes(false)[0]);
                var state = System.Activator.CreateInstance(singleType) as BattleBaseState;
                state.StateId = attribute._state;
                var stateId = (int)state.StateId;
#if UNITY_EDITOR
                if (null != _stateDic[stateId])
                {
                    UnityEngine.Debug.LogErrorFormat("The {0} state has a instance, please check. now {1} other {2}", state.StateId, singleType, _stateDic[stateId].GetType());
                }
                else
#endif
                {
                    _stateDic[stateId] = state;
                }

#if UNITY_EDITOR
                var fields = singleType.GetFields();
                if (fields.Length > 0)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has filed!", singleType);
                }

                var properties = singleType.GetProperties();
                if (properties.Length > 1)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has property!", singleType);
                }
#endif
            }
        }
    }

    public override void Update(BattleEntity battleEntity, BattleEntity _)
    {
        var curState = _stateDic[battleEntity.State.curStateId];
        curState.OnUpdate(battleEntity, null);
    }

    public void LateUpdate(BattleEntity battleEntity, BattleEntity _)
    {
        var curState = _stateDic[battleEntity.State.curStateId];
        curState.OnLateUpdate(battleEntity, null);
    }

    public bool DoChangeState(BattleEntity battleEntity, BattleEntity _)
    {
        var nextId = battleEntity.State.nextStateId;
        if (nextId != 0 && _stateDic[nextId] is BattleBaseState nextState && nextState.TryEnter(battleEntity, null))
        {
            var currId = battleEntity.State.curStateId;
            if (currId != 0 && _stateDic[currId] is BattleBaseState currState && currState.TryExit(battleEntity, null))
            {
                currState.OnExit(battleEntity, null);
            }
            battleEntity.State.nextStateId = (int)EBattleState.None;
            nextState.Reset(battleEntity, null);
            nextState.OnEnter(battleEntity, null);
            if (battleEntity.State.nextStateId != (int)EBattleState.None)
            {
                return DoChangeState(battleEntity, null);
            }
            return true;
        }
        else
        {
            battleEntity.State.nextStateId = (int)EBattleState.None;
        }
        return false;
    }

}
