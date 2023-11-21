
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    /// <summary>
    /// 玩家实体ID
    /// </summary>
    public int entityId { get; private set; }

    public async void Init(BattlePlayerCommonData data)
    {
        entityId = data.pos;
        await CoLoadCharacter();
    }

    /// <summary>
    /// 加载角色
    /// </summary>
    /// <returns></returns>
    private async UniTask<GameObject> CoLoadCharacter()
    {
        var character = await Resources.LoadAsync<GameObject>(BattleConstant.PlayerCharacterPath) as GameObject;
        var go = Instantiate(character, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(transform, false);

        var renderers = go.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (var t in renderers)
        {
            t.material.color = BattleConstant.InitPlayerColor[entityId];
        }
        return go;
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
    /// <param name="battleEntity"></param>
    /// <param name="deltaTime"></param>
    public void RenderUpdate(BattleEntity battleEntity, float deltaTime)
    {
        var entity = battleEntity.FindEntity(entityId);
        TransformUpdate(entity, deltaTime);
#if UNITY_DEBUG
        SpacePartition.UpdateEntityCell(entity);
#endif
    }

    /// <summary>
    /// 位置轮询
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="deltaTime"></param>
    private void TransformUpdate(BaseEntity entity, float deltaTime)
    {
        if (!gameObject.activeSelf)
            return;

        var currentPosition = transform.position;
        var nextDeltaPosition = currentPosition + entity.Movement.position.ToVector3();
        if((currentPosition - nextDeltaPosition).sqrMagnitude >= 4)
        {
            currentPosition = Vector3.Lerp(currentPosition, nextDeltaPosition, deltaTime);
        }
        var currentRotation = transform.rotation;
        var nextDeltaRotation = entity.Movement.rotation.ToQuaternion();
        if(currentRotation != nextDeltaRotation)
        {
            currentRotation = Quaternion.RotateTowards(transform.rotation, nextDeltaRotation, entity.Movement.turnSpeed * deltaTime);
        }

        var transform1 = transform;
        transform1.position = currentPosition;
        transform1.rotation = currentRotation;

        entity.Transform.pos = new FixedVector3(currentPosition);
        entity.Transform.rot = new FixedQuaternion(currentRotation);
        entity.Transform.fwd = new FixedVector3(currentRotation * Vector3.forward);
    }

}
