using UnityEngine;

/// <summary>
/// 战斗启动器
/// </summary>
public class BattleLauncher : MonoBehaviour
{
    private BattleCommonData _battleCommonData;

    private void Awake()
    {
        InitBattleCommonData();
    }

    /// <summary>
    /// 初始化战斗基本数据
    /// </summary>
    void InitBattleCommonData()
    {
        _battleCommonData = new BattleCommonData();
        _battleCommonData.players = new BattlePlayerCommonData[] {
            new BattlePlayerCommonData() { pos = 0 },
            new BattlePlayerCommonData() { pos = 1 }
        };
    }

    /// <summary>
    /// 开始
    /// </summary>
    void Start()
    {
        BattleManager.Instance.Initialize();
        BattleManager.Instance.SetBattleData(_battleCommonData);
        BattleManager.Instance.StartBattle(BattleConstant.SelfID);
    }

}
