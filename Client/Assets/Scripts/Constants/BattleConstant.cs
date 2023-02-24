using UnityEngine;

public class BattleConstant
{
    public static readonly int SelfID = 0;

    public static readonly int FrameInterval = 30;

    public static readonly float normalPlayerPositionOffset = 10f;

    public static readonly string playerCharacterPath = "Prefabs/Cube";

    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    public static readonly uint randomSeed = 114514;

    public static readonly float spaceX = 100f;

    public static readonly float spaceZ = 100f;

    public static readonly int angle = 90;

    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

    public static readonly float[] hpStates = new float[] { 0, 30.0f, 60.0f, 100 };

}