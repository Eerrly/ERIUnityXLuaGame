using UnityEngine;

public class BattleLauncher : MonoBehaviour
{
    private BattleCommonData battleCommonData;

    private void Awake()
    {
        InitBattleCommonData();
    }

    void InitBattleCommonData()
    {
        battleCommonData = new BattleCommonData();
        battleCommonData.mode = 1;
        var battlePlayerCommonDataA = new BattlePlayerCommonData() { pos = 0, level = 1, name = "A" };
        var battlePlayerCommonDataB = new BattlePlayerCommonData() { pos = 1, level = 1, name = "B" };
        battleCommonData.players = new BattlePlayerCommonData[] { battlePlayerCommonDataA, battlePlayerCommonDataB };
    }

    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle(0);
    }

}
