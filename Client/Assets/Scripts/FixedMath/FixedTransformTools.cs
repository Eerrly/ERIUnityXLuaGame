
public static class FixedTransformTools
{

    #region public

    public static FixedVector3 Transform(int eualrY, FixedVector3 v)
    {
        return YawMultiplyVector3(eualrY, v);
    }

    public static FixedVector3 LocalToWorldPoint(FixedVector3 point, int yaw, FixedVector3 root)
    {
        return YawMultiplyVector3(yaw, root) + point;
    }

    public static FixedVector3 WorldToLocalPoint(FixedVector3 point, int yaw, FixedVector3 root)
    {
        return YawMultiplyVector3(-yaw, root - point);
    }

    public static FixedVector3 LocalToWorldPoint(FixedVector3 point, FixedQuaternion r, FixedVector3 root)
    {
        return r * root + point;
    }

    public static FixedVector3 YawMultiplyVector3(int yaw, FixedVector3 v)
    {
        return EulerMultiplyVector3(new FixedVector3(FixedNumber.Zero, FixedNumber.MakeFixNum(yaw, 10000), FixedNumber.Zero), v);
    }

    public static FixedVector3 EulerMultiplyVector3(FixedVector3 euler, FixedVector3 v)
    {
        FixedQuaternion q = FixedQuaternion.Euler(euler.x, euler.y, euler.z);
        return q * v;
    }

    public static FixedNumber CalLogicScaleValue(FixedNumber bValue, FixedNumber minJudgeValue, FixedNumber sValue)
    {
        return FixedMath.Abs(bValue) <= minJudgeValue ?
            FixedNumber.One : sValue / bValue;
    }

    public static FixedVector3 CalLogicScaleValue(FixedVector3 bValue, FixedNumber minJudgeValue, FixedVector3 sValue)
    {
        FixedVector3 res;
        res.x = CalLogicScaleValue(bValue.x, minJudgeValue, sValue.x);
        res.y = CalLogicScaleValue(bValue.y, minJudgeValue, sValue.y);
        res.z = CalLogicScaleValue(bValue.z, minJudgeValue, sValue.z);
        return res;
    }

    #endregion
}

