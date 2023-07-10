using UnityEngine;

public class Test : MonoBehaviour
{

    int b = 1;

    public void Start()
    {
        Method1(b);
        Debug.Log(b);
    }

    public void Method1(int a)
    {
        a = 2;
    }

}
