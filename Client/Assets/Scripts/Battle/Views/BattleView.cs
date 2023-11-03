
using System.Collections.Generic;
using UnityEngine;

public class BattleView : MonoBehaviour
{
    private List<PlayerView> _playerViews = new List<PlayerView>();

    public void InitView(BattleCommonData data)
    {
        var spaceX = transform.localScale.x * BattleConstant.spaceX;
        var spaceZ = transform.localScale.z * BattleConstant.spaceZ;
        SpacePartition.Init(spaceX, spaceZ, BattleConstant.cellSize);
        InitEntityView(data);
    }

    private void InitEntityView(BattleCommonData data)
    {
        for (int i = 0; i < data.players.Length; i++)
        {
            var player = new GameObject(string.Format("Player:{0}", data.players[i].pos));
            player.transform.position = BattleConstant.InitPlayerPos[i];
            player.transform.rotation = BattleConstant.InitPlayerRot[i];

            PlayerView playerView = player.AddComponent<PlayerView>();
            playerView.Init(data.players[i]);
            _playerViews.Add(playerView);
        }
        BattleManager.Instance.cameraControl.m_Targets = _playerViews.ToArray();
    }

    public PlayerView FindPlayerView(int playerId)
    {
        for (int i = 0; i < _playerViews.Count; ++i)
        {
            if (_playerViews[i].entityId == playerId)
                return _playerViews[i];
        }
        return null;
    }

    public void RenderUpdate(BattleEntity battleEntity)
    {
        for (int i = 0; i < _playerViews.Count; i++)
        {
            _playerViews[i].RenderUpdate(battleEntity, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in _playerViews)
        {
            if (item)
            {
                Destroy(item.gameObject);
            }
        }
    }

}
