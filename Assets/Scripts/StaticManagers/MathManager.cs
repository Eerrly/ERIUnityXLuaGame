using UnityEngine;

public static class MathManager
{
    public static readonly float AngleMax = 360.0f;
    public static readonly float HalfAngleMax = 180.0f;
    public const int YawOffset = 1;
    public const int YawStop = -YawOffset;
    public static readonly float DivAngle = 45.0f;
    public static readonly float HalfDivAngle = 22.5f;

    public static float[] Vector3Zero => new float[] { 0.0f, 0.0f, 0.0f };

    public static float[] QuaternionIdentity => new float[] { 0.0f, 0.0f, 0.0f, 1.0f };

    public static int Format8DirInput(Vector3 input)
    {
        input.y = 0.0f;
        if(input.sqrMagnitude > 0)
        {
            Vector3 dir = input.normalized;
            float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);

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
                div = (int)val;
            }
            return div + YawOffset;
        }
        return 0;
    }

    public static Quaternion FromYaw(int yaw)
    {
        Quaternion r = Quaternion.Euler(0.0f, (yaw) * DivAngle, 0.0f);
        return r;
    }

    private static Vector3[] _cacheYawToVector3 = new Vector3[24];
    private static Vector3 _FromYawToVector3(int yaw)
    {
        switch (yaw)
        {
            case 0: return new Vector3(0.0f, 0.0f, 1.0f);
            case 2: return new Vector3(1.0f, 0.0f, 0.0f);
            case 4: return new Vector3(0.0f, 0.0f, -1.0f);
            case 6: return new Vector3(-1.0f, 0.0f, 0.0f);
        }
        Quaternion rot = FromYaw(yaw);
        return rot * Vector3.forward;
    }

    public static Vector3 FromYawToVector3(int yaw)
    {
        if (yaw < 0)
        {
            return Vector3.zero;
        }
        yaw = yaw % 8;
        if (_cacheYawToVector3[yaw] == Vector3.zero)
        {
            _cacheYawToVector3[yaw] = _FromYawToVector3(yaw);
        }
        return _cacheYawToVector3[yaw];
    }

    public static float GetYawAngle(int yaw)
    {
        return yaw * DivAngle;
    }

    public static Vector3 ToVector3(float[] pos)
    {
        return new Vector3(pos[0], pos[1], pos[2]);
    }

    public static Quaternion ToQuaternion(float[] rot)
    {
        return new Quaternion(rot[0], rot[1], rot[2], rot[3]);
    }

    public static float[] ToFloat2(Vector2 vector)
    {
        return new float[2] { vector.x, vector.y };
    }

    public static float[] ToFloat3(Vector3 vector)
    {
        return new float[3] { vector.x, vector.y, vector.z };
    }

    public static float[] ToFloat4(Quaternion quaternion)
    {
        return new float[4] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
    }

}
