using System;

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

    public FixedNumber collsionSize
    {
        get { return common.collsionSize; }
        set { common.collsionSize = value; }
    }

    public System.Int32 collisionDir
    {
        get { return common.collisionDir; }
        set { common.collisionDir = value; }
    }

}
