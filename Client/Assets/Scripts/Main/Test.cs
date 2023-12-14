using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using UnityEngine;

public class Test : MonoBehaviour
{
    public class TestData
    {
        public List<TestDataItem> items;
    }
    
    [System.Serializable]
    public class TestDataItem
    {
        public uint hash;
        public List<uint> dependencies;
        // public uint offset;
        // public int size;
        // public bool directories;
        // public string extension;
        // public string packageResourcePath;
        // public bool isPatching;
        // public string md5;
    }

    public void Start()
    {
        TestData t1 = new TestData();
        t1.items = new List<TestDataItem>();
        var defaultDependencies = new List<uint>();
        t1.items.Add(new TestDataItem(){ hash = 1193447771, dependencies = defaultDependencies});
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(t1);
        Debug.Log("json:" + json);
        var t = Newtonsoft.Json.JsonConvert.DeserializeObject<TestData>(json);
        Debug.Log("count:" + t.items.Count);
    }

}
