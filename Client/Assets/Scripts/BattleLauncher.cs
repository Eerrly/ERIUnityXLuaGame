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
        battleCommonData.players = new BattlePlayerCommonData[] {
            new BattlePlayerCommonData() { pos = 0 },
            new BattlePlayerCommonData() { pos = 1 }
        };
    }

    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle(BattleConstant.SelfID);
    }

}
