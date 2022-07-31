/// <summary>
/// ¶¯»­×é¼þ
/// </summary>
public class AnimationComponent : BaseComponent
{

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public EAnimationID animId;
        public float fixedTransitionDuration;
        public int layer;
        public float fixedTimeOffset;
        public float normalizedTransitionTime;

        public Common(int no)
        {
            animId = default(EAnimationID);
            fixedTransitionDuration = default(float);
            layer = default(int);
            fixedTimeOffset = default(float);
            normalizedTransitionTime = default(float);
        }
    }

    private Common common = new Common(0);

    public EAnimationID animId
    {
        get { return common.animId; }
        set { common.animId = value; }
    }

    public float fixedTransitionDuration
    {
        get { return common.fixedTransitionDuration; }
        set { common.fixedTransitionDuration = value; }
    }

    public int layer
    {
        get { return common.layer; }
        set { common.layer = value; }
    }

    public float fixedTimeOffset
    {
        get { return common.fixedTimeOffset; }
        set { common.fixedTimeOffset = value; }
    }

    public float normalizedTransitionTime
    {
        get { return common.normalizedTransitionTime; }
        set { common.normalizedTransitionTime = value; }
    }


}
