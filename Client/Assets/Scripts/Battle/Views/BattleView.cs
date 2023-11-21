
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleView : MonoBehaviour
{
    private readonly List<PlayerView> _playerViews = new List<PlayerView>();

    public void InitView(BattleCommonData data)
    {
        var localScale = transform.localScale;
        var spaceX = localScale.x * BattleConstant.SpaceX;
        var spaceZ = localScale.z * BattleConstant.SpaceZ;
        SpacePartition.Init(spaceX, spaceZ, BattleConstant.CellSize);
        InitEntityView(data);
    }

    private void InitEntityView(BattleCommonData data)
    {
        for (var i = 0; i < data.players.Length; i++)
        {
            var player = new GameObject($"Player:{data.players[i].pos}");
            player.transform.position = BattleConstant.InitPlayerPos[i];
            player.transform.rotation = BattleConstant.InitPlayerRot[i];

            var playerView = player.AddComponent<PlayerView>();
            playerView.Init(data.players[i]);
            _playerViews.Add(playerView);
        }
        BattleManager.Instance.cameraControl.m_Targets = _playerViews.ToArray();
    }

    public PlayerView FindPlayerView(int playerId)
    {
        return _playerViews.FirstOrDefault(t => t.entityId == playerId);
    }

    public void RenderUpdate(BattleEntity battleEntity)
    {
        foreach (var t in _playerViews)
        {
            t.RenderUpdate(battleEntity, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in _playerViews.Where(item => item))
        {
            Destroy(item.gameObject);
        }
    }

}
