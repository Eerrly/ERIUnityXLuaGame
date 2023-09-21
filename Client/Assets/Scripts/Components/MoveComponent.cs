
/// <summary>
/// 移动组件
/// </summary>
public class MoveComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public FixedVector3 position;
        public FixedQuaternion rotation;
        public FixedNumber moveSpeed;
        public float turnSpeed;

        public Common(int no)
        {
            position = default(FixedVector3);
            rotation = default(FixedQuaternion);
            moveSpeed = default(FixedNumber);
            turnSpeed = default(float);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 位移的向量
    /// </summary>
    public FixedVector3 position
    {
        get { return common.position; }
        set { common.position = value; }
    }

    /// <summary>
    /// 唯一的旋转
    /// </summary>
    public FixedQuaternion rotation
    {
        get { return common.rotation; }
        set { common.rotation = value; }
    }

    /// <summary>
    /// 唯一的速度
    /// </summary>
    public FixedNumber moveSpeed
    {
        get { return common.moveSpeed; }
        set { common.moveSpeed = value; }
    }

    /// <summary>
    /// 转向的速度
    /// </summary>
    public float turnSpeed
    {
        get { return common.turnSpeed; }
        set { common.turnSpeed = value; }
    }

}
