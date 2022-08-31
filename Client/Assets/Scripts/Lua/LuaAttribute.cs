using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GenComment : Attribute { }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class NoComment : Attribute { }

public static class CS2LuaCommentGen
{
    public static void Generate(Type[] types, string outputPath)
    {
        if (types == null)
        {
            Debug.Log("null types");
            return;
        }

        StringBuilder strb = new StringBuilder();
        foreach (Type t in types)
        {
            if (t.IsClass)
            {
                object[] genComAttr = t.GetCustomAttributes(typeof(GenComment), false);
                if (genComAttr == null || genComAttr.Length == 0)
                {
                    continue;
                }

                GenClass(strb, t);

                FieldInfo[] fieldInfos = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (fieldInfos != null)
                {
                    SortMembers(fieldInfos);
                    foreach (FieldInfo fieldInfo in fieldInfos)
                    {
                        if (!IsDeclareNoComment(fieldInfo))
                        {
                            GenField(strb, fieldInfo);
                        }
                    }
                }

                PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (propInfos != null)
                {
                    SortMembers(propInfos);
                    foreach (PropertyInfo propInfo in propInfos)
                    {
                        if (!IsDeclareNoComment(propInfo))
                        {
                            GenProperty(strb, propInfo);
                        }
                    }
                }

                MethodInfo[] methodInfos = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);
                if (methodInfos != null)
                {
                    SortMembers(methodInfos);
                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        if (!IsDeclareNoComment(methodInfo))
                        {
                            if (!methodInfo.IsSpecialName)
                            {
                                GenFunc(strb, methodInfo);
                            }
                        }
                    }
                }

                strb.AppendLine();
            }
        }

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        }

        File.WriteAllText(outputPath, strb.ToString());
        Debug.Log(outputPath + " generated.");
    }

    static void GenClass(StringBuilder strb, Type t)
    {
        strb.AppendLine("---@class " + "CS." + t.Name);
    }

    static void GenProperty(StringBuilder strb, PropertyInfo info)
    {
        strb.AppendLine("---@field " + info.Name + " " + GetTypeName(info.PropertyType));
    }

    static void GenField(StringBuilder strb, FieldInfo info)
    {
        strb.AppendLine("---@field " + info.Name + " " + GetTypeName(info.FieldType));
    }

    static void GenFunc(StringBuilder strb, MethodInfo info)
    {
        strb.AppendLine("---@field " + info.Name + " fun(" + GenSelfParam(info) + GenParams(info) + ")" + GenReturn(info));
    }

    static string GenSelfParam(MethodInfo info)
    {
        return !info.IsStatic ? "self:" + GetTypeName(info.DeclaringType) : "";
    }

    static string GenParams(MethodInfo info)
    {
        ParameterInfo[] pas = info.GetParameters();
        if (pas == null || pas.Length == 0)
        {
            return "";
        }
        string result = !info.IsStatic ? "," : "";
        bool isFirst = true;
        foreach (ParameterInfo param in pas)
        {
            if (!isFirst)
            {
                result += ", ";
            }
            result += param.Name + ":" + GetTypeName(param.ParameterType);
            isFirst = false;
        }
        return result;
    }

    static string GenReturn(MethodInfo info)
    {
        Type t = info.ReturnType;
        if (t == null || t == typeof(void))
        {
            return "";
        }
        return ":" + GetTypeName(t);
    }

    static string GetTypeName(Type t)
    {
        if (t.IsClass)
        {
            object[] genComAttr = t.GetCustomAttributes(typeof(GenComment), false);
            if (genComAttr != null && genComAttr.Length > 0)
            {
                return "CS." + t.Name;
            }
        }
        if (t.IsPrimitive || t == typeof(string))
        {
            return t.Name.ToLower();
        }
        if (t.IsArray)
        {
            return GetTypeName(t.GetElementType()) + "[]";
        }
        return t.Name;
    }

    static bool IsDeclareNoComment(MemberInfo info)
    {
        object[] noComAttr = info.GetCustomAttributes(typeof(NoComment), false);
        return noComAttr != null && noComAttr.Length > 0;
    }

    static void SortMembers(MemberInfo[] members)
    {
#if UNITY_EDITOR
        Array.Sort(members, (i1, i2) => EditorUtility.NaturalCompare(i1.Name, i2.Name));
#endif
    }
}
