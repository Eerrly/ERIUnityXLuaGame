using System;
using System.Collections.Generic;
using UnityEngine;

public class SupportLauncher : MonoBehaviour
{
#if UNITY_DEBUG
    [Header("扇形范围绘制")]
    [SerializeField] float sectorCheckAngle;
    [SerializeField] float sectorCheckTheta;
    [Header("文本绘制")]
    [SerializeField] int nGuiLabelHeight = 20;
    [SerializeField] int nGuiLabelWidth = 1024;
    private void OnGUI()
    {
        int index = 1;
        BattleEntity battleEntity = BattleManager.Instance.battle.battleEntity;
        PlayerEntity playerEntity = (PlayerEntity)battleEntity.FindEntity(BattleConstant.SelfID);
        GUI.color = Color.green;
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, nGuiLabelWidth, nGuiLabelHeight),
            string.Format("[player]\tid:{0} logicFrame:{1} time:{2:N3}", playerEntity.ID, BattleManager.Instance.logicFrame, battleEntity.time));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, nGuiLabelWidth, nGuiLabelHeight),
            string.Format("[input]\tpos:{0}, yaw:{1}, key:{2}", playerEntity.input.pos, playerEntity.input.yaw, playerEntity.input.key));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, nGuiLabelWidth, nGuiLabelHeight),
            string.Format("[state]\t{0}", Enum.GetName(typeof(EPlayerState), playerEntity.curStateId)));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, nGuiLabelWidth, nGuiLabelHeight),
            string.Format("[move]\tposition:{0}, rotation:{1}", playerEntity.movement.position.ToString(), playerEntity.movement.rotation.ToString()));

        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
        string strCellInfo = "[cell]\t";
        for (int i = 0; i < aroundCellList.Count; i++)
        {
            if (aroundCellList[i].entities.Count == 1 && aroundCellList[i].entities[0].ID == BattleManager.Instance.selfPlayerId)
                continue;
            if (aroundCellList[i].entities.Count > 0)
            {
                strCellInfo += (aroundCellList[i].ToString() + " ");
            }
        }
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, nGuiLabelWidth, nGuiLabelHeight), strCellInfo);
    }

    private void DrawSelfEntity()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.battle == null)
            return;
        PlayerEntity playerEntity = (PlayerEntity)BattleManager.Instance.battle.battleEntity.FindEntity(BattleConstant.SelfID);
        if (playerEntity != null)
        {
            DrawAroundCellList(playerEntity);
        }
    }

    private void DrawCellList()
    {
        List<Cell> cellList = SpacePartition.GetCellList();
        if (cellList == null)
        {
            return;
        }
        for (int i = 0; i < cellList.Count; i++)
        {
            Cell cell = cellList[i];
            if (cell.entities.Count > 0)
            {
                Gizmos.color = new Color(0, 1f, 0, 0.2f);
                Gizmos.DrawCube(cell.bounds.center, cell.bounds.size);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(cell.bounds.center, cell.bounds.size);
            }
        }
    }

    private void DrawWireSemicircle(Vector3 origin, Vector3 direction, float radius, int angle, Vector3 axis)
    {
        Vector3 leftdir = Quaternion.AngleAxis(-angle / 2, axis) * direction;
        Vector3 rightdir = Quaternion.AngleAxis(angle / 2, axis) * direction;

        Vector3 currentP = origin + leftdir * radius;
        Vector3 oldP;
        if (angle != 360)
        {
            Gizmos.DrawLine(origin, currentP);
        }
        for (int i = 0; i < angle / 10; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(10 * i, axis) * leftdir;
            oldP = currentP;
            currentP = origin + dir * radius;
            Gizmos.DrawLine(oldP, currentP);
        }
        oldP = currentP;
        currentP = origin + rightdir * radius;
        Gizmos.DrawLine(oldP, currentP);
        if (angle != 360)
        {
            Gizmos.DrawLine(currentP, origin);
        }
    }

    private void DrawAroundCellList(PlayerEntity playerEntity)
    {
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
        for (int i = 0; i < aroundCellList.Count; i++)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(aroundCellList[i].bounds.center, aroundCellList[i].bounds.size);
            Gizmos.color = new Color(1, 0, 0);
            for (int j = 0; j < aroundCellList[i].entities.Count; j++)
            {
                var entity = aroundCellList[i].entities[j];
                if ((int)entity.curStateId != entity.state.count - 1)
                {
                    Gizmos.DrawLine(playerEntity.transform.pos.ToVector3(), entity.transform.pos.ToVector3());
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawSelfEntity();
        DrawCellList();
    }
#endif
}
