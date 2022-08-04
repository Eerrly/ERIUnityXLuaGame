
using System.Collections.Generic;
using UnityEngine;

public class BattleView : MonoBehaviour
{
    private List<PlayerView> _playerViews = new List<PlayerView>();

    public void InitView(BattleCommonData data)
    {
        InitPlayerView(data);
    }

    private void InitPlayerView(BattleCommonData data)
    {
        for (int i = 0; i < data.players.Length; i++)
        {
            var player = new GameObject(data.players[i].name);
            player.transform.position = new Vector3(i * BattleConstant.normalPlayerPositionOffset, 0, 0);
            PlayerView playerView = player.AddComponent<PlayerView>();
            playerView.Init(i);
            _playerViews.Add(playerView);
        }
    }

    public PlayerView FindPlayerView(int playerId)
    {
        for (int i = 0; i < _playerViews.Count; ++i)
        {
            if (_playerViews[i].playerId == playerId)
                return _playerViews[i];
        }
        return null;
    }

    public void RenderUpdate(BattleEntity battleEntity)
    {
        for (int i = 0; i < _playerViews.Count; i++)
        {
            _playerViews[i].RenderUpdate(battleEntity, i, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in _playerViews)
        {
            if (item) Destroy(item.gameObject);
        }
    }

}
