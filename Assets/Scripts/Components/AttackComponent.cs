public class AttackComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public float cd;
        public float atk;
        public float lastAttackTime;
        public float attackDistance;
        public float displacementCompensation;

        public Common(int no)
        {
            cd = default(float);
            atk = default(float);
            lastAttackTime = default(float);
            attackDistance = default(float);
            displacementCompensation = default(float);
        }
    }

    private Common common = new Common(0);

    public float cd
    {
        get { return common.cd; }
        set { common.cd = value; }
    }

    public float atk
    {
        get { return common.atk; }
        set { common.atk = value; }
    }

    public float lastAttackTime
    {
        get { return common.lastAttackTime; }
        set { common.lastAttackTime = value; }
    }

    public float attackDistance
    {
        get { return common.attackDistance; }
        set { common.attackDistance = value; }
    }

    public float displacementCompensation
    {
        get { return common.displacementCompensation; }
        set { common.displacementCompensation = value; }
    }

}
