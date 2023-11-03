using System;


[Serializable]
public struct FixedVector3
{

    public FixedNumber x;

    public FixedNumber y;

    public FixedNumber z;

    public static readonly FixedVector3 Zero = new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, FixedNumber.Zero);
    public static readonly FixedVector3 One = new FixedVector3(FixedNumber.One, FixedNumber.One, FixedNumber.One);
    public static readonly FixedVector3 Half = new FixedVector3(FixedNumber.Half, FixedNumber.Half, FixedNumber.Half);
    public static readonly FixedVector3 Forward = new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, FixedNumber.One);
    public static readonly FixedVector3 Back = new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, -FixedNumber.One);
    public static readonly FixedVector3 Up = new FixedVector3(FixedNumber.Zero, FixedNumber.One, FixedNumber.Zero);
    public static readonly FixedVector3 Down = new FixedVector3(FixedNumber.Zero, -FixedNumber.One, FixedNumber.Zero);
    public static readonly FixedVector3 Right = new FixedVector3(FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);
    public static readonly FixedVector3 Left = new FixedVector3(-FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);

    public FixedNumber this[int i]
    {
        get
        {
            return (i != 0) ? ((i != 1) ? this.z : this.y) : this.x;
        }
        set
        {
            if (i == 0)
            {
                this.x = value;
            }
            else if (i == 1)
            {
                this.y = value;
            }
            else
            {
                this.z = value;
            }
        }
    }

    public FixedVector2 xz
    {
        get
        {
            return new FixedVector2(this.x, this.z);
        }
    }

    public FixedNumber Magnitude
    {
        get
        {
            return FixedMath.Sqrt(x * x + y * y + z * z);
        }
    }

    public FixedNumber magnitude2D
    {
        get
        {
            return FixedMath.Sqrt(x * x + z * z);
        }
    }

    public FixedNumber unsafeSqrMagnitude
    {
        get
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
        }
    }

    public FixedVector3 abs
    {
        get
        {
            return new FixedVector3(FixedMath.Abs(this.x), FixedMath.Abs(this.y), FixedMath.Abs(this.z));
        }
    }


    public FixedVector3 Normalized
    {
        get
        {
            if (unsafeSqrMagnitude == FixedNumber.Zero)
                return FixedVector3.Zero;
            FixedNumber length = Magnitude;
            return new FixedVector3(x / length, y / length, z / length);
        }
    }

    public FixedVector3(FixedNumber x, FixedNumber y, FixedNumber z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public FixedVector3(UnityEngine.Vector3 vector)
    {
        this.x = FixedNumber.MakeFixNum((int)(vector.x * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
        this.y = FixedNumber.MakeFixNum((int)(vector.y * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
        this.z = FixedNumber.MakeFixNum((int)(vector.z * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
    }

    public FixedVector3 Scale(FixedNumber scale)
    {
        return this * scale;
    }

    public FixedVector3 YZero()
    {
        return new FixedVector3(x, FixedNumber.Zero, z);
    }

    public static FixedNumber Distance(FixedVector3 start, FixedVector3 end)
    {
        FixedVector3 dt = end - start;
        return dt.Magnitude;
    }

    public static FixedNumber SqrDistance(FixedVector3 start, FixedVector3 end)
    {
        FixedVector3 dt = end - start;
        return dt.sqrMagnitudeLong;
    }

    public static FixedNumber SqrDistanceXZ(FixedVector3 start, FixedVector3 end)
    {
        FixedVector3 dt = end - start;
        return dt.sqrMagnitudeLongXZ;
    }

    /// <summary>
    /// 返回的是弧度
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static FixedNumber AngleIntRad(FixedVector3 lhs, FixedVector3 rhs)
    {
        // 先求出两个向量各个位数的平方和然后相乘，意义就是算出来斜边的平方值
        FixedNumber den = lhs.sqrMagnitudeLong * rhs.sqrMagnitudeLong;
        if (den == 0)
        {
            return FixedNumber.Zero;
        }
        // 得到临边的投影
        var dot = FixedVector3.Dot(ref lhs, ref rhs);
        // 邻边的平方比斜边的平方
        FixedNumber val = FixedMath.Sqrt((dot * dot) / den);
        // 反余弦（临边比斜边的值求出弧度值）求出弧度值
        return FixedMath.Acos(dot < 0 ? -val : val);
    }

    /// <summary>
    /// 返回的角度
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static FixedNumber AngleInt(FixedVector3 lhs, FixedVector3 rhs)
    {
        FixedNumber radValue = AngleIntRad(lhs, rhs);    // 自己前方于篮筐的弧度
        FixedNumber tmpValue = radValue * FixedMath.Rad2Deg;    // 通过弧度算出角度
        return FixedMath.Abs(ClampAngleTo180(tmpValue));    // 将角度限制在180以内并且求出绝对值
    }

    public static FixedNumber AngleIntSingle(FixedVector3 lhs, FixedVector3 rhs)
    {
        FixedNumber radValue = AngleIntRad(lhs, rhs);
        FixedNumber tmpValue = ClampAngleTo180(radValue * FixedMath.Rad2Deg);
        // tmpValue = [0, 180]
        return FixedVector3.Cross(lhs, rhs).y > FixedNumber.Zero ? tmpValue : -tmpValue;
    }

    /// <summary>
    /// 约束到正负180之间
    /// </summary>
    /// <returns></returns>
    public static FixedNumber ClampAngleTo180(FixedNumber a)
    {
        if (a > 0)
        {
            while (a > FixedMath.HalfAngleMax)
            {
                a -= FixedMath.AngleMax;
            }
        }
        else if (a < 0)
        {
            while (a < -FixedMath.HalfAngleMax)
            {
                a += FixedMath.AngleMax;
            }
        }
        return a;
    }

    //public static IntFactor DotWithInfactor(ref IVector3 lhs, ref IVector3 rhs)
    //{
    //    return new IntFactor(((long)lhs.x * rhs.x + (long)lhs.y * rhs.y + (long)lhs.z * rhs.z) / GlobalDefine.BaseCalFactor, GlobalDefine.BaseCalFactor);
    //}

    public static FixedNumber Dot(ref FixedVector3 lhs, ref FixedVector3 rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
    }

    public static FixedNumber Dot(FixedVector3 lhs, FixedVector3 rhs, bool normalLizeInput = false)
    {
        if (normalLizeInput)
        {
            lhs.Normalize();
            rhs.Normalize();
        }
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
    }

    //public static IntFactor DotLong(IVector3 lhs, IVector3 rhs)
    //{
    //    return (long)lhs.x * (long)rhs.x + (long)lhs.y * (long)rhs.y + (long)lhs.z * (long)rhs.z;
    //}

    //public static long DotLong(ref IVector3 lhs, ref IVector3 rhs)
    //{
    //    return (long)lhs.x * (long)rhs.x + (long)lhs.y * (long)rhs.y + (long)lhs.z * (long)rhs.z;
    //}

    //public static long DotXZLong(ref IVector3 lhs, ref IVector3 rhs)
    //{
    //    return (long)lhs.x * (long)rhs.x + (long)lhs.z * (long)rhs.z;
    //}

    //public static long DotXZLong(IVector3 lhs, IVector3 rhs)
    //{
    //    return (long)lhs.x * (long)rhs.x + (long)lhs.z * (long)rhs.z;
    //}

    public static FixedVector3 Cross(ref FixedVector3 lhs, ref FixedVector3 rhs)
    {
        return new FixedVector3(lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);
    }

    public static FixedVector3 Cross(FixedVector3 lhs, FixedVector3 rhs)
    {
        return new FixedVector3(lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);
    }

    public FixedVector3 Normal2D()
    {
        return new FixedVector3(this.z, this.y, -this.x);
    }

    public void Normalize()
    {
        if (x == 0 && y == 0 && z == 0)
        {
            return;
        }
        FixedNumber length = Magnitude;
        this.x /= length;
        this.y /= length;
        this.z /= length;
    }



    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})", x.ToString(), y.ToString(), z.ToString());
    }

    //public override bool Equals(object o)
    //{
    //    if (o == null)
    //    {
    //        return false;
    //    }
    //    IVector3 vInt = (IVector3)o;
    //    return this.x == vInt.x && this.y == vInt.y && this.z == vInt.z;
    //}


    public static FixedVector3 Lerp(FixedVector3 a, FixedVector3 b, FixedNumber f)
    {
        return new FixedVector3((b.x - a.x) * f + a.x, (b.y - a.y) * f + a.y, (b.z - a.z) * f + a.z);
    }

    public static FixedVector3 ClampLerp(FixedVector3 a, FixedVector3 b, FixedNumber f)
    {
        f = FixedMath.Clamp(f, FixedNumber.Zero, FixedNumber.One);
        return Lerp(a, b, f);
    }

    public static FixedVector3 Project(FixedVector3 vector, FixedVector3 normal)
    {
        var sqrMag = Dot(normal, normal);
        if (sqrMag < FixedNumber.ApproximatelyError)
        {
            return FixedVector3.Zero;
        }
        else
        {
            var dot = Dot(vector, normal);
            return new FixedVector3(normal.x * dot / sqrMag, normal.y * dot / sqrMag, normal.z * dot / sqrMag);
        }
    }

    public static FixedVector3 ProjectionVector(FixedVector3 from, FixedVector3 to)
    {
        FixedNumber rad = AngleIntRad(from, to);
        FixedNumber cos = FixedMath.Cos(rad);
        FixedNumber projection = from.magnitude2D * cos;
        return to.Normalized.Scale(projection);
    }

    public FixedNumber XZSqrMagnitude(FixedVector3 rhs)
    {
        FixedNumber num = (this.x - rhs.x);
        FixedNumber num2 = (this.z - rhs.z);
        return num * num + num2 * num2;
    }

    public FixedNumber XZSqrMagnitude(ref FixedVector3 rhs)
    {
        FixedNumber num = (this.x - rhs.x);
        FixedNumber num2 = (this.z - rhs.z);
        return num * num + num2 * num2;
    }

    public bool IsEqualXZ(FixedVector3 rhs)
    {
        return this.x == rhs.x && this.z == rhs.z;
    }

    public bool IsEqualXZ(ref FixedVector3 rhs)
    {
        return this.x == rhs.x && this.z == rhs.z;
    }


    public static bool operator ==(FixedVector3 lhs, FixedVector3 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }

    public static bool operator !=(FixedVector3 lhs, FixedVector3 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
    }


    public static FixedVector3 operator -(FixedVector3 lhs, FixedVector3 rhs)
    {
        lhs.x -= rhs.x;
        lhs.y -= rhs.y;
        lhs.z -= rhs.z;
        return lhs;
    }

    public static FixedVector3 operator -(FixedVector3 lhs)
    {
        lhs.x = -lhs.x;
        lhs.y = -lhs.y;
        lhs.z = -lhs.z;
        return lhs;
    }

    public static FixedVector3 operator +(FixedVector3 lhs, FixedVector3 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }

    public static FixedVector3 operator *(FixedVector3 lhs, int rhs)
    {
        lhs.x *= rhs;
        lhs.y *= rhs;
        lhs.z *= rhs;
        return lhs;
    }

    public static FixedVector3 operator /(FixedVector3 lhs, int div)
    {
        if (div == 0)
        {
            return lhs;
        }
        lhs.x /= div;
        lhs.y /= div;
        lhs.z /= div;
        return lhs;
    }

    public static FixedVector3 operator *(FixedVector3 lhs, FixedVector3 rhs)
    {
        lhs.x *= rhs.x;
        lhs.y *= rhs.y;
        lhs.z *= rhs.z;
        return lhs;
    }


    public static implicit operator string(FixedVector3 ob)
    {
        return ob.ToString();
    }


    #region safe


    public FixedNumber sqrMagnitudeLong
    {
        get
        {
            return x * x + y * y + z * z;
        }
    }

    public FixedNumber sqrMagnitudeLongXZ
    {
        get
        {
            return x * x + z * z;
        }
    }

    public static FixedVector3 operator *(FixedVector3 v, FixedNumber f)
    {
        return new FixedVector3(v.x * f, v.y * f, v.z * f);
    }

    public static FixedVector3 operator *(FixedNumber f, FixedVector3 v)
    {
        return new FixedVector3(v.x * f, v.y * f, v.z * f);
    }

    public static FixedVector3 operator /(FixedVector3 v, FixedNumber f)
    {
        return new FixedVector3(v.x / f, v.y / f, v.z / f);
    }
    #endregion

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(FixedVector3))
        {
            FixedVector3 r2 = (FixedVector3)obj;
            return x == r2.x && y == r2.y && z == r2.z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unsafe
        {
            fixed (FixedVector3* key = &this)
            {
                return *((int*)key);
            }
        }
    }

#if !Server
    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
    }
#endif
}


