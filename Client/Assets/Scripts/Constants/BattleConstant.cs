using UnityEngine;

public class BattleConstant
{
    public static readonly int MyCamp = 1;

    public static readonly int FrameInterval = 30;

    public static readonly float normalPlayerPositionOffset = 40f;

    public static readonly string playerCharacterPath = "Prefabs/Player";

    public static readonly string enemyCharacterPath = "Prefabs/Enemy";

    public static readonly string[] buttonNames = new string[] { "tab", "j", "k", "l" };

    public static readonly int randomSeed = 114514;

    public static readonly float spaceX = 100f;

    public static readonly float spaceZ = 100f;

    public static readonly int angle = 90;

    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

    public static readonly float[] hpStates = new float[] { 0, 30.0f, 60.0f, 100 };
}