
/// <summary>
/// 移动组件
/// </summary>
public class MoveComponent : BaseComponent
{

    internal class Common
    {

        public float[] position;
        public float[] rotation;
        public float moveSpeed;
        public float turnSpeed;

        public Common()
        {
            position = new float[3];
            rotation = new float[4];
            moveSpeed = default(float);
            turnSpeed = default(float);
        }

    }


    private Common common = new Common();

    /// <summary>
    /// 目标位置
    /// </summary>
    public float[] position => common.position;

    /// <summary>
    /// 目标方向
    /// </summary>
    public float[] rotation => common.rotation;

    /// <summary>
    /// 移动速度
    /// </summary>
    public float moveSpeed => common.moveSpeed;

    /// <summary>
    /// 转向速度
    /// </summary>
    public float turnSpeed => common.turnSpeed;

}
