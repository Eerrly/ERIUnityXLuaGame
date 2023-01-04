public class TransformComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float[] pos;
        public float[] rot;
        public float[] fwd;

        public Common(int no)
        {
            pos = new float[3] { 0, 0, 0 };
            rot = new float[4] { 0, 0, 0, 1 };
            fwd = new float[3] { 0, 0, 1 };
        }
    }

    private Common common = new Common(0);

    public float[] pos
    {
        get { return common.pos; }
        set { common.pos = value; }
    }

    public float[] rot
    {
        get { return common.rot; }
        set { common.rot = value; }
    }

    public float[] fwd
    {
        get { return common.fwd; }
        set { common.fwd = value; }
    }

}
