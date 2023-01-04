public class PropertyComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int pos;

        public Common(int no)
        {
            pos = default(int);
        }
    }

    private Common common = new Common(0);

    public int pos
    {
        get { return common.pos; }
        set { common.pos = value; }
    }

}
