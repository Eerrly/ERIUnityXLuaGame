[EntitySystem]
public class RandomSystem
{
    private static int Random(ref uint seed)
    {
        return Util.Random(ref seed);
    }

    public static int Random(ref uint seed, int min, int max)
    {
        if (min == max)
        {
            return min;
        }

        if (min > max)
        {
            var t = max;
            max = min;
            min = t;
        }

        var result = Random(ref seed);
        return (result % (max - min)) + min;
    }

    public static float Random(ref uint seed, float min, float max)
    {
        if (min == max)
        {
            return min;
        }
        if (min > max)
        {
            var temp = max;
            max = min;
            min = temp;
        }
        int rand = Random(ref seed);
        if (rand < 0)
        {
            rand = -rand;
        }
        float p = (float)rand / int.MaxValue;
        return min + (max - min) * p;
    }

}
