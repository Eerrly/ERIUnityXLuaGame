
[System.Serializable]
public struct FixedQuaternion
{
    #region Fields

    public FixedNumber x;
    public FixedNumber y;
    public FixedNumber z;
    public FixedNumber w;

    public static readonly FixedQuaternion Identity = new FixedQuaternion(FixedNumber.Zero, FixedNumber.Zero, FixedNumber.Zero, FixedNumber.One);

    #endregion

    #region public
    public FixedQuaternion(FixedNumber ix, FixedNumber iy, FixedNumber iz, FixedNumber iw)
    {
        x = ix;
        y = iy;
        z = iz;
        w = iw;
    }

    public FixedQuaternion(UnityEngine.Quaternion quaternion)
    {
        x = FixedNumber.MakeFixNum((int)(quaternion.x * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
        y = FixedNumber.MakeFixNum((int)(quaternion.y * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
        z = FixedNumber.MakeFixNum((int)(quaternion.z * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
        w = FixedNumber.MakeFixNum((int)(quaternion.w * FixedMath.DataConrvertScale), FixedMath.DataConrvertScale);
    }

    public static FixedQuaternion EluerY(FixedNumber y)
    {
        return Euler(FixedNumber.Zero, y, FixedNumber.Zero);
    }

    public static FixedQuaternion Euler(FixedNumber x, FixedNumber y, FixedNumber z)
    {
        FixedNumber eulerX = (x >> 1) * FixedMath.Deg2Rad;
        FixedNumber cX = FixedMath.Cos(eulerX);
        FixedNumber sX = FixedMath.Sin(eulerX);
        FixedNumber eulerY = (y >> 1) * FixedMath.Deg2Rad;
        FixedNumber cY = FixedMath.Cos(eulerY);
        FixedNumber sY = FixedMath.Sin(eulerY);
        FixedNumber eulerZ = (z >> 1) * FixedMath.Deg2Rad;
        FixedNumber cZ = FixedMath.Cos(eulerZ);
        FixedNumber sZ = FixedMath.Sin(eulerZ);

        FixedNumber ix = sX * cY * cZ - cX * sY * sZ;
        FixedNumber iy = cX * sY * cZ + sX * cY * sZ;
        FixedNumber iz = cX * cY * sZ - sX * sY * cZ;
        FixedNumber iw = cX * cY * cZ + sX * sY * sZ;
        FixedQuaternion q = new FixedQuaternion(ix, iy, iz, iw);
        return q;
    }

    public FixedNumber pitch
    {
        get
        {
            long raw = (this.y.numerator * this.z.numerator + this.w.numerator * this.x.numerator) * 2;
            long raw2 = this.w.numerator * this.w.numerator - this.x.numerator * this.x.numerator - this.y.numerator * this.y.numerator + this.z.numerator * this.z.numerator;
            FixedNumber number = new FixedNumber(raw);
            FixedNumber number2 = new FixedNumber(raw2);
            FixedNumber number3 = FixedMath.Atan2(number.ToInt(), number2.ToInt());
            if (number3 < FixedNumber.Zero)
            {
                number3 += FixedMath.twoPi;
            }
            FixedNumber result = FixedMath.Rad2Deg * number3;
            //    Math.CheckRange(result, Number.zero, 360, "pitch");
            return result;
        }
    }

    public FixedNumber yaw
    {
        get
        {
            long num = (long)((this.x.numerator * this.z.numerator - this.w.numerator * this.y.numerator * -2));
            FixedNumber number = new FixedNumber(num);
            FixedNumber number2 = FixedMath.Asin(FixedMath.Clamp(number, -FixedNumber.One, FixedNumber.One));
            if (number2 < FixedNumber.Zero)
            {
                number2 += FixedMath.twoPi;
            }
            FixedNumber result = FixedMath.Rad2Deg * number2;
            return result;
        }
    }

    public FixedNumber roll
    {
        get
        {
            long raw = (this.x.numerator * this.y.numerator + this.w.numerator * this.z.numerator) * 2;
            long raw2 = this.w.numerator * this.w.numerator + this.x.numerator * this.x.numerator - this.y.numerator * this.y.numerator - this.z.numerator * this.z.numerator;
            FixedNumber number = new FixedNumber(raw);
            FixedNumber number2 = new FixedNumber(raw2);
            FixedNumber number3 = FixedMath.Atan2(number.ToInt(), number2.ToInt());
            if (number3 < FixedNumber.Zero)
            {
                number3 += FixedMath.twoPi;
            }
            FixedNumber result = FixedMath.Rad2Deg * number3;
            return result;
        }
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}, {2}, {3})",
            x.ToString(),
            y.ToString(),
            z.ToString(),
            w.ToString());
    }

    #endregion

    #region static operator

    /// <summary>
    /// 正确
    /// </summary>
    /// <param name="r"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static FixedVector3 operator *(FixedQuaternion r, FixedVector3 v)
    {
        FixedNumber x = r.x + r.x;
        FixedNumber y = r.y + r.y;
        FixedNumber z = r.z + r.z;
        FixedNumber xx = (r.x * x);
        FixedNumber yy = (r.y * y);
        FixedNumber zz = (r.z * z);
        FixedNumber xy = (r.x * y);
        FixedNumber xz = (r.x * z);
        FixedNumber yz = (r.y * z);
        FixedNumber wx = (r.w * x);
        FixedNumber wy = (r.w * y);
        FixedNumber wz = (r.w * z);

        FixedVector3 res;
        res.x = (((1 - (yy + zz)) * v.x
            + (xy - wz) * v.y
            + (xz + wy) * v.z));

        res.y = (((xy + wz) * v.x
            + (1 - (xx + zz)) * v.y
            + (yz - wx) * v.z));

        res.z = (((xz - wy) * v.x
            + (yz + wx) * v.y
            + (1 - (xx + yy)) * v.z));
        return res;
    }
    /// <summary>
    /// 正确
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    public static FixedQuaternion operator *(FixedQuaternion r1, FixedQuaternion r2)
    {
        FixedNumber tempx = ((r1.w * r2.x)
            + (r1.x * r2.w)
            + (r1.y * r2.z)
            - (r1.z * r2.y));
        FixedNumber tempy = ((r1.w * r2.y)
            + (r1.y * r2.w)
            + (r1.z * r2.x)
            - (r1.x * r2.z));
        FixedNumber tempz = ((r1.w * r2.z)
            + (r1.z * r2.w)
            + (r1.x * r2.y)
            - (r1.y * r2.x));

        FixedNumber tempw = ((r1.w * r2.w)
            - (r1.x * r2.x)
            - (r1.y * r2.y)
            - (r1.z * r2.z));
        return new FixedQuaternion(tempx, tempy, tempz, tempw);
    }

    public static bool operator ==(FixedQuaternion r1, FixedQuaternion r2)
    {
        return r1.x == r2.x && r1.y == r2.y && r1.z == r2.z && r1.w == r2.w;
    }

    public static bool operator !=(FixedQuaternion r1, FixedQuaternion r2)
    {
        return !(r1 == r2);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(FixedQuaternion))
        {
            FixedQuaternion r2 = (FixedQuaternion)obj;
            return x == r2.x && y == r2.y && z == r2.z && w == r2.w;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unsafe
        {
            fixed (FixedQuaternion* key = &this)
            {
                return *((int*)key);
            }
        }
    }

    #endregion

    public UnityEngine.Quaternion ToQuaternion()
    {
        return new UnityEngine.Quaternion(x.ToFloat(), y.ToFloat(), z.ToFloat(), w.ToFloat());
    }
}

