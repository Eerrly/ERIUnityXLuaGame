/// <summary>
/// 碰撞组件
/// </summary>
public class CollisionComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public FixedNumber collisionSize;
        public System.Int32 collisionDir;

        public Common(int no)
        {
            collisionSize = default(FixedNumber);
            collisionDir = default(System.Int32);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 碰撞半径
    /// </summary>
    public FixedNumber collisionSize
    {
        get => common.collisionSize;
        set => common.collisionSize = value;
    }

    /// <summary>
    /// 碰撞方向
    /// </summary>
    public System.Int32 collisionDir
    {
        get => common.collisionDir;
        set => common.collisionDir = value;
    }

}
