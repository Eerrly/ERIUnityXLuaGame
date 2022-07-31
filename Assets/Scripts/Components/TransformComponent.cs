/// <summary>
/// 位置组件
/// </summary>
public class TransformComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float[] pos;
        public float[] rot;

        public Common(int no)
        {
            pos = new float[3] { 0, 0, 0 };
            rot = new float[4] { 0, 0, 0, 1 };
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// 坐标
    /// </summary>
    public float[] pos
    {
        get { return common.pos; }
        set { common.pos = value; }
    }

    /// <summary>
    /// 旋转
    /// </summary>
    public float[] rot
    {
        get { return common.rot; }
        set { common.rot = value; }
    }

}
