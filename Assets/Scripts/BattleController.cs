/// <summary>
/// 战斗控制器
/// </summary>
public class BattleController : IBattleController
{
    public int nextFrame;

    /// <summary>
    /// 是否游戏结束
    /// </summary>
    private bool _gameOver = false;

    public BattleEntity battleEntity { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">战斗数据</param>
    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        for (int i = 0; i < data.players.Length; i++)
        {
            PlayerEntity playerEntity = new PlayerEntity();
            playerEntity.Init(data.players[i]);
            battleEntity.playerList.Add(playerEntity);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        _gameOver = false;
    }

    /// <summary>
    /// 逻辑轮询
    /// </summary>
    public override void LogicUpdate()
    {
        try
        {
            if (!Paused)
            {
                UpdateInput();

                var playerList = battleEntity.playerList;

                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.Update(playerList[i], battleEntity);
                }
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.LateUpdate(playerList[i], battleEntity);
                }
                for (int i = 0; i < playerList.Count; i++)
                {
                    PlayerStateMachine.Instance.DoChangeState(playerList[i], battleEntity);
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public void UpdateInput() {
        var input = BattleManager.Instance.GetInput();
        var playerList = battleEntity.playerList;
        for (int i = 0; i < playerList.Count; i++)
        {
#if UNITY_EDITOR && UNITY_DEBUG
            UnityEngine.Debug.Log(string.Format("Player Input ：{0}", input.ToString()));
#endif
            playerList[i].input.yaw = input.yaw - MathManager.YawOffset;
            playerList[i].input.key = input.key;
        }
    }

    /// <summary>
    /// 渲染轮询
    /// </summary>
    public override void RenderUpdate()
    {
        try
        {
            BattleManager.Instance.battleView.RenderUpdate(battleEntity);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public override void Release() { }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public override void GameOver()
    {
        _gameOver = true;
    }

}
