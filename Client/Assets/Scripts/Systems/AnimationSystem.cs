[EntitySystem]
public class AnimationSystem
{

    private static int minAnimId;
    private static int maxAnimId;

    [EntitySystem.Initialize]
    private static void Initialize()
    {
        minAnimId = (int)EAnimationID.None;
        maxAnimId = (int)EAnimationID.Count;
    }

    public static void ChangePlayerAnimation(BaseEntity entity, EAnimationID animId)
    {
        ChangePlayerAnimation(entity, (int)animId);
    }

    public static void ChangePlayerAnimation(BaseEntity entity, int nAnimId)
    {
        if (nAnimId > minAnimId && nAnimId < maxAnimId)
        {
            entity.animation.animId = nAnimId;
        }
    }

    public static void EnableAnimator(BaseEntity entity, bool enable)
    {
        entity.animation.enable = enable;
    }

    public static bool CheckAnimationNormalizedTime(BaseEntity entity, float normalizedTime = 0.84f)
    {
        return entity.animation.normalizedTime >= normalizedTime;
    }

}
