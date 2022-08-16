
public class MoveComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float[] position;
        public float[] rotation;
        public float moveSpeed;
        public float turnSpeed;

        public Common(int no)
        {
            position = new float[3] { 0, 0, 0 };
            rotation = new float[4] { 0, 0, 0, 1 };
            moveSpeed = default(float);
            turnSpeed = default(float);
        }
    }

    private Common common = new Common(0);

    public float[] position
    {
        get { return common.position; }
        set { common.position = value; }
    }

    public float[] rotation
    {
        get { return common.rotation; }
        set { common.rotation = value; }
    }

    public float moveSpeed
    {
        get { return common.moveSpeed; }
        set { common.moveSpeed = value; }
    }

    public float turnSpeed
    {
        get { return common.turnSpeed; }
        set { common.turnSpeed = value; }
    }

}
