
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

    public void RenderUpdate(BattleEntity battleEntity, float deltaTime)
    {
        BaseEntity entity = battleEntity.FindEntity(entityId);
        TransformUpdate(entity, deltaTime);
#if UNITY_DEBUG
        SpacePartition.UpdateEntityCell(entity);
#endif
    }

    private void TransformUpdate(BaseEntity entity, float deltaTime)
    {
        if (!gameObject.activeSelf)
            return;

        var currentPosition = transform.position;
        var nextDetlaPosition = currentPosition + entity.movement.position.ToVector3();
        if((currentPosition - nextDetlaPosition).sqrMagnitude >= 4)
        {
            currentPosition = Vector3.Lerp(currentPosition, nextDetlaPosition, deltaTime);
        }
        var currentRotation = transform.rotation;
        var nextDetlaRotation = entity.movement.rotation.ToQuaternion();
        if(currentRotation != nextDetlaRotation)
        {
            currentRotation = Quaternion.RotateTowards(transform.rotation, nextDetlaRotation, entity.movement.turnSpeed * deltaTime);
        }

        transform.position = currentPosition;
        transform.rotation = currentRotation;

        entity.transform.pos = new FixedVector3(currentPosition);
        entity.transform.rot = new FixedQuaternion(currentRotation);
        entity.transform.fwd = new FixedVector3(currentRotation * Vector3.forward);
    }

}
