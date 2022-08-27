
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private AnimatedMeshAnimator animator;

    private int _entityId;
    public int entityId => _entityId;

    private int _camp;
    public int camp => _camp;

    public async void Init(BattlePlayerCommonData data)
    {
        _entityId = data.pos;
        _camp = data.camp;
        await CoLoadCharacter();
    }

    public async UniTask<GameObject> CoLoadCharacter()
    {
        var resourcePrefabPath = entityId == 0 ? BattleConstant.playerCharacterPath : BattleConstant.enemyCharacterPath;
        GameObject character = await Resources.LoadAsync<GameObject>(resourcePrefabPath) as GameObject;
        GameObject go = Instantiate(character, transform, false);
        animator = go.GetComponentInChildren<AnimatedMeshAnimator>();
        return go;
    }

    public void RenderUpdate(BattleEntity battleEntity, float deltaTime)
    {
        BaseEntity entity = battleEntity.FindEntity(entityId);
        TransformUpdate(entity, deltaTime);
        AnimationUpdate(entity);
        SpacePartition.UpdateEntityCell(entity);
    }

    private Vector3 p_startPosition;
    private Vector3 p_position;
    private Vector3 p_pos;
    private Quaternion p_startRotation;
    private Quaternion p_rotation;
    private Quaternion p_qua;
    private void TransformUpdate(BaseEntity entity, float deltaTime)
    {
        if (!gameObject.activeSelf)
            return;

        p_startPosition = transform.position;
        p_position = MathManager.ToVector3(entity.transform.pos);
        if(Vector3.zero == p_position || p_position == transform.position)
        {
            p_position = MathManager.ToVector3(entity.movement.position);
        }
        p_pos = Vector3.Lerp(p_startPosition + p_position * deltaTime, Vector3.zero, 0.0f);
        transform.position = p_pos;

        p_startRotation = transform.rotation;
        p_rotation = MathManager.ToQuaternion(entity.transform.rot);
        if (Quaternion.identity == p_rotation || p_rotation == transform.rotation)
        {
            p_rotation = MathManager.ToQuaternion(entity.movement.rotation);
        }
        p_qua = Quaternion.Lerp(p_startRotation, p_rotation, entity.movement.turnSpeed * deltaTime);
        transform.rotation = p_qua;

        entity.transform.pos = MathManager.ToFloat3(transform.position);
        entity.transform.rot = MathManager.ToFloat4(transform.rotation);
        entity.transform.fwd = MathManager.ToFloat3(transform.rotation * Vector3.forward);
    }

    private int p_animId;
    private int p_lastAnimationId = -1;
    private void AnimationUpdate(BaseEntity entity)
    {
        if (!gameObject.activeSelf)
            return;
        p_animId = entity.animation.animId;
        if(animator != null)
        {
            if(p_animId != 0 && p_lastAnimationId != p_animId)
            {
                var animations = entity.property.camp == ECamp.Alliance ? AnimationConstant.PlayerAnimationNames : AnimationConstant.EnemyAniamtionNames;
                animator.Play(animations[p_animId], 0f, entity.animation.loop);
                p_lastAnimationId = p_animId;
            }
            else if(p_animId == 0)
            {
                animator.Stop();
            }
            entity.animation.normalizedTime = animator.NormalizedTime;
        }
    }

}
