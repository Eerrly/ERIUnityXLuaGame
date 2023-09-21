/// <summary>
/// 属性组件
/// </summary>
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

    /// <summary>
    /// 玩家ID
    /// </summary>
    public int pos
    {
        get { return common.pos; }
        set { common.pos = value; }
    }

}
