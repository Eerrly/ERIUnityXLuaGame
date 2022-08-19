
using System.Collections.Generic;
using UnityEngine;

public class BattleView : MonoBehaviour
{
    private List<PlayerView> _playerViews = new List<PlayerView>();

    public void InitView(BattleCommonData data)
    {
        InitPlayerView(data);
        var spaceX = transform.localScale.x * BattleConstant.spaceX;
        var spaceZ = transform.localScale.z * BattleConstant.spaceZ;
        SpacePartition.Init(spaceX, spaceZ, BattleConstant.cellSize);
    }

    private void InitPlayerView(BattleCommonData data)
    {
        for (int i = 0; i < data.players.Length; i++)
        {
            var player = new GameObject(data.players[i].name);
            player.transform.position = new Vector3(Random.insideUnitCircle.x * BattleConstant.normalPlayerPositionOffset, 0, Random.insideUnitCircle.y * BattleConstant.normalPlayerPositionOffset);
            PlayerView playerView = player.AddComponent<PlayerView>();
            playerView.Init(data.players[i]);
            _playerViews.Add(playerView);
        }
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
