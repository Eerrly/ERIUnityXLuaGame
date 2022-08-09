using UnityEngine;

[EntitySystem]
public class RandomSystem
{
    private static void Initialize()
    {
    }

    public static float RandomRange(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static float[] RandomUintCircle()
    {
        return MathManager.ToFloat2(Random.insideUnitCircle);
    }

}
