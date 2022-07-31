public class BattleController : IBattleController
{
    public int nextFrame;

    public BattleEntity battleEntity { get; set; }

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

    public override void Initialize()
    {
    }

    public override void LogicUpdate()
    {
        try
        {
            if (!Paused)
            {
                battleEntity.deltaTime = FrameEngine.frameInterval * battleEntity.timeScale;
                battleEntity.time += battleEntity.deltaTime;

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

    public override void GameOver() { }

}
