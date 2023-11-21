using System;

[EntitySystem]
public class RandomSystem
{
    private static System.Random _random;

    [EntitySystem.Initialize]
    private static void Initialize()
    {
        _random = new System.Random();
    }

    public static float[] RandomUintCircle(int min, int max)
    {
        var vector = new float[3];
        vector[0] = _random.Next(min, max);
        vector[1] = 0;
        vector[2] = _random.Next(min, max);
        var cellIndex = SpacePartition.PositionIntoIndex(vector);
        if(cellIndex < 0 || cellIndex > SpacePartition.cellList.Count)
        {
            return RandomUintCircle(min, max);
        }
        return vector;
    }

}
