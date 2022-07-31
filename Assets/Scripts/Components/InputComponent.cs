public class InputComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int yaw;
        public int key;

        public Common(int no)
        {
            yaw = default(int);
            key = default(int);
        }
    }

    private Common common = new Common(0);

    public int yaw
    {
        get { return common.yaw; }
        set { common.yaw = value; }
    }

    public int key
    {
        get { return common.key; }
        set { common.key = value; }
    }

}
