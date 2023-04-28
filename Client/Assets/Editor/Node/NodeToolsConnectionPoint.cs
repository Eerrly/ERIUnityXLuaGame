using System;
using UnityEngine;

/// <summary>
/// 连接类型
/// </summary>
public enum ConnectionPointType { 
    In,
    Out,
}

/// <summary>
/// 连接2个节点
/// </summary>
public class NodeToolsConnectionPoint
{

    public Rect rect;

    public ConnectionPointType type;

    public NodeToolsEditorItem node;

    public GUIStyle style;

    public Action<NodeToolsConnectionPoint> OnClickConnectionPoint;

    public NodeToolsConnectionPoint(NodeToolsEditorItem node, ConnectionPointType type, GUIStyle style, Action<NodeToolsConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10, 20);
    }

    public void Draw()
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }

}
