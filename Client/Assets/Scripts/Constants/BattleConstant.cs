using UnityEngine;

public class BattleConstant
{
    public static int SelfID = 0;

    // 帧间隔
    public static readonly int FrameInterval = 30;

    // 玩家Prefab
    public static readonly string playerCharacterPath = "Prefabs/CompleteTank";
    // 玩家生成位置
    public static readonly Vector3[] InitPlayerPos = { new Vector3(-3, 0, 30), new Vector3(13, 0, -5) };
    // 玩家生成旋转
    public static readonly Quaternion[] InitPlayerRot = { new Quaternion(0, -180, 0, 0), new Quaternion(0, 0, 0, 0) };
    // 玩家生成颜色
    public static readonly Color[] InitPlayerColor = { new Color((float)42/255, (float)100 /255, (float)178 /255), new Color((float)229/255, (float)46 /255, (float)40 /255) };

    // 键位
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    // 随机种子
    public static readonly uint randomSeed = 114514;

    // 格子的宽
    public static readonly float spaceX = 100f;

    // 格子的高
    public static readonly float spaceZ = 100f;

    // 面向角度
    public static readonly FixedNumber angle = FixedNumber.MakeFixNum(90 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    // 格子尺寸
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

}