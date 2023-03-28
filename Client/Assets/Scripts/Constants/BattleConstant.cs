using UnityEngine;

public class BattleConstant
{
    public static int SelfID = 0;

    // 帧间隔
    public static readonly int FrameInterval = 30;

    // 玩家生成位置间距
    public static readonly float normalPlayerPositionOffset = 10f;

    // 玩家Prefab
    public static readonly string playerCharacterPath = "Prefabs/Cube";

    // 键位
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    // 随机种子
    public static readonly uint randomSeed = 114514;

    // 格子的宽
    public static readonly float spaceX = 100f;

    // 格子的高
    public static readonly float spaceZ = 100f;

    // 面向角度
    public static readonly int angle = 90;

    // 格子尺寸
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

    public static readonly float[] hpStates = new float[] { 0, 30.0f, 60.0f, 100 };

}