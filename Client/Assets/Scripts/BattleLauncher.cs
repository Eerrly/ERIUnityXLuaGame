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
        battleCommonData.players = new BattlePlayerCommonData[2];
        for (int i = 0; i < battleCommonData.players.Length; i++)
        {
            BattlePlayerCommonData data = new BattlePlayerCommonData();
            data.pos = i;
            data.level = 1;
            if (i == 0)
            {
                data.camp = 1;
                data.name = "Player";
            }
            else
            {
                data.camp = 2;
                data.name = "Enemy";
            }
            battleCommonData.players[i] = data;
        }
    }

    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(battleCommonData);
        BattleManager.Instance.StartBattle(0);
    }

}
