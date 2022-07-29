
public class PlayerStateMachine : BaseStateMachine<PlayerEntity>
{
    private static PlayerStateMachine _instance;

    public static PlayerStateMachine Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new PlayerStateMachine();
            }
            return _instance;
        }
    }

    private PlayerStateMachine() : base()
    {
        var types = GetType().Assembly.GetExportedTypes();
        _stateDic = new BaseState<PlayerEntity>[(int)EPlayerState.Count];
        for (var i = 0; i < types.Length; ++i)
        {
            if (types[i].IsDefined(typeof(PlayerStateAttribute), false))
            {
                var state = System.Activator.CreateInstance(types[i]) as PlayerBaseState;
                var attributes = types[i].GetCustomAttributes(false);
                for (var j = 0; j < attributes.Length; ++j)
                {
                    if (attributes[j] is PlayerStateAttribute)
                    {
                        var attr = (attributes[j] as PlayerStateAttribute);
                        state.StateId = attr._state;
                    }
                }

                var stateId = (int)state.StateId;

#if UNITY_EDITOR
                if (null != _stateDic[stateId])
                {
                    UnityEngine.Debug.LogErrorFormat("The {0} state has a instance, please check. now {1} other {2}", state.StateId, types[i], _stateDic[stateId].GetType());
                }
                else
#endif
                {
                    _stateDic[stateId] = state;
                }
#if UNITY_EDITOR
                var fileds = types[i].GetFields();
                if (fileds.Length > 0)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has filed!", types[i]);
                }

                var properties = types[i].GetProperties();
                if (properties.Length > 4)
                {
                    UnityEngine.Debug.LogErrorFormat("State:{0} has property!", types[i]);
                }
#endif

            }
        }
    }

    public override void Update(PlayerEntity playerEntity)
    {
        base.Update(playerEntity);
        var curState = _stateDic[(int)playerEntity.curStateId];
        curState.OnUpdate(playerEntity);
    }

    public void LateUpdate(PlayerEntity playerEntity)
    {
        var curState = _stateDic[(int)playerEntity.curStateId];
        curState.OnLateUpdate(playerEntity);
    }

}
