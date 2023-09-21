using UnityEngine;

public class BattleConstant
{
    /// <summary>
    /// 自己的ID
    /// </summary>
    public static int SelfID = 0;

    /// <summary>
    /// 帧间隔(毫秒)
    /// </summary>
    public static readonly int FrameInterval = 33;

    /// <summary>
    /// 最大客户端数量
    /// </summary>
    public static readonly int MaxClientCount = 2;

    /// <summary>
    /// 心跳间隔(帧)
    /// </summary>
    public static readonly int HeartBeatFrame = 100;

    /// <summary>
    /// 玩家Prefab
    /// </summary>
    public static readonly string playerCharacterPath = "Prefabs/Cube";

    /// <summary>
    /// 玩家生成位置
    /// </summary>
    public static readonly Vector3[] InitPlayerPos = { new Vector3(-3, 0, 30), new Vector3(13, 0, -5) };

    /// <summary>
    /// 玩家生成旋转
    /// </summary>
    public static readonly Quaternion[] InitPlayerRot = { new Quaternion(0, -180, 0, 0), new Quaternion(0, 0, 0, 0) };

    /// <summary>
    /// 玩家生成颜色
    /// </summary>
    public static readonly Color[] InitPlayerColor = { new Color((float)42/255, (float)100 /255, (float)178 /255), new Color((float)229/255, (float)46 /255, (float)40 /255) };

    /// <summary>
    /// 键位
    /// </summary>
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    /// <summary>
    /// 随机种子
    /// </summary>
    public static readonly uint randomSeed = 114514;

    /// <summary>
    /// 格子的宽
    /// </summary>
    public static readonly float spaceX = 100f;

    /// <summary>
    /// 格子的长
    /// </summary>
    public static readonly float spaceZ = 100f;

    /// <summary>
    /// 面向角度
    /// </summary>
    public static readonly FixedNumber angle = FixedNumber.MakeFixNum(90 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    /// <summary>
    /// 格子尺寸
    /// </summary>
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

}