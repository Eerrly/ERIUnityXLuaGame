using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;

public class Test : MonoBehaviour
{
    BattleNetController battleNetController;
    bool isSendMsg = false;
    Thread thread;

    private void Awake()
    {
        battleNetController = new BattleNetController();
        battleNetController.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        battleNetController.Connect("192.168.31.219", 10086);
        thread = new Thread(new ThreadStart(() => {
            if (battleNetController.IsConnected)
            {
                if (!isSendMsg)
                {
                    battleNetController.SendReadyMsg();
                    isSendMsg = true;
                }
                battleNetController.Update();
            }
            Thread.Sleep(1);
        }));
    }

    private void OnDestroy()
    {
        thread.Abort();
    }

}
