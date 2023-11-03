
[EntitySystem]
public static class MoveSystem
{

    public static void UpdatePosition(BaseEntity entity)
    {
        FixedVector3 _position = FixedVector3.Zero;
        if(!KeySystem.IsYawTypeStop(entity.input.yaw))
        {
            var vector = FixedMath.FromYawToVector3(entity.input.yaw);
            _position = (vector * entity.movement.moveSpeed).YZero();
        }
        entity.movement.position = _position;
    }

    public static void UpdateRotaion(BaseEntity entity)
    {
        FixedQuaternion _rotation = FixedQuaternion.Identity;
        if(!KeySystem.IsYawTypeStop(entity.input.yaw))
        {
            _rotation = FixedMath.FromYaw(entity.input.yaw);
        }
        entity.movement.rotation = _rotation;
    }
}
