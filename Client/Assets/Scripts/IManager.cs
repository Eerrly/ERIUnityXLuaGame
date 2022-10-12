using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{
    bool IsInitialized { get; set; }

    void OnInitialize();

    void OnRelease();

}
