
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 玩家渲染类
/// </summary>
public class PlayerView : MonoBehaviour
{
    private Animator animator;

    /// <summary>
    /// 玩家Id
    /// </summary>
    public int playerId { get; set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public async void Init(int id)
    {
        playerId = id;
        await CoLoadCharacter();
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    public async UniTask<GameObject> CoLoadCharacter()
    {
        GameObject character = await Resources.LoadAsync<GameObject>(BattleConstant.playerCharacterPath) as GameObject;
        GameObject go = Instantiate(character, transform, false);
        animator = go.GetComponentInChildren<Animator>();
        return go;
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
    public void RenderUpdate(BattleEntity battleEntity, int playerId, float deltaTime)
    {
        PlayerEntity playerEntity = battleEntity.FindPlayer(playerId);
        TransformUpdate(playerEntity, deltaTime);
        AnimationUpdate(playerEntity);
    }

    private int _yaw;
    private Vector3 _startPosition;
    private Vector3 _position;
    private Vector3 _statePosition;
    private Vector3 _moveDirection;
    private Vector3 _move;
    private Vector3 _delta;
    private Quaternion _startRotation;
    private Quaternion _rotation;
    private Quaternion _qua;
    private void TransformUpdate(PlayerEntity playerEntity, float deltaTime)
    {
        if (!gameObject.activeSelf)
            return;
        
        _yaw = playerEntity.input.yaw;

        _startPosition = transform.position;
        _position = MathManager.ToVector3(playerEntity.transform.pos);
        _statePosition = _position;

        _moveDirection = MathManager.ToVector3(playerEntity.movement.moveDirection).normalized;
        if (_moveDirection == Vector3.zero)
        {
            _moveDirection = MathManager.FromYawToVector3(_yaw).normalized;
        }
        _move = _moveDirection * playerEntity.movement.moveSpeed;

        _position += _move * deltaTime;
        _delta = Vector3.Lerp(_startPosition - _statePosition, Vector3.zero, 0.0f);
        transform.position = _position + _delta;

        _startRotation = transform.rotation;
        _rotation = MathManager.ToQuaternion(playerEntity.transform.rot);
        if (_rotation == Quaternion.identity)
        {
            if (_yaw == -1)
                _rotation = _startRotation;
            else
                _rotation = MathManager.FromYaw(_yaw);
        }
        _qua = Quaternion.Lerp(_startRotation, _rotation, playerEntity.movement.turnSpeed * deltaTime);
        transform.rotation = _qua;
    }

    private EAnimationID animId;
    private EAnimationID _lastAnimationId = EAnimationID.None;
    private void AnimationUpdate(PlayerEntity playerEntity)
    {
        if (!gameObject.activeSelf)
            return;
        animId = playerEntity.animation.animId;
        if(animator && _lastAnimationId != animId)
        {
            _lastAnimationId = animId;
            AnimationManager.CrossFadeInFixedTime(
                animator,
                AnimtionConstant.aniamtionNames[(int)animId],
                playerEntity.animation.fixedTransitionDuration,
                playerEntity.animation.layer,
                playerEntity.animation.fixedTimeOffset,
                playerEntity.animation.normalizedTransitionTime
                );
        }
    }

}
