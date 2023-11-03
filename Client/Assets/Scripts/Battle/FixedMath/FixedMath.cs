public class FixedMath
{
    public static readonly int DataConrvertScale = 10000;
    public static readonly FixedNumber Pi = FixedNumber.MakeFixNum(31416, 10000);
    public static readonly FixedNumber HalPi = Pi / 2;
    public static readonly FixedNumber twoPi = Pi * 2;
    public static readonly FixedNumber AngleMax = FixedNumber.MakeFixNum(360, 1);
    public static readonly FixedNumber HalfAngleMax = AngleMax / 2;
    public static readonly FixedNumber QuarterAngleMax = HalfAngleMax / 2;
    public const int YawOffset = 1;
    public const int YawStop = -YawOffset;
    public static readonly FixedNumber Deg2Rad = FixedNumber.MakeFixNum(175, 10000);
    public static readonly FixedNumber Rad2Deg = FixedNumber.MakeFixNum(572958, 10000);
    public static readonly FixedNumber DivAngle = FixedNumber.MakeFixNum(450000, 10000);
    public static readonly FixedNumber HalfDivAngle = FixedNumber.MakeFixNum(22500, 10000);
    public static readonly FixedNumber Log2Max = new FixedNumber(RAW_LOG2MAX);
    public static readonly FixedNumber Log2Min = new FixedNumber(RAW_LOG2MIN);
    public static readonly FixedNumber Ln2 = new FixedNumber(RAW_LN2);

    private const long RAW_LOG2MAX = 0x1F0000;
    private const long RAW_LOG2MIN = -0x200000;
    private const long RAW_LN2 = 0xB172;

    public static FixedNumber Atan(FixedNumber cosVal)
    {
        return Atan2(cosVal.numerator, FixedNumber.FRACTION_RANGE);
    }

    public static FixedNumber Atan2(long y, long x)
    {
        int num;
        int num2;
        if (x < 0)
        {
            if (y < 0)
            {
                //第三象限
                x = -x;
                y = -y;
                num = 1;
            }
            else
            {
                //第二象限
                x = -x;
                num = -1;
            }
            //-PI 乘以10000
            num2 = -31416;
        }
        else
        {
            if (y < 0)
            {
                //第四象限
                y = -y;
                num = -1;
            }
            else
            {
                //第一象限
                num = 1;
            }
            num2 = 0;
        }
        int dIM = FixedAtan2Table.DIM;   //2^7 = 128
        long num3 = (long)(dIM - 1);  //127
                                      //下边这段的意思是，把xy归一化后映射到0-127闭区间上，然后去查表
                                      //y做行，x做列去查询设置好的二维表
        long b = (long)((x >= y) ? x : y);
        int num4 = (int)FixedMath.Divide((long)x * num3, b);
        int num5 = (int)FixedMath.Divide((long)y * num3, b);
        int num6 = FixedAtan2Table.table[num5 * dIM + num4];
        return FixedNumber.MakeFixNum(((num6 + num2) * num), 10000);
    }

    public static FixedNumber Acos(FixedNumber cosVal)
    {
        //计算acos就比较简单了，因为cos的取值就是-1~1，不存在无穷的问题
        //如果把cos比作x/length,当length等于1的时候，x就是cos值，所以只需要把-1~1平均分成IntAcosTable.COUNT份
        //然后做一个-1~1与0~IntAcosTable.COUNT的映射即可，该函数的功能就是做这个映射 
        //由于cos函数与角度不是线性的，即两者的变化率不一样，所以多少有点值分配不均匀的问题，不过分成1024份，影响不大
        int num = (cosVal * FixedAcosTable.HALF_COUNT).ToInt() + FixedAcosTable.HALF_COUNT;
        num = FixedMath.Clamp(num, 0, FixedAcosTable.COUNT);
        return FixedNumber.MakeFixNum(FixedAcosTable.table[num], 10000);
    }

    /// <summary>
    /// 返回的是-pi/2 到 pi/2的弧度值
    /// </summary>
    /// <param name="nom"></param>
    /// <param name="den"></param>
    /// <returns></returns>
    public static FixedNumber Asin(FixedNumber sinVal)
    {
        int num = (sinVal * FixedAcosTable.HALF_COUNT).ToInt() + FixedAsinTable.HALF_COUNT;
        num = FixedMath.Clamp(num, 0, FixedAsinTable.COUNT);
        return FixedNumber.MakeFixNum(FixedAsinTable.table[num], 10000);
    }

    public static FixedNumber Sin(FixedNumber sinVal)
    {
        //索引值求的原理见对应函数内注释 
        int index = FixedSinCosTable.getIndex(sinVal.numerator, FixedNumber.FRACTION_RANGE);
        return FixedNumber.MakeFixNum(FixedSinCosTable.sin_table[index], 10000);
    }

    public static FixedNumber Cos(FixedNumber cosVal)
    {
        //索引值求的原理见对应函数内注释 
        int index = FixedSinCosTable.getIndex(cosVal.numerator, FixedNumber.FRACTION_RANGE);
        return FixedNumber.MakeFixNum(FixedSinCosTable.cos_table[index], 10000);
    }

    public static FixedNumber Tan(FixedNumber val)
    {
        FixedNumber sin = FixedMath.Sin(val);
        FixedNumber cos = FixedMath.Cos(val);
        if (cos == 0)
        {
            return FixedNumber.MaxValue;
        }
        return sin / cos;

    }

    public static FixedNumber cosDeg(FixedNumber deg)
    {
        var tmpValue = deg * Deg2Rad;
        return Cos(tmpValue);
    }

    public static void sincos(out FixedNumber s, out FixedNumber c, long nom, long den)
    {
        int index = FixedSinCosTable.getIndex(nom, den);
        s = FixedNumber.MakeFixNum(FixedSinCosTable.sin_table[index], 10000);
        c = FixedNumber.MakeFixNum(FixedSinCosTable.cos_table[index], 10000);
    }

    public static void sincos(out FixedNumber s, out FixedNumber c, FixedNumber angle)
    {
        int index = FixedSinCosTable.getIndex(angle.numerator, FixedNumber.FRACTION_RANGE);
        s = FixedNumber.MakeFixNum(FixedSinCosTable.sin_table[index], 10000);
        c = FixedNumber.MakeFixNum(FixedSinCosTable.cos_table[index], 10000);
    }

    public static long Divide(long a, long b)
    {
        if (a == 0)
        {
            return 0;
        }
        if (b == 0)
        {
            return long.MaxValue;
        }
        return a / b;
    }


    public static long Abs(long val)
    {
        if (val < 0)
            return -val;
        return val;
    }

    public static int Abs(int val)
    {
        if (val < 0)
            return -val;
        return val;
    }

    public static FixedNumber Abs(FixedNumber v)
    {
        return new FixedNumber(Abs(v.numerator));
    }

    public static uint Sqrt32(uint a)
    {
        //经典的逐位确认法
        uint num = 0u;
        uint num2 = 0u;
        for (int i = 0; i < 16; i++)
        {
            num2 <<= 1;
            num <<= 2;
            num += a >> 30;
            a <<= 2;
            if (num2 < num)
            {
                num2 += 1u;
                num -= num2;
                num2 += 1u;
            }
        }
        return num2 >> 1 & 65535u;
    }

    public static ulong Sqrt64(ulong a)
    {
        //经典的逐位确认法
        ulong num = 0uL;
        ulong num2 = 0uL;
        for (int i = 0; i < 32; i++)
        {
            num2 <<= 1;
            num <<= 2;
            num += a >> 62;
            a <<= 2;
            if (num2 < num)
            {
                num2 += 1uL;
                num -= num2;
                num2 += 1uL;
            }
        }
        return num2 >> 1 & unchecked((ulong)-1);
    }

    public static long SqrtLong(long a)
    {
        if (a <= 0L)
        {
            return 0L;
        }
        if (a <= unchecked((long)(unchecked((ulong)-1))))
        {
            return (long)((ulong)FixedMath.Sqrt32((uint)a));
        }
        return (long)FixedMath.Sqrt64((ulong)a);
    }

    public static int Sqrt(long a)
    {
        if (a <= 0L)
        {
            return 0;
        }
        if (a <= unchecked((long)(unchecked((ulong)-1))))
        {
            return (int)FixedMath.Sqrt32((uint)a);
        }
        return (int)FixedMath.Sqrt64((ulong)a);
    }

    public static FixedNumber Sqrt(FixedNumber a)
    {
        if (a < FixedNumber.Zero)
            return FixedNumber.Zero;
        long num = Sqrt(a.numerator << FixedNumber.FRACTIONAL_BITS);
        return new FixedNumber(num);
    }

    public static FixedNumber Clamp(FixedNumber a, FixedNumber min, FixedNumber max)
    {
        if (a < min)
        {
            return min;
        }
        if (a > max)
        {
            return max;
        }
        return a;

    }

    public static int Clamp(int a, int min, int max)
    {
        if (a < min)
        {
            return min;
        }
        if (a > max)
        {
            return max;
        }
        return a;
    }

    public static FixedNumber Lerp(FixedNumber a, FixedNumber b, FixedNumber c)
    {
        return (b - a) * c + a;
    }

    public static FixedNumber InverseLerp(FixedNumber a, FixedNumber b, FixedNumber c)
    {
        if (a == b)
            return FixedNumber.Zero;
        return Clamp((c - a) / (b - a), FixedNumber.Zero, FixedNumber.One);
    }

    public static FixedNumber Min(FixedNumber a, FixedNumber b)
    {
        return a < b ? a : b;
    }

    public static FixedNumber Max(FixedNumber a, FixedNumber b)
    {
        return a < b ? b : a;
    }

    public static int Max(int a, int b)
    {
        return a < b ? b : a;
    }

    public static int Min(int a, int b)
    {
        return a > b ? b : a;
    }

    public static int Sign(FixedNumber value)
    {
        return value >= 0 ? 1 : -1;
    }

    public static FixedNumber SignedAngleAxisY(FixedVector3 lhs, FixedVector3 rhs)
    {
        FixedNumber num = FixedVector3.AngleInt(lhs, rhs);
        FixedVector3 rotateAxis = FixedVector3.Cross(lhs, rhs).Normalized;
        int num2 = Sign(FixedVector3.Dot(FixedVector3.Up, rotateAxis, true));
        return (num * num2);
    }

    public static FixedNumber SignedAngle(FixedVector3 lhs, FixedVector3 rhs, FixedVector3 axis)
    {
        FixedNumber num = FixedVector3.AngleInt(lhs, rhs);
        FixedVector3 rotateAxis = FixedVector3.Cross(lhs, rhs).Normalized;
        int num2 = Sign(FixedVector3.Dot(axis, rotateAxis, true));
        return (num * num2);
    }

    public static FixedNumber GetYawAngle(int yaw)
    {
        return (yaw) * DivAngle;
    }

    public static FixedQuaternion FromYaw(int yaw)
    {
        FixedQuaternion r = FixedQuaternion.Euler(FixedNumber.Zero, (yaw) * DivAngle, FixedNumber.Zero);
        return r;
    }

    public static FixedQuaternion FromYAngle(FixedNumber angle)
    {
        FixedQuaternion r = FixedQuaternion.Euler(FixedNumber.Zero, angle, FixedNumber.Zero);
        return r;
    }

    private static FixedVector3[] _cacheYaToVector3 = new FixedVector3[8];
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

    public static FixedVector3 FromYawToVector3(int yaw)
    {
        if (yaw < 0)
        {
            return FixedVector3.Zero;
        }

        yaw = yaw % 8;
        if (_cacheYaToVector3[yaw] == FixedVector3.Zero)
        {
            _cacheYaToVector3[yaw] = _FromYawToVector3(yaw);
        }
        return _cacheYaToVector3[yaw];
    }

    public static FixedVector3 FromYAngleToVector3(FixedNumber angle)
    {
        FixedQuaternion r = FromYAngle(angle);
        return r * FixedVector3.Forward;
    }

    public static int Format8DirInput(FixedVector3 input)
    {
        input.y = FixedNumber.Zero;
        if (input.sqrMagnitudeLong > 0)
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
                FixedNumber val = (angle - HalfDivAngle) / DivAngle;
                val += 1;
                div = val.ToInt();
            }
            return div + YawOffset;
        }
        return 0;
    }

    public static FixedNumber Yaw(FixedVector3 vec)
    {
        return SignedAngle(FixedVector3.Forward, vec.YZero().Normalized, FixedVector3.Up);
    }

    public static FixedNumber MoveTowardsAngle(FixedNumber current, FixedNumber target, FixedNumber maxDelta)
    {
        current = ClampAngle0To360(current);
        target = ClampAngle0To360(target);
        FixedNumber deltaAngle = ClampAngle0To360(target - current);
        if (deltaAngle > HalfAngleMax)
        {
            deltaAngle -= AngleMax;
        }
        if (deltaAngle >= -maxDelta && deltaAngle <= maxDelta)
        {
            return target;
        }
        target = current + deltaAngle;
        return current + Sign(target - current) * maxDelta;
    }

    public static FixedNumber ClampAngle0To360(FixedNumber angle)
    {
        FixedNumber intAngle = angle;
        int times = (intAngle / AngleMax).ToInt();
        FixedNumber agl = angle - times * AngleMax;
        if (agl < 0)
        {
            agl += AngleMax;
        }
        return agl;
    }

    /// <summary>
    /// 求点投射到直线上的点
    /// </summary>
    /// <param name="fromPos">起点</param>
    /// <param name="toPos">终点</param>
    /// <param name="dir">投射向量</param>
    /// <returns></returns>
    public static FixedVector3 ProjectPointOnLine(FixedVector3 fromPos, FixedVector3 toPos, FixedVector3 dir)
    {
        FixedVector3 lhs = toPos - fromPos;
        return FixedVector3.Project(lhs, dir) + fromPos;
        //FixedNumber num = FixedVector3.Dot(lhs, dir);
        //return (fromPos + dir * num);
    }

    /// <summary>
    /// 投射点在线段上的方位
    /// </summary>
    /// <param name="fromPos">起点</param>
    /// <param name="toPos">终点</param>
    /// <param name="point">检测点</param>
    /// <returns>0终点外;1起点后;2中间</returns>
    public static int PointOnWhichSideOfLineSegment(FixedVector3 fromPos, FixedVector3 toPos, FixedVector3 point)
    {
        FixedVector3 rhs = toPos - fromPos;
        FixedVector3 lhs = point - fromPos;

        if (FixedVector3.Dot(lhs, rhs) <= FixedNumber.Zero)
        {
            return 1;
        }
        if (rhs.sqrMagnitudeLong <= lhs.sqrMagnitudeLong)
        {
            return 0;
        }
        return 2;
    }

    public static FixedVector3 LocalToWorldPoint(FixedVector3 point, FixedNumber yaw, FixedVector3 root)
    {
        return ((FixedVector3)(FixedQuaternion.Euler(FixedNumber.Zero, yaw, FixedNumber.Zero) * root)) + point;
    }

    public static FixedVector3 WorldToLocalPoint(FixedVector3 point, FixedNumber yaw, FixedVector3 root)
    {
        return ((FixedVector3)(FixedQuaternion.Euler(FixedNumber.Zero, FixedNumber.MakeFixNum(360, 1) - yaw, FixedNumber.Zero) * (root - point)));
    }

    #region Transform
    public static FixedVector3 Transform(ref FixedVector3 point, ref FixedVector3 axis_x, ref FixedVector3 axis_y, ref FixedVector3 axis_z, ref FixedVector3 trans)
    {
        //就是一个3*3矩阵变换一个方向的计算公式 
        return new FixedVector3(axis_x.x * point.x + axis_y.x * point.y + axis_z.x * point.z + trans.x,
            axis_x.y * point.x + axis_y.y * point.y + axis_z.y * point.z + trans.y,
            axis_x.z * point.x + axis_y.z * point.y + axis_z.z * point.z + trans.z);
    }

    public static FixedVector3 Transform(FixedVector3 point, ref FixedVector3 axis_x, ref FixedVector3 axis_y, ref FixedVector3 axis_z, ref FixedVector3 trans)
    {
        return new FixedVector3(axis_x.x * point.x + axis_y.x * point.y + axis_z.x * point.z + trans.x,
            axis_x.y * point.x + axis_y.y * point.y + axis_z.y * point.z + trans.y,
            axis_x.z * point.x + axis_y.z * point.y + axis_z.z * point.z + trans.z);
    }

    public static FixedVector3 Transform(ref FixedVector3 point, ref FixedVector3 axis_x, ref FixedVector3 axis_y, ref FixedVector3 axis_z, ref FixedVector3 trans, ref FixedVector3 scale)
    {
        FixedNumber num = point.x * scale.x;
        FixedNumber num2 = point.y * scale.x;
        FixedNumber num3 = point.z * scale.x;
        return new FixedVector3(axis_x.x * num + axis_y.x * num2 + axis_z.x * num3 + trans.x,
            axis_x.y * num + axis_y.y * num2 + axis_z.y * num3 + trans.y,
            axis_x.z * num + axis_y.z * num2 + axis_z.z * num3 + trans.z);
    }

    public static FixedVector3 Transform(ref FixedVector3 point, ref FixedVector3 forward, ref FixedVector3 trans)
    {
        FixedVector3 up = FixedVector3.Up;
        FixedVector3 vInt = FixedVector3.Cross(FixedVector3.Up, forward);
        return FixedMath.Transform(ref point, ref vInt, ref up, ref forward, ref trans);
    }

    public static FixedVector3 Transform(FixedVector3 point, FixedVector3 forward, FixedVector3 trans)
    {
        FixedVector3 up = FixedVector3.Up;
        FixedVector3 vInt = FixedVector3.Cross(FixedVector3.Up, forward);
        return FixedMath.Transform(ref point, ref vInt, ref up, ref forward, ref trans);
    }

    public static FixedVector3 Transform(FixedVector3 point, FixedVector3 forward, FixedVector3 trans, FixedVector3 scale)
    {
        FixedVector3 up = FixedVector3.Up;
        FixedVector3 vInt = FixedVector3.Cross(FixedVector3.Up, forward);
        return FixedMath.Transform(ref point, ref vInt, ref up, ref forward, ref trans, ref scale);
    }
    #endregion

    #region line
    public static bool IsPowerOfTwo(int x)
    {
        return (x & x - 1) == 0;
    }

    public static int CeilPowerOfTwo(int x)
    {
        x--;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        x++;
        return x;
    }

    public static void SegvecToLinegen(ref FixedVector2 segSrc, ref FixedVector2 segVec, out FixedNumber a, out FixedNumber b, out FixedNumber c)
    {
        a = segVec.y;
        b = -segVec.x;
        c = segVec.x * segSrc.y - segSrc.x * segVec.y;
    }

    private static bool IsPointOnSegment(ref FixedVector2 segSrc, ref FixedVector2 segVec, FixedNumber x, FixedNumber y)
    {
        FixedNumber num = x - segSrc.x;
        FixedNumber num2 = y - segSrc.y;
        return segVec.x * num + segVec.y * num2 >= 0 && num * num + num2 * num2 <= segVec.sqrMagnitude;
    }

    public static bool IntersectSegment(ref FixedVector2 seg1Src, ref FixedVector2 seg1Vec, ref FixedVector2 seg2Src, ref FixedVector2 seg2Vec, out FixedVector2 interPoint)
    {
        FixedNumber num;
        FixedNumber num2;
        FixedNumber num3;
        FixedMath.SegvecToLinegen(ref seg1Src, ref seg1Vec, out num, out num2, out num3);
        FixedNumber num4;
        FixedNumber num5;
        FixedNumber num6;
        FixedMath.SegvecToLinegen(ref seg2Src, ref seg2Vec, out num4, out num5, out num6);
        FixedNumber num7 = num * num5 - num4 * num2;
        if (num7 != 0)
        {
            FixedNumber num8 = num2 * num6 - num5 * num3 / num7;
            FixedNumber num9 = num4 * num3 - num * num6 / num7;
            bool result = FixedMath.IsPointOnSegment(ref seg1Src, ref seg1Vec, num8, num9) && FixedMath.IsPointOnSegment(ref seg2Src, ref seg2Vec, num8, num9);
            interPoint.x = num8;
            interPoint.y = num9;
            return result;
        }
        interPoint = FixedVector2.zero;
        return false;
    }

    //射线检测法，一条经过点p平行于x轴的射线与每条线段交点个数，偶数为在外侧，奇数在内测
    public static bool PointInPolygon(ref FixedVector2 pnt, FixedVector2[] plg)
    {
        if (plg == null || plg.Length < 3)
        {
            return false;
        }
        bool flag = false;
        int i = 0;
        int num = plg.Length - 1;
        while (i < plg.Length)
        {
            FixedVector2 vInt = plg[i];
            FixedVector2 vIVector2 = plg[num];
            if ((vInt.y <= pnt.y && pnt.y < vIVector2.y) || (vIVector2.y <= pnt.y && pnt.y < vInt.y))
            {
                FixedNumber num2 = vIVector2.y - vInt.y;
                FixedNumber num3 = (pnt.y - vInt.y) * (vIVector2.x - vInt.x) - (pnt.x - vInt.x) * num2;
                if (num2 > 0)
                {
                    if (num3 > 0)
                    {
                        flag = !flag;
                    }
                }
                else if (num3 < 0)
                {
                    flag = !flag;
                }
            }
            num = i++;
        }
        return flag;
    }

    public static bool SegIntersectPlg(ref FixedVector2 segSrc, ref FixedVector2 segVec, FixedVector2[] plg, out FixedVector2 nearPoint, out FixedVector2 projectVec)
    {
        nearPoint = FixedVector2.zero;
        projectVec = FixedVector2.zero;
        if (plg == null || plg.Length < 2)
        {
            return false;
        }
        bool result = false;
        FixedNumber num = -FixedNumber.One;
        int num2 = -1;
        for (int i = 0; i < plg.Length; i++)
        {
            FixedVector2 vInt = plg[(i + 1) % plg.Length] - plg[i];
            FixedVector2 vIVector2;
            if (FixedMath.IntersectSegment(ref segSrc, ref segVec, ref plg[i], ref vInt, out vIVector2))
            {
                FixedNumber sqrMagnitudeLong = (vIVector2 - segSrc).sqrMagnitude;
                if (num < 0 || sqrMagnitudeLong < num)
                {
                    nearPoint = vIVector2;
                    num = sqrMagnitudeLong;
                    num2 = i;
                    result = true;
                }
            }
        }
        if (num2 >= 0)
        {
            FixedVector2 lhs = plg[(num2 + 1) % plg.Length] - plg[num2];
            FixedVector2 vIVector3 = segSrc + segVec - nearPoint;
            FixedNumber num3 = vIVector3.x * lhs.x + vIVector3.y * lhs.y;
            if (num3 < 0)
            {
                num3 = -num3;
                lhs = -lhs;
            }
            FixedNumber sqrMagnitudeLong2 = lhs.sqrMagnitude;
            projectVec.x = lhs.x * num3 / sqrMagnitudeLong2;
            projectVec.y = lhs.y * num3 / sqrMagnitudeLong2;
        }
        return result;
    }
    #endregion

    public static FixedNumber Floor(FixedNumber value)
    {
        return new FixedNumber((long)((ulong)value.numerator & 0xFFFFFFFFFFFF0000));
    }
    /// <summary>
    /// 2的value次方
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static FixedNumber Pow2(FixedNumber value)
    {
        if (value.numerator == 0)
        {
            return FixedNumber.One;
        }

        bool negative = value.numerator < 0;
        if (negative)
        {
            value *= -1;
        }

        if (value == FixedNumber.One)
        {
            return negative ? new FixedNumber(FixedNumber.RAW_ONE >> 1) : new FixedNumber(FixedNumber.RAW_ONE << 1);
        }
        if (value >= Log2Max)
        {
            return negative ? FixedNumber.One / FixedNumber.MaxValue : FixedNumber.MaxValue;
        }
        if (value <= Log2Min)
        {
            return negative ? FixedNumber.MaxValue : FixedNumber.Zero;
        }

        /* The algorithm is based on the power series for exp(x):
         * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
         * From term n, we get term n+1 by multiplying with x/n.
         * When the sum term drops to zero, we can stop summing.
         */
        int intergerPart = Floor(value).ToInt();
        value = new FixedNumber(value.numerator & 0xFFFF);

        var result = FixedNumber.One;
        var term = FixedNumber.One;

        int index = 1;
        while (term.numerator != 0)
        {
            term = FastMul(FastMul(value, term), Ln2) / (FixedNumber)index;
            result += term;
            ++index;
        }

        result = new FixedNumber(result.numerator << intergerPart);
        if (negative)
        {
            result = FixedNumber.One / result;
        }

        return result;
    }

    public static FixedNumber FastMul(FixedNumber x, FixedNumber y)
    {
        var xRaw = x.numerator;
        var yRaw = y.numerator;

        var xRawLow = (ulong)(xRaw & 0xFFFF);
        var xRawHigh = xRaw >> FixedNumber.FRACTIONAL_BITS;
        var yRawLow = (ulong)(yRaw & 0xFFFF);
        var yRawHigh = yRaw >> FixedNumber.FRACTIONAL_BITS;

        var lowlow = xRawLow * yRawLow;
        var lowHigh = (long)xRawLow * yRawHigh;
        var highLow = xRawHigh * (long)yRawLow;
        var highHigh = xRawHigh * yRawHigh;

        var lowResult = lowlow >> FixedNumber.FRACTIONAL_BITS;
        var midResult1 = lowHigh;
        var midResult2 = highLow;
        var highResult = highHigh << FixedNumber.FRACTIONAL_BITS;

        var sum = (long)lowResult + midResult1 + midResult2 + highResult;
        return new FixedNumber(sum);
    }

    public static FixedNumber Log2(FixedNumber value)
    {
        if (value.numerator <= 0)
        {
            throw new System.ArgumentOutOfRangeException("Non-positive value passed to Log", "value");
        }

        // This implementation is based on Clay.S.Turner's fast binary logarithm algorithm
        // http://www.claysturner.com/dsp/BinaryLogarithm.pdf
        long y = 0;

        var one = FixedNumber.RAW_ONE;
        var two = one << 1;
        long rawX = value.numerator;
        while (rawX < one)
        {
            rawX <<= 1;
            y -= one;
        }

        while (rawX >= two)
        {
            rawX >>= 1;
            y += one;
        }

        var z = new FixedNumber(rawX);
        long b = 1U << (FixedNumber.FRACTIONAL_BITS - 1); // 0.5

        for (int index = 0; index < FixedNumber.FRACTIONAL_BITS; ++index)
        {
            z = FastMul(z, z);
            if (z.numerator >= two)
            {
                z = new FixedNumber(z.numerator >> 1);
                y += b;
            }
            b >>= 1;
        }

        return new FixedNumber(y);
    }

    private struct PowCacheItem
    {
        public FixedNumber value;
        public FixedNumber exp;
    }
    private static PowCacheItem[] _powCacheIndex = new PowCacheItem[4096];
    private static FixedNumber[] _powCacheValue = new FixedNumber[4096];

    public static FixedNumber Pow(FixedNumber value, FixedNumber exp)
    {
        if (value == FixedNumber.One)
        {
            return FixedNumber.One;
        }

        if (exp.numerator == 0)
        {
            return FixedNumber.One;
        }

        if (value.numerator == 0)
        {
            if (exp.numerator < 0)
            {
                return FixedNumber.MaxValue;
            }

            return FixedNumber.Zero;
        }

        var tempV = (value._raw >> 16) ^ value._raw;
        var tempE = (exp._raw >> 16) ^ exp._raw;
        var tempIndex = (tempV ^ tempE) % _powCacheIndex.Length;
        if (_powCacheIndex[tempIndex].value == value && _powCacheIndex[tempIndex].exp == exp)
        {
            return _powCacheValue[tempIndex];
        }

        bool negative = false;
        // negative only support integer power
        if (value.numerator < 0)
        {
            if (!exp.IsInteger)
            {
                return FixedNumber.MaxValue;
            }

            negative = (exp.ToInt() % 2 != 0);
            value *= -1;
        }

        var log2 = Log2(value);
        var result = negative ? -1 * Pow2(exp * log2) : Pow2(exp * log2);

        _powCacheIndex[tempIndex].value = value;
        _powCacheIndex[tempIndex].exp = exp;
        _powCacheValue[tempIndex] = result;

        return result;
    }


}
