using System;
using UnityEngine;
public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T t = null;
        if(null != go)
        {
            t = go.GetComponent<T>();
            if(t == null)
            {
                t = go.AddComponent<T>();
            }
        }
        return t;
    }

    public static void InvokeAttributeCall(object obj, Type classType, bool parengInherit, Type methodType, bool methodInherit)
    {
        if (null != obj)
        {
            var types = obj.GetType().Assembly.GetExportedTypes();
            for (var i = 0; i < types.Length; ++i)
            {
                if (types[i].IsDefined(classType, parengInherit))
                {
                    var methods = types[i].GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    for (var j = 0; j < methods.Length; ++j)
                    {
                        if (methods[j].IsDefined(methodType, methodInherit))
                        {
                            methods[j].Invoke(null, null);
                        }
                    }
                }
            }
        }
    }

    public static int Random(ref uint seed)
    {
        unsafe
        {
            uint next = seed;
            uint result;

            next *= 1103515245;
            next += 12345;
            result = (uint)(next / 65536) % 2048;

            next *= 1103515245;
            next += 12345;
            result <<= 10;
            result ^= (uint)(next / 65536) % 1024;

            next *= 1103515245;
            next += 12345;
            result <<= 10;
            result ^= (uint)(next / 65536) % 1024;

            seed = next;
            return (int)result;
        }
    }

}
