using System.Collections;
using UnityEngine;

public class BattleLauncher : MonoBehaviour
{
    private BattleCommonData battleCommonData;
    private void Awake()
    {
        battleCommonData = new BattleCommonData();
        battleCommonData.mode = 1;
        var battlePlayerCommonData = new BattlePlayerCommonData() { level = 1, name = "sb" };
        battleCommonData.players = new BattlePlayerCommonData[] { battlePlayerCommonData };
    }

    void Start()
    {
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle();
    }

}
