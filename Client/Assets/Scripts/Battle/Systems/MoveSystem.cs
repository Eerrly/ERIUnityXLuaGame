
[EntitySystem]
public static class MoveSystem
{

    public static void UpdatePosition(BaseEntity entity)
    {
        var position = FixedVector3.Zero;
        if(!KeySystem.IsYawTypeStop(entity.Input.yaw))
        {
            var vector = FixedMath.FromYawToVector3(entity.Input.yaw);
            position = (vector * entity.Movement.moveSpeed).YZero();
        }
        entity.Movement.position = position;
    }

    public static void UpdateRotation(BaseEntity entity)
    {
        var rotation = FixedQuaternion.Identity;
        if(!KeySystem.IsYawTypeStop(entity.Input.yaw))
        {
            rotation = FixedMath.FromYaw(entity.Input.yaw);
        }
        entity.Movement.rotation = rotation;
    }
}
