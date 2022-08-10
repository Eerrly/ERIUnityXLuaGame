using System;
using System.Collections.Generic;
using UnityEngine;

public class SupportLauncher : MonoBehaviour
{
#if UNITY_DEBUG
    [Header("扇形范围检测")]
    [SerializeField] float sectorCheckAngle;
    [SerializeField] float sectorCheckTheta;
    [Header("ONGUI文本高度")]
    [SerializeField] int nGuiLabelHeight = 20;
    private void OnGUI()
    {
        int index = 1;
        PlayerEntity playerEntity = (BattleManager.Instance.battle as BattleController).battleEntity.selfPlayerEntity;
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight),
            string.Format("[player]\tid:{0}", playerEntity.ID));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight),
            string.Format("[input]\tyaw:{0}, key:{1}", playerEntity.input.yaw, playerEntity.input.key));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight),
            string.Format("[state]\t{0}", Enum.GetName(typeof(EPlayerState), playerEntity.curStateId)));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight),
            string.Format("[anim]\tanimId:{0}, normalizedTime:{1}", Enum.GetName(typeof(EAnimationID), playerEntity.animation.animId), playerEntity.animation.normalizedTime));
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight),
            string.Format("[move]\tposition:{0}, rotation:{1}", MathManager.ToVector3(playerEntity.movement.position).ToString(), MathManager.ToQuaternion(playerEntity.movement.rotation).ToString()));

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
        GUI.Label(new Rect(20, index++ * nGuiLabelHeight, 1024, nGuiLabelHeight), strCellInfo);
    }

    private void DrawSelfEntity()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.battle == null)
            return;
        PlayerEntity playerEntity = (BattleManager.Instance.battle as BattleController).battleEntity.selfPlayerEntity;
        List<Cell> aroundCellList = SpacePartition.GetAroundCellList(playerEntity);
        for (int i = 0; i < aroundCellList.Count; i++)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(aroundCellList[i].bounds.center, aroundCellList[i].bounds.size);
            Gizmos.color = new Color(1, 0, 0);
            for (int j = 0; j < aroundCellList[i].entities.Count; j++)
            {
                Gizmos.DrawLine(MathManager.ToVector3(playerEntity.transform.pos), MathManager.ToVector3(aroundCellList[i].entities[j].transform.pos));
            }
        }

        Gizmos.color = Color.green;
        Vector3 position = MathManager.ToVector3(playerEntity.transform.pos);
        Vector3 forward = MathManager.ToQuaternion(playerEntity.transform.rot) * Vector3.forward;
        DrawWireSemicircle(position, forward, PlayerPropertyConstant.AttackDistance, 45, Vector3.up);
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

    private void OnDrawGizmos()
    {
        DrawSelfEntity();
        DrawCellList();
    }
#endif
}
