
/// <summary>
/// 移动组件
/// </summary>
public class MoveComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float[] moveDirection;
        public float moveSpeed;
        public float turnSpeed;

        public Common(int no)
        {
            moveDirection = new float[3] { 0, 0, 0 };
            moveSpeed = default(float);
            turnSpeed = default(float);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 移动方向
    /// </summary>
    public float[] moveDirection
    {
        get { return common.moveDirection; }
        set { common.moveDirection = value; }
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    public float moveSpeed
    {
        get { return common.moveSpeed; }
        set { common.moveSpeed = value; }
    }

    /// <summary>
    /// 转向速度
    /// </summary>
    public float turnSpeed
    {
        get { return common.turnSpeed; }
        set { common.turnSpeed = value; }
    }

}
