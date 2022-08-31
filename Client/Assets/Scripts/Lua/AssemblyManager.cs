using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class AssemblyManager{

    private static readonly Assembly[] assemblies = default(Assembly[]);
    private static readonly Dictionary<string, Type> cacheTypes = new Dictionary<string, Type>();

    static AssemblyManager(){
        assemblies = AppDomain.CurrentDomain.GetAssemblies();
    }

    public static Assembly[] GetAssemblies(){
        return assemblies;
    }

    public static Type[] GetTypesByAssemblies(){
        List<Type> types = new List<Type>();
        foreach(var assembly in assemblies){
            types.AddRange(assembly.GetTypes());
        }
        return types.ToArray();
    }

    public static Type GetType(string typeName){
        if(string.IsNullOrEmpty(typeName)){
            throw new Exception("Type Name Is Invalid!");
        }
        Type type = null;
        if(cacheTypes.TryGetValue(typeName, out type)){
            return type;
        }
        type = Type.GetType(typeName);
        if(type != null){
            return type;
        }
        foreach (var assembly in assemblies)
        {
            type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
            if(type!=null){
                cacheTypes.Add(typeName, type);
                return type;
            }
        }
        return null;
    }

}