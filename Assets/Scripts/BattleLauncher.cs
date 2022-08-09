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
            new BattlePlayerCommonData() { camp = 1, pos = 0, level = 1, name = "Player" },
            new BattlePlayerCommonData() { camp = 2, pos = 1, level = 1, name = "Enemy1" },
            new BattlePlayerCommonData() { camp = 2, pos = 2, level = 1, name = "Enemy2" },
            new BattlePlayerCommonData() { camp = 2, pos = 3, level = 1, name = "Enemy3" },
            new BattlePlayerCommonData() { camp = 2, pos = 4, level = 1, name = "Enemy4" },
            new BattlePlayerCommonData() { camp = 2, pos = 5, level = 1, name = "Enemy5" },
        };
    }

    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle(0);
    }

}
