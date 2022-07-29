
public class BattleController : IBattleController
{
    public BattleController(BattleCommonData data)
    {
        battleEntity = new BattleEntity();
        for (int i = 0; i < data.players.Length; i++)
        {
            PlayerEntity playerEntity = new PlayerEntity();
            battleEntity.playerList.Add(playerEntity);
        }
    }

    private bool _gameOver = false;

    public BattleEntity battleEntity { get; set; }

    public override void Initialize()
    {
        _gameOver = false;
    }

    public override void LogicUpdate()
    {
        try
        {
            var playerList = battleEntity.playerList;
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerStateMachine.Instance.Update(playerList[i]);
            }
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerStateMachine.Instance.LateUpdate(playerList[i]);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
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

}
