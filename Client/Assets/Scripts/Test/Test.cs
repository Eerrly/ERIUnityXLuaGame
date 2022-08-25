using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private int[] arr;
    private List<int> intList = new List<int>();

    private void Awake()
    {
        arr = new int[2];
        intList.Add(1);
        //intList.Add(2);
        //intList.Add(3);
    }

    private void Start()
    {
        arr = intList.GetRange(0, Mathf.Min(intList.Count, arr.Length)).ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            Debug.Log("i:" + i + ", value:" + arr[i]);
        }
    }

    private void OnDestroy()
    {
    }

}