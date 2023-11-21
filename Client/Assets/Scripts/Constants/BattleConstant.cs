using UnityEngine;

public class BattleConstant
{
    /// <summary>
    /// 自己ID
    /// </summary>
    public static int SelfID = 0;

    /// <summary>
    /// 一帧33ms
    /// </summary>
    public const int FrameInterval = 33;

    /// <summary>
    /// 最大客户端数量
    /// </summary>
    public const int MaxClientCount = 2;

    /// <summary>
    /// 心跳间隔帧数
    /// </summary>
    public const int HeartBeatFrame = 100;

    /// <summary>
    /// 玩家Prefab
    /// </summary>
    public const string PlayerCharacterPath = "Prefabs/Cube";

    /// <summary>
    /// 初始化玩家位置
    /// </summary>
    public static readonly Vector3[] InitPlayerPos = { new Vector3(-3, 0, 30), new Vector3(13, 0, -5) };

    /// <summary>
    /// 初始化玩家旋转
    /// </summary>
    public static readonly Quaternion[] InitPlayerRot = { new Quaternion(0, -180, 0, 0), new Quaternion(0, 0, 0, 0) };

    /// <summary>
    /// 初始化玩家颜色
    /// </summary>
    public static readonly Color[] InitPlayerColor = { new Color((float)42/255, (float)100 /255, (float)178 /255), new Color((float)229/255, (float)46 /255, (float)40 /255) };

    /// <summary>
    /// 键位名称
    /// </summary>
    public static readonly string[] ButtonNames = new string[] { "j", "k", "l" };

    /// <summary>
    /// 随机种子
    /// </summary>
    public const uint RandomSeed = 114514;

    /// <summary>
    /// 格子宽
    /// </summary>
    public const float SpaceX = 100f;

    /// <summary>
    /// 格子长
    /// </summary>
    public const float SpaceZ = 100f;

    /// <summary>
    /// 角度
    /// </summary>
    public static readonly FixedNumber Angle = FixedNumber.MakeFixNum(90 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    /// <summary>
    /// 格子长宽
    /// </summary>
    public static readonly Vector3 CellSize = new Vector3(10f, 0, 10f);

}