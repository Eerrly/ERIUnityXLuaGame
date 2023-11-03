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
    /// 默认按键状态
    /// </summary>
    public static bool[] defaultKeies = new bool[] { false, false, false, false };

    public static float Vertical
    {
        get
        {
            if (!enabled)
                return 0;
            return Input.GetAxisRaw(InputConstant.Vertical);
        }
    }

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
    /// 获取对应键位的状态
    /// </summary>
    /// <param name="name">键位名称</param>
    /// <returns>状态</returns>
    public static bool GetKey(string name)
    {
        if (!enabled)
            return false;
        var state = Input.GetKey(name);
        if (!state)
        {
            switch (name)
            {
                case InputConstant.KeyCodeSpace:
                    state = defaultKeies[0];
                    break;
                case InputConstant.KeyCodeJ:
                    state = defaultKeies[1];
                    break;
                case InputConstant.KeyCodeK:
                    state = defaultKeies[2];
                    break;
                case InputConstant.KeyCodeL:
                    state = defaultKeies[3];
                    break;
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
