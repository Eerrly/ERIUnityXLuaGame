/// <summary>
/// 碰撞组件
/// </summary>
public class CollisionComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public FixedNumber collsionSize;
        public System.Int32 collisionDir;

        public Common(int no)
        {
            collsionSize = default(FixedNumber);
            collisionDir = default(System.Int32);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 碰撞半径
    /// </summary>
    public FixedNumber collsionSize
    {
        get { return common.collsionSize; }
        set { common.collsionSize = value; }
    }

    /// <summary>
    /// 碰撞方向
    /// </summary>
    public System.Int32 collisionDir
    {
        get { return common.collisionDir; }
        set { common.collisionDir = value; }
    }

}
