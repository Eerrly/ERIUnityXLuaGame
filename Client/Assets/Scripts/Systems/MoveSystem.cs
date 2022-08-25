
[EntitySystem]
public static class MoveSystem
{

    public static void UpdatePosition(BaseEntity entity)
    {
        float[] _position = MathManager.Vector3Zero;
        if(!KeySystem.IsYawTypeStop(entity.input.yaw))
        {
            var vector = MathManager.FromYawToVector3(entity.input.yaw);
            _position[0] = vector.x * entity.movement.moveSpeed;
            _position[1] = 0.0f;
            _position[2] = vector.z * entity.movement.moveSpeed;
        }
        entity.movement.position = _position;
    }

    public static void UpdateRotaion(BaseEntity entity)
    {
        float[] _rotation = MathManager.QuaternionIdentity;
        if(!KeySystem.IsYawTypeStop(entity.input.yaw))
        {
            var quaternion = MathManager.FromYaw(entity.input.yaw);
            _rotation[0] = 0.0f;
            _rotation[1] = quaternion.y;
            _rotation[2] = 0.0f;
            _rotation[3] = quaternion.w;
        }
        entity.movement.rotation = _rotation;
    }
}
