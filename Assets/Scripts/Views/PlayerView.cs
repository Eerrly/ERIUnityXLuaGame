
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public int playerId { get; set; }

    public void Init()
    {

    }

    public void RenderUpdate(BattleEntity battleEntity, int playerId, float deltaTime)
    {
        PlayerEntity playerEntity = battleEntity.FindPlayer(playerId);
        if (gameObject.activeSelf)
        {
            transform.position = ToVector3(playerEntity.transform.pos);
            transform.rotation = ToQuaternion(playerEntity.transform.rot);
        }
    }

    public Vector3 ToVector3(float[] pos)
    {
        return new Vector3(pos[0], pos[1], pos[2]);
    }

    public Quaternion ToQuaternion(float[] rot)
    {
        return new Quaternion(rot[0], rot[1], rot[2], rot[3]);
    }

}
