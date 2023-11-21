using System;

[Serializable]
public partial struct FixedNumber
{
    internal const int FRACTIONAL_BITS = 16;
    internal const int INTEGER_BITS = sizeof(long) * 8 - FRACTIONAL_BITS;
    internal const int FRACTION_MASK = (int)(ulong.MaxValue >> INTEGER_BITS);
    internal const int INTEGER_MASK = (int)(-1 & ~FRACTION_MASK);
    internal const int FRACTION_RANGE = FRACTION_MASK + 1;

    public static readonly FixedNumber MinValue = new FixedNumber(long.MinValue);
    public static readonly FixedNumber MaxValue = new FixedNumber(long.MaxValue);
    public static readonly FixedNumber Zero = new FixedNumber(0);
    public static readonly FixedNumber Hundred = FixedNumber.MakeFixNum(100, 1);
    public static readonly FixedNumber One = new FixedNumber(RAW_ONE);
    public static readonly FixedNumber NegOne = -One;
    public static readonly FixedNumber Half = One / 2;
    public static readonly FixedNumber ApproximatelyError = FixedNumber.MakeFixNum(1, 100);
    public static readonly FixedNumber PredictApproximatelyError = FixedNumber.MakeFixNum(30, 100);
    public static readonly FixedNumber HalfRound = FixedNumber.MakeFixNum(180, 1); // 180度
    public static readonly FixedNumber OneTenth = MakeFixNum(1, 10);

    public const long RAW_ONE = 1L << FRACTIONAL_BITS;


    public long _raw;
    public long numerator => _raw;

    public bool IsInteger => (_raw & FRACTION_MASK) == 0;

    public FixedNumber(long val)
    {
        _raw = val;
    }

    public static FixedNumber MakeFixNum(long numerator, long denominator)
    {
        if (denominator == 0)
            return MaxValue;
        return new FixedNumber((((numerator) << (FRACTIONAL_BITS + 1)) / denominator + 1) >> 1);
    }

    public int ToInt()
    {
        return (int)(_raw >> FRACTIONAL_BITS);
    }

    public short ToShort()
    {
        return (short)(_raw >> FRACTIONAL_BITS);
    }

    public double ToDouble()
    {
        return (double)(_raw >> FRACTIONAL_BITS) + (_raw & FRACTION_MASK) / (double)FRACTION_RANGE;
    }

    public float ToFloat()
    {
        return (float)(_raw >> FRACTIONAL_BITS) + (_raw & FRACTION_MASK) / (float)FRACTION_RANGE;
    }

    public override bool Equals(object obj)
    {
        return obj != null && base.GetType() == obj.GetType() && this == (FixedNumber)obj;
    }

    public override int GetHashCode()
    {
        return _raw.GetHashCode();
    }

    public override string ToString()
    {
        return this.ToDouble().ToString("f4");
    }

    public int CompareTo(FixedNumber other)
    {
        return this._raw.CompareTo(other._raw);
    }

    #region override operator <
    public static bool operator <(FixedNumber a, FixedNumber b)
    {
        return a._raw < b._raw;
    }

    public static bool operator <(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) < b._raw;
    }

    public static bool operator <(FixedNumber a, int b)
    {
        return a._raw < ((long)b << FRACTIONAL_BITS);
    }
    #endregion

    #region override operator >
    public static bool operator >(FixedNumber a, FixedNumber b)
    {
        return a._raw > b._raw;
    }

    public static bool operator >(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) > b._raw;
    }

    public static bool operator >(FixedNumber a, int b)
    {
        return a._raw > ((long)b << FRACTIONAL_BITS);
    }
    #endregion

    #region override operator <=
    public static bool operator <=(FixedNumber a, FixedNumber b)
    {
        return a._raw <= b._raw;
    }

    public static bool operator <=(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) <= b._raw;
    }

    public static bool operator <=(FixedNumber a, int b)
    {
        return a._raw <= ((long)b << FRACTIONAL_BITS);
    }
    #endregion

    #region override operator >=
    public static bool operator >=(FixedNumber a, FixedNumber b)
    {
        return a._raw >= b._raw;
    }
    public static bool operator >=(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) >= b._raw;
    }

    public static bool operator >=(FixedNumber a, int b)
    {
        return a._raw >= ((long)b << FRACTIONAL_BITS);
    }
    #endregion

    #region override operator ==
    public static bool operator ==(FixedNumber a, FixedNumber b)
    {
        return a._raw == b._raw;
    }

    public static bool operator ==(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) == b._raw;
    }

    public static bool operator ==(FixedNumber a, int b)
    {
        return a._raw == ((long)b << FRACTIONAL_BITS);
    }
    #endregion

    #region override operator !=
    public static bool operator !=(FixedNumber a, FixedNumber b)
    {
        return a._raw != b._raw;
    }

    public static bool operator !=(FixedNumber a, int b)
    {
        return a._raw != ((long)b << FRACTIONAL_BITS);
    }

    public static bool operator !=(int a, FixedNumber b)
    {
        return ((long)a << FRACTIONAL_BITS) != b._raw;
    }
    #endregion

    #region override operator + 
    public static FixedNumber operator +(FixedNumber a, FixedNumber b)
    {
        return new FixedNumber
        {
            _raw = a._raw + b._raw,
        };
    }

    public static FixedNumber operator +(FixedNumber a, int b)
    {
        return new FixedNumber
        {
            _raw = a._raw + (((long)b) << FRACTIONAL_BITS),
        };
    }

    public static FixedNumber operator +(int i, FixedNumber f)
    {
        return f + i;
    }
    #endregion

    #region override operator -
    public static FixedNumber operator -(FixedNumber a, FixedNumber b)
    {
        return new FixedNumber(a._raw - b._raw);
    }

    public static FixedNumber operator -(FixedNumber a, int b)
    {
        return new FixedNumber(a._raw - ((long)b << FRACTIONAL_BITS));
    }

    public static FixedNumber operator -(int a, FixedNumber b)
    {
        return new FixedNumber
        {
            _raw = ((long)a << FRACTIONAL_BITS) - b._raw,
        };
    }

    #endregion

    #region override operator *
    public static FixedNumber operator *(FixedNumber a, FixedNumber b)
    {
        return new FixedNumber
        {
            _raw = a._raw * b._raw >> FRACTIONAL_BITS,
        };
    }

    public static FixedNumber operator *(FixedNumber a, int b)
    {
        return new FixedNumber
        {
            _raw = a._raw * b,
        };
    }

    public static FixedNumber operator *(int a, FixedNumber b)
    {
        return b * a;
    }
    #endregion

    #region override operator /
    public static FixedNumber operator /(FixedNumber a, FixedNumber b)
    {
        return new FixedNumber((a._raw << FRACTIONAL_BITS) / b._raw);
    }

    public static FixedNumber operator /(FixedNumber a, int b)
    {
        return new FixedNumber(a._raw / b);
    }

    public static FixedNumber operator /(int a, FixedNumber b)
    {
        return new FixedNumber((long)a << FRACTIONAL_BITS) / b;
    }
    #endregion

    #region override operator <<
    public static FixedNumber operator <<(FixedNumber a, int b)
    {
        return new FixedNumber(a._raw << b);
    }
    #endregion

    #region override operator >>
    public static FixedNumber operator >>(FixedNumber a, int b)
    {
        return new FixedNumber(a._raw >> b);
    }
    #endregion

    public static explicit operator FixedNumber(long value)
    {
        return new FixedNumber(value * RAW_ONE);
    }

    public static explicit operator FixedNumber(int value)
    {
        return new FixedNumber(value * RAW_ONE);
    }

    public static FixedNumber operator -(FixedNumber a)
    {
        a._raw = -a._raw;
        return a;
    }


    public static bool ApproximatelyEqual(FixedNumber a, FixedNumber b)
    {
        return FixedMath.Abs(a - b) < ApproximatelyError;
    }

    public static bool ApproximatelyEqual(FixedVector3 a, FixedVector3 b)
    {
        return ApproximatelyEqual(a.x, b.x) &&
            ApproximatelyEqual(a.y, b.y) &&
            ApproximatelyEqual(a.z, b.z);
    }

    public static bool ApproximatelyEqual(FixedQuaternion a, FixedQuaternion b)
    {
        return ApproximatelyEqual(a.x, b.x) &&
            ApproximatelyEqual(a.y, b.y) &&
            ApproximatelyEqual(a.z, b.z) &&
            ApproximatelyEqual(a.w, b.w);
    }


    public static bool ApproximatelyEqual(FixedNumber a, FixedNumber b, FixedNumber error)
    {
        return FixedMath.Abs(a - b) < error;
    }

    public static bool ApproximatelyEqual(FixedVector3 a, FixedVector3 b, FixedNumber error)
    {
        return ApproximatelyEqual(a.x, b.x, error) &&
            ApproximatelyEqual(a.y, b.y, error) &&
            ApproximatelyEqual(a.z, b.z, error);
    }

    public static bool ApproximatelyEqual(FixedQuaternion a, FixedQuaternion b, FixedNumber error)
    {
        return ApproximatelyEqual(a.x, b.x, error) &&
            ApproximatelyEqual(a.y, b.y, error) &&
            ApproximatelyEqual(a.z, b.z, error) &&
            ApproximatelyEqual(a.w, b.w, error);
    }
}

