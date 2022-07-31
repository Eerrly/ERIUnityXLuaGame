using UnityEngine;

/// <summary>
/// 输入管理器
/// </summary>
public static class InputManager
{
    /// <summary>
    /// 开关
    /// </summary>
    public static bool enabled = true;

    /// <summary>
    /// 默认的按键状态 （J、K、L）
    /// </summary>
    public static bool[] defaultKeies = new bool[] { false, false, false, false };

    /// <summary>
    /// 纵向
    /// </summary>
    public static float Vertical
    {
        get
        {
            if (!enabled)
                return 0;
            return Input.GetAxisRaw(InputConstant.Vertical);
        }
    }

    /// <summary>
    /// 横向
    /// </summary>
    public static float Horizontal
    {
        get
        {
            if (!enabled)
                return 0;
            return Input.GetAxisRaw(InputConstant.Horizontal);
        }
    }

    /// <summary>
    /// 是否按键
    /// </summary>
    /// <param name="name">键名</param>
    /// <returns>是否按键</returns>
    public static bool GetKey(string name)
    {
        if (!enabled)
            return false;
        var state = Input.GetKey(name);
        if (!state)
        {
            if (InputConstant.KeyCodeSpace.Equals(name))
            {
                state = defaultKeies[0];
            }
            else if (InputConstant.KeyCodeJ.Equals(name))
            {
                state = defaultKeies[1];
            }
            else if (InputConstant.KeyCodeK.Equals(name))
            {
                state = defaultKeies[2];
            }
            else if (InputConstant.KeyCodeL.Equals(name))
            {
                state = defaultKeies[3];
            }
        }
        return state;
    }

    public static void Reset()
    {
        for (var i = 0; i < defaultKeies.Length; ++i)
        {
            defaultKeies[i] = false;
        }
    }

}
