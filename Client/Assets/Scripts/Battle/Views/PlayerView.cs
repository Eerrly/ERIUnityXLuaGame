
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private int _entityId;
    public int entityId => _entityId;

    public async void Init(BattlePlayerCommonData data)
    {
        _entityId = data.pos;
        await CoLoadCharacter();
    }

    /// <summary>
    /// 加载角色
    /// </summary>
    /// <returns></returns>
    public async UniTask<GameObject> CoLoadCharacter()
    {
        GameObject character = await Resources.LoadAsync<GameObject>(BattleConstant.playerCharacterPath) as GameObject;
        GameObject go = Instantiate(character, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(transform, false);

        var renderers = go.transform.GetComponentsInChildren<MeshRenderer>();
        for (int j = 0; j < renderers.Length; j++)
            renderers[j].material.color = BattleConstant.InitPlayerColor[_entityId];
        return go;
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
    /// <param name="battleEntity"></param>
    /// <param name="deltaTime"></param>
    public void RenderUpdate(BattleEntity battleEntity, float deltaTime)
    {
        BaseEntity entity = battleEntity.FindEntity(entityId);
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
        var nextDetlaPosition = currentPosition + entity.Movement.position.ToVector3();
        if((currentPosition - nextDetlaPosition).sqrMagnitude >= 4)
        {
            currentPosition = Vector3.Lerp(currentPosition, nextDetlaPosition, deltaTime);
        }
        var currentRotation = transform.rotation;
        var nextDetlaRotation = entity.Movement.rotation.ToQuaternion();
        if(currentRotation != nextDetlaRotation)
        {
            currentRotation = Quaternion.RotateTowards(transform.rotation, nextDetlaRotation, entity.Movement.turnSpeed * deltaTime);
        }

        transform.position = currentPosition;
        transform.rotation = currentRotation;

        entity.Transform.pos = new FixedVector3(currentPosition);
        entity.Transform.rot = new FixedQuaternion(currentRotation);
        entity.Transform.fwd = new FixedVector3(currentRotation * Vector3.forward);
    }

}
