using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetConstant
{
    /// <summary>
    /// 心跳
    /// </summary>
    public static byte PingAct = 1;

    /// <summary>
    /// 每帧操作
    /// </summary>
    public static byte FrameAct = 2;

    /// <summary>
    /// 准备
    /// </summary>
    public static byte ReadyAct = 3;
}
