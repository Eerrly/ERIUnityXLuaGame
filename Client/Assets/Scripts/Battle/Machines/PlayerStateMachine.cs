
using System;

public class PlayerStateMachine : BaseStateMachine<PlayerEntity>
{
    private static PlayerStateMachine _instance;

    public static PlayerStateMachine Instance => _instance ?? (_instance = new PlayerStateMachine());

    private PlayerStateMachine() : base()
    {
        var types = GetType().Assembly.GetExportedTypes();
        _stateDic = new BaseState<PlayerEntity>[(int)EPlayerState.Count];
        foreach (var t in types)
        {
            if (!t.IsDefined(typeof(PlayerStateAttribute), false)) continue;
            
            var state = System.Activator.CreateInstance(t) as PlayerBaseState;
            var attributes = t.GetCustomAttributes(false);
            foreach (var t1 in attributes)
            {
                if (t1 is PlayerStateAttribute)
                {
                    var attr = (t1 as PlayerStateAttribute);
                    state.StateId = attr._state;
                }
            }

            var stateId = (int)state.StateId;

#if UNITY_EDITOR
            if (null != _stateDic[stateId])
            {
                UnityEngine.Debug.LogErrorFormat("The {0} state has a instance, please check. now {1} other {2}", state.StateId, t, _stateDic[stateId].GetType());
            }
            else
#endif
            {
                _stateDic[stateId] = state;
            }
#if UNITY_EDITOR
            var fields = t.GetFields();
            if (fields.Length > 0)
            {
                UnityEngine.Debug.LogErrorFormat("State:{0} has filed!", t);
            }

            var properties = t.GetProperties();
            if (properties.Length > 4)
            {
                UnityEngine.Debug.LogErrorFormat("State:{0} has property!", t);
            }
#endif
        }
    }

    public override void Update(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        var curState = _stateDic[(int)playerEntity.curStateId];
        curState.OnUpdate(playerEntity, battleEntity);
    }

    public void LateUpdate(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        var curState = _stateDic[(int)playerEntity.curStateId];
        curState.OnLateUpdate(playerEntity, battleEntity);
    }

    public bool DoChangeState(PlayerEntity playerEntity, BattleEntity battleEntity)
    {
        var nextId = playerEntity.State.nextStateId;
        if(nextId != 0 && _stateDic[nextId] is PlayerBaseState nextState && nextState.TryEnter(playerEntity, battleEntity))
        {
            var currId = playerEntity.State.curStateId;
            if(currId != 0 && _stateDic[currId] is PlayerBaseState currState && currState.TryExit(playerEntity, battleEntity))
            {
                currState.OnExit(playerEntity, battleEntity);
            }
            playerEntity.State.nextStateId = (int)EPlayerState.None;
            nextState.Reset(playerEntity, battleEntity);
            nextState.OnEnter(playerEntity, battleEntity);
            if(playerEntity.State.nextStateId != (int)EPlayerState.None)
            {
                return DoChangeState(playerEntity, battleEntity);
            }
            return true;
        }
        else
        {
            playerEntity.State.nextStateId = (int)EPlayerState.None;
        }
        return false;
    }

}
