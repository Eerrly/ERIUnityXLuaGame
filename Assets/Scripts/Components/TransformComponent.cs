/// <summary>
/// 位置组件
/// </summary>
public class TransformComponent : BaseComponent
{
    
    internal class Common
    {

        public float[] pos;
        public float[] rot;

        public Common()
        {
            pos = new float[3];
            rot = new float[4];
        }

    }


    private Common common = new Common();

    /// <summary>
    /// 坐标
    /// </summary>
    public float[] pos => common.pos;

    /// <summary>
    /// 旋转
    /// </summary>
    public float[] rot => common.rot;

}
