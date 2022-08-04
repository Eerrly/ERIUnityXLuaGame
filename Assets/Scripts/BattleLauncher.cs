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
        battleCommonData.players = new BattlePlayerCommonData[] {
            new BattlePlayerCommonData() { camp = 1, pos = 0, level = 1, name = "A" },
            new BattlePlayerCommonData() { isAi = true, camp = 2, pos = 1, level = 1, name = "B" },
        };
    }

    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle(0);
    }

}
