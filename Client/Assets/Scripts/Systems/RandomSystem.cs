using System;

[EntitySystem]
public class RandomSystem
{
    private static System.Random random;

    [EntitySystem.Initialize]
    private static void Initialize()
    {
        random = new System.Random();
    }

    public static float[] RandomUintCircle(int min, int max)
    {
        float[] vector = new float[3];
        vector[0] = random.Next(min, max);
        vector[1] = 0;
        vector[2] = random.Next(min, max);
        int cellIndex = SpacePartition.PositionIntoIndex(vector);
        if(cellIndex < 0 || cellIndex > SpacePartition.cellList.Count)
        {
            return RandomUintCircle(min, max);
        }
        return vector;
    }

}
