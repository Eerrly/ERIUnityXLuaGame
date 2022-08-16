using UnityEngine;

public static class InputManager
{

    public static bool enabled = true;

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
