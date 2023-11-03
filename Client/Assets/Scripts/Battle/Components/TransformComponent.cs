/// <summary>
/// 位置组件
/// </summary>
public class TransformComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public FixedVector3 pos;
        public FixedQuaternion rot;
        public FixedVector3 fwd;

        public Common(int no)
        {
            pos = FixedVector3.Zero;
            rot = default(FixedQuaternion);
            fwd = default(FixedVector3);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// Position
    /// </summary>
    public FixedVector3 pos
    {
        get { return common.pos; }
        set { common.pos = value; }
    }

    /// <summary>
    /// Rotation
    /// </summary>
    public FixedQuaternion rot
    {
        get { return common.rot; }
        set { common.rot = value; }
    }

    /// <summary>
    /// Forward
    /// </summary>
    public FixedVector3 fwd
    {
        get { return common.fwd; }
        set { common.fwd = value; }
    }

}
