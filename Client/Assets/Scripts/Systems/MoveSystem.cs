
[EntitySystem]
public static class MoveSystem
{

    public static void UpdatePosition(PlayerEntity playerEntity)
    {
        float[] _position = MathManager.Vector3Zero;
        if(!KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            var vector = MathManager.FromYawToVector3(playerEntity.input.yaw);
            _position[0] = vector.x * playerEntity.movement.moveSpeed;
            _position[1] = 0.0f;
            _position[2] = vector.z * playerEntity.movement.moveSpeed;
        }
        playerEntity.movement.position = _position;
    }

    public static void UpdateRotaion(PlayerEntity playerEntity)
    {
        float[] _rotation = MathManager.QuaternionIdentity;
        if(!KeySystem.IsYawTypeStop(playerEntity.input.yaw))
        {
            var quaternion = MathManager.FromYaw(playerEntity.input.yaw);
            _rotation[0] = 0.0f;
            _rotation[1] = quaternion.y;
            _rotation[2] = 0.0f;
            _rotation[3] = quaternion.w;
        }
        playerEntity.movement.rotation = _rotation;
    }
}
