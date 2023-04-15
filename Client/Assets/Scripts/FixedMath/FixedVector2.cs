using System;


[Serializable]
public struct FixedVector2
{
    public FixedNumber x;

    public FixedNumber y;

    public static FixedVector2 zero = default(FixedVector2);

    private static readonly int[] Rotations = new int[]
    {
        1,
        0,
        0,
        1,
        0,
        1,
        -1,
        0,
        -1,
        0,
        0,
        -1,
        0,
        -1,
        1,
        0
    };

    public FixedNumber sqrMagnitude
    {
        get
        {
            return this.x * this.x + this.y * this.y;
        }
    }

    //public IntFactor sqrMagnitudeLong
    //{
    //    get
    //    {
    //        long num = (long)this.x;
    //        long num2 = (long)this.y;
    //        return num * num + num2 * num2;
    //    }
    //}

    public FixedNumber magnitude
    {
        get
        {
            //long num = (long)this.x;
            //long num2 = (long)this.y;
            return FixedMath.Sqrt(sqrMagnitude);
        }
    }

    public FixedVector2 normalized
    {
        get
        {
            FixedVector2 result = new FixedVector2(this.x, this.y);
            result.Normalize();
            return result;
        }
    }


    public FixedVector2(FixedNumber x, FixedNumber y)
    {
        this.x = x;
        this.y = y;
    }
    public static FixedNumber Dot(ref FixedVector2 a, ref FixedVector2 b)
    {
        return a.x * b.x + a.y * b.y;
    }

    //public static long DotLong(ref IVector2 a, ref IVector2 b)
    //{
    //    return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
    //}

    //public static long DotLong(IVector2 a, IVector2 b)
    //{
    //    return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
    //}

    public static FixedNumber DetLong(ref FixedVector2 a, ref FixedVector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    //public static long DetLong(IVector2 a, IVector2 b)
    //{
    //    return (long)a.x * (long)b.y - (long)a.y * (long)b.x;
    //}

    //public override bool Equals(object o)
    //{
    //    if (o == null)
    //    {
    //        return false;
    //    }
    //    IVector2 vInt = (IVector2)o;
    //    return this.x == vInt.x && this.y == vInt.y;
    //}

    //public override int GetHashCode()
    //{
    //    return this.x * 49157 + this.y * 98317;
    //}

    public static FixedVector2 operator /(FixedVector2 v, FixedNumber f)
    {
        return new FixedVector2(v.x / f, v.y / f);
    }

    public static FixedVector2 operator *(FixedVector2 v, FixedNumber f)
    {
        return new FixedVector2(v.x * f, v.y * f);
    }

    public static FixedVector2 Rotate(FixedVector2 v, int r)
    {
        r %= 4;
        return new FixedVector2(v.x * FixedVector2.Rotations[r * 4] + v.y * FixedVector2.Rotations[r * 4 + 1], v.x * FixedVector2.Rotations[r * 4 + 2] + v.y * FixedVector2.Rotations[r * 4 + 3]);
    }

    //x,y will use the min,so the result may not equal all of the two inputs
    //public static IVector2 Min(IVector2 a, IVector2 b)
    //{
    //    return new IVector2(IntMath.Min(a.x, b.x), IntMath.Min(a.y, b.y));
    //}
    ////x,y calculate seperate,o the result may not equal all of the two inputs
    //public static IVector2 Max(IVector2 a, IVector2 b)
    //{
    //    return new IVector2(IntMath.Max(a.x, b.x), IntMath.Max(a.y, b.y));
    //}
    //take the x and z
    //default up is y
    public static FixedVector2 FromIVector3XZ(FixedVector3 o)
    {
        return new FixedVector2(o.x, o.z);
    }

    public static FixedVector3 ToIVector3XZ(FixedVector2 o)
    {
        return new FixedVector3(o.x, FixedNumber.Zero, o.y);
    }

    public override string ToString()
    {
        return string.Concat(new object[]
        {
            "(",
            this.x,
            ", ",
            this.y,
            ")"
        });
    }

    //public void Min(ref IVector2 r)
    //{
    //    this.x = IntMath.Min(this.x, r.x);
    //    this.y = IntMath.Min(this.y, r.y);
    //}

    //public void Max(ref IVector2 r)
    //{
    //    this.x = IntMath.Max(this.x, r.x);
    //    this.y = IntMath.Max(this.y, r.y);
    //}

    public void Normalize()
    {
        if (x == FixedNumber.Zero && y == FixedNumber.Zero)
        {
            return;
        }
        FixedNumber sqrtMagnitude = this.magnitude;
        this.x /= sqrtMagnitude;
        this.y /= sqrtMagnitude;
    }

    public static FixedVector2 ClampMagnitude(FixedVector2 v, FixedNumber maxLength)
    {
        if (maxLength == FixedNumber.Zero)
            return FixedVector2.zero;
        FixedNumber sqrMagnitudeLong = v.sqrMagnitude;
        if (sqrMagnitudeLong > maxLength * maxLength)
        {
            FixedNumber sqrtMagnitude = FixedMath.Sqrt(sqrMagnitudeLong);
            return new FixedVector2(v.x * maxLength / sqrtMagnitude, v.y * maxLength / sqrtMagnitude);
        }
        return v;
    }

    public static FixedVector2 Lerp(FixedVector2 a, FixedVector2 b, FixedNumber f)
    {
        return new FixedVector2((b.x - a.x) * f + a.x, (b.y - a.y) * f + a.y);
    }
    //public static explicit operator Vector2(IVector2 ob)
    //{
    //    return new Vector2((float)ob.x * 0.001f, (float)ob.y * 0.001f);
    //}

    //public static explicit operator IVector2(Vector2 ob)
    //{
    //    return new IVector2((int)Math.Round((double)(ob.x * 1000f)), (int)Math.Round((double)(ob.y * 1000f)));
    //}

    public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b)
    {
        return new FixedVector2(a.x + b.x, a.y + b.y);
    }

    public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b)
    {
        return new FixedVector2(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(FixedVector2 a, FixedVector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(FixedVector2 a, FixedVector2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static FixedVector2 operator -(FixedVector2 lhs)
    {
        lhs.x = -lhs.x;
        lhs.y = -lhs.y;
        return lhs;
    }

    public static FixedVector2 operator *(FixedVector2 lhs, int rhs)
    {
        lhs.x *= rhs;
        lhs.y *= rhs;
        return lhs;
    }

    public override bool Equals(object obj)
    {
        FixedVector2 r2 = (FixedVector2)obj;
        return x == r2.x && y == r2.y;
    }

    public override int GetHashCode()
    {
        unsafe
        {
            fixed (FixedVector2* key = &this)
            {
                return *((int*)key);
            }
        }
    }
}
