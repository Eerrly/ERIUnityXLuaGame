using System;
using UnityEditor;
using UnityEngine;

public class NodeToolsConnection
{
    public NodeToolsConnectionPoint inPoint;
    public NodeToolsConnectionPoint outPoint;
    public Action<NodeToolsConnection> OnClickRemoveConnection;

    public NodeToolsConnection(NodeToolsConnectionPoint inPoint, NodeToolsConnectionPoint outPoint, Action<NodeToolsConnection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        Handles.DrawBezier(
            inPoint.rect.center, 
            outPoint.rect.center, 
            inPoint.rect.center + Vector2.left * 50f, 
            outPoint.rect.center - Vector2.left * 50f, 
            Color.white, 
            null, 
            2f
        );
        if(Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.SphereHandleCap))
        {
            if(OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }

}
