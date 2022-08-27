public class CollisionComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float collsionSize;
        public int collisionDir;

        public Common(int no)
        {
            collsionSize = default(float);
            collisionDir = default(int);
        }
    }

    private Common common = new Common(0);

    public float collsionSize
    {
        get { return common.collsionSize; }
        set { common.collsionSize = value; }
    }

    public int collisionDir
    {
        get { return common.collisionDir; }
        set { common.collisionDir = value; }
    }

}
