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

    public static void ChangePlayerAnimation(PlayerEntity playerEntity, EAnimationID animId)
    {
        ChangePlayerAnimation(playerEntity, (int)animId);
    }

    public static void ChangePlayerAnimation(PlayerEntity playerEntity, int nAnimId)
    {
        if (nAnimId > minAnimId && nAnimId < maxAnimId)
        {
            playerEntity.animation.animId = nAnimId;
        }
    }

    public static bool CheckAnimationNormalizedTimeDone(PlayerEntity playerEntity)
    {
        return playerEntity.animation.normalizedTime >= 0.95f;
    }

}
