
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private AnimatedMeshAnimator animator;

    public int playerId { get; set; }

    public async void Init(int id)
    {
        playerId = id;
        await CoLoadCharacter();
    }

    public async UniTask<GameObject> CoLoadCharacter()
    {
        var resourcePrefabPath = playerId == 0 ? BattleConstant.playerCharacterPath : BattleConstant.enemyCharacterPath;
        GameObject character = await Resources.LoadAsync<GameObject>(resourcePrefabPath) as GameObject;
        GameObject go = Instantiate(character, transform, false);
        animator = go.GetComponentInChildren<AnimatedMeshAnimator>();
        return go;
    }

    public void RenderUpdate(BattleEntity battleEntity, int playerId, float deltaTime)
    {
        PlayerEntity playerEntity = battleEntity.FindPlayer(playerId);
        TransformUpdate(playerEntity, deltaTime);
        AnimationUpdate(playerEntity);
    }

    private Vector3 _startPosition;
    private Vector3 _position;
    private Vector3 _pos;
    private Quaternion _startRotation;
    private Quaternion _rotation;
    private Quaternion _qua;
    private void TransformUpdate(PlayerEntity playerEntity, float deltaTime)
    {
        if (!gameObject.activeSelf)
            return;

        _startPosition = transform.position;
        _position = MathManager.ToVector3(playerEntity.transform.pos);
        if(Vector3.zero == _position || _position == transform.position)
        {
            _position = MathManager.ToVector3(playerEntity.movement.position);
        }
        _pos = Vector3.Lerp(_startPosition + _position * deltaTime, Vector3.zero, 0.0f);
        transform.position = _pos;

        _startRotation = transform.rotation;
        _rotation = MathManager.ToQuaternion(playerEntity.transform.rot);
        if (Quaternion.identity == _rotation || _rotation == transform.rotation)
        {
            _rotation = MathManager.ToQuaternion(playerEntity.movement.rotation);
        }
        _qua = Quaternion.Lerp(_startRotation, _rotation, playerEntity.movement.turnSpeed * deltaTime);
        transform.rotation = _qua;

        playerEntity.transform.pos = MathManager.ToFloat3(transform.position);
        playerEntity.transform.rot = MathManager.ToFloat4(transform.rotation);
    }

    private int animId;
    private int _lastAnimationId = -1;
    private void AnimationUpdate(PlayerEntity playerEntity)
    {
        if (!gameObject.activeSelf)
            return;
        animId = playerEntity.animation.animId;
        if(animator != null && animId != 0 && _lastAnimationId != animId)
        {
            var animations = playerEntity.ID == 0 ? AnimationConstant.PlayerAnimationNames : AnimationConstant.EnemyAniamtionNames;
            animator.Play(animations[animId], 0f);
            _lastAnimationId = animId;
        }
    }

}
