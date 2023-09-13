using UnityEngine;

public static class MathManager
{
    /// <summary>
    /// 圆的角度最大值 360
    /// </summary>
    public static readonly FixedNumber AngleMax = FixedNumber.MakeFixNum(360, 1);

    /// <summary>
    /// 半圆角度最大值 180
    /// </summary>
    public static readonly FixedNumber HalfAngleMax = AngleMax / 2;

    /// <summary>
    /// 表达移动向量角度计算的偏移值,PlayerEntity的InputComponent组件中的yaw值为FrameBuffer.Input中的yaw减去YawOffset
    /// </summary>
    public const int YawOffset = 1;

    /// <summary>
    /// 默认摇杆
    /// </summary>
    public const int YawStop = -YawOffset;

    /// <summary>
    /// 移动向量夹角度数
    /// </summary>
    public static readonly FixedNumber DivAngle = FixedNumber.MakeFixNum(450000, 10000);

    /// <summary>
    /// 移动向量夹角度数的一半
    /// </summary>
    public static readonly FixedNumber HalfDivAngle = DivAngle / 2;

    /// <summary>
    /// 通过输入
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int Format8DirInput(FixedVector3 input)
    {
        input.y = FixedNumber.Zero;
        if(input.sqrMagnitudeLong > 0)
        {
            FixedVector3 dir = input.Normalized;
            FixedNumber angle = SignedAngle(FixedVector3.Forward, dir, FixedVector3.Up);

            angle = angle < 0 ? AngleMax + angle : angle;
            int div;
            if (angle <= HalfDivAngle || angle > AngleMax - HalfDivAngle)
            {
                div = 0;
            }
            else
            {
                var val = (angle - HalfDivAngle) / DivAngle;
                val += 1;
                div = val.ToInt();
            }
            return div + YawOffset;
        }
        return 0;
    }

    /// <summary>
    /// 通过摇杆获取对应的旋转
    /// </summary>
    /// <param name="yaw">摇杆</param>
    /// <returns>旋转四元数</returns>
    public static FixedQuaternion FromYaw(int yaw)
    {
        FixedQuaternion r = FixedQuaternion.Euler(FixedNumber.Zero, (yaw) * DivAngle, FixedNumber.Zero);
        return r;
    }

    /// <summary>
    /// 缓存摇杆对应的单位方向向量
    /// </summary>
    private static FixedVector3[] _cacheYawToVector3 = new FixedVector3[8];
    /// <summary>
    /// 通过摇杆获取单位方向向量
    /// </summary>
    /// <param name="yaw">摇杆</param>
    /// <returns>单位方向向量</returns>
    private static FixedVector3 _FromYawToVector3(int yaw)
    {
        switch (yaw)
        {
            case 0: return new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, FixedNumber.One);
            case 2: return new FixedVector3(FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);
            case 4: return new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, -FixedNumber.One);
            case 6: return new FixedVector3(-FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);
        }
        FixedQuaternion rot = FromYaw(yaw);
        return rot * FixedVector3.Forward;
    }

    /// <summary>
    /// 通过摇杆获取对应的单位方向向量
    /// </summary>
    /// <param name="yaw">摇杆</param>
    /// <returns>单位方向向量</returns>
    public static FixedVector3 FromYawToVector3(int yaw)
    {
        if (yaw < 0)
        {
            return FixedVector3.Zero;
        }
        yaw = yaw % 8;
        if (_cacheYawToVector3[yaw] == FixedVector3.Zero)
        {
            _cacheYawToVector3[yaw] = _FromYawToVector3(yaw);
        }
        return _cacheYawToVector3[yaw];
    }

    /// <summary>
    /// 获取两个向量的夹角，带正负
    /// </summary>
    /// <param name="lhs">向量</param>
    /// <param name="rhs">向量</param>
    /// <param name="axis">旋转轴</param>
    /// <returns>夹角</returns>
    public static FixedNumber SignedAngle(FixedVector3 lhs, FixedVector3 rhs, FixedVector3 axis)
    {
        FixedNumber num = FixedVector3.AngleInt(lhs, rhs);
        FixedVector3 rotateAxis = FixedVector3.Cross(lhs, rhs).Normalized;
        int num2 = Sign(FixedVector3.Dot(axis, rotateAxis, true));
        return (num * num2);
    }

    public static int Sign(FixedNumber value)
    {
        return value >= 0 ? 1 : -1;
    }

}
