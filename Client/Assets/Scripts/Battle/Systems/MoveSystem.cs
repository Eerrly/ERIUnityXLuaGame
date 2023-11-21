
[EntitySystem]
public static class MoveSystem
{

    public static void UpdatePosition(BaseEntity entity)
    {
        FixedVector3 _position = FixedVector3.Zero;
        if(!KeySystem.IsYawTypeStop(entity.Input.yaw))
        {
            var vector = FixedMath.FromYawToVector3(entity.Input.yaw);
            _position = (vector * entity.Movement.moveSpeed).YZero();
        }
        entity.Movement.position = _position;
    }

    public static void UpdateRotaion(BaseEntity entity)
    {
        FixedQuaternion _rotation = FixedQuaternion.Identity;
        if(!KeySystem.IsYawTypeStop(entity.Input.yaw))
        {
            _rotation = FixedMath.FromYaw(entity.Input.yaw);
        }
        entity.Movement.rotation = _rotation;
    }
}
