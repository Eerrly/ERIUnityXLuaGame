using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class NodeToolsEditorWin : EditorWindow
{
    /// <summary>
    /// 子节点集合
    /// </summary>
    private List<NodeToolsEditorItem> nodes;
    /// <summary>
    /// 连线集合
    /// </summary>
    private List<NodeToolsConnection> connections;

    /// <summary>
    /// 子节点风格
    /// </summary>
    private GUIStyle nodeStyle;

    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;
    private GUIStyle selectedNodeStyle;

    private NodeToolsConnectionPoint selectedInPoint;
    private NodeToolsConnectionPoint selectedOutPoint;

    public static NodeToolsEditorWin Open()
    {
        NodeToolsEditorWin window = GetWindow<NodeToolsEditorWin>();
        window.titleContent = new GUIContent("Node Tools Editor");
        return window;
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);
    }

    private void OnGUI()
    {
        DrawNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// 绘制节点
    /// </summary>
    private void DrawNodes()
    {
        if(nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                nodes[i]?.Draw();
            }
        }
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                connections[i]?.Draw();
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    /// <summary>
    /// 触发的事件
    /// </summary>
    /// <param name="e"></param>
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if(e.button == 1) ProcessContextMenu(e.mousePosition);
                break;
            case EventType.MouseDrag:
                if(e.button == 0) OnDrag(e.delta);
                break;
        }
    }

    private void OnDrag(Vector2 delta)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                nodes[i]?.Drag(delta);
            }
        }
        GUI.changed = true;
    }

    /// <summary>
    /// 节点触发的事件
    /// </summary>
    /// <param name="e"></param>
    private void ProcessNodeEvents(Event e)
    {
        if(nodes != null)
        {
            foreach (var item in nodes)
            {
                GUI.changed = item.ProcessEvents(e);
            }
        }
    }

    /// <summary>
    /// 右键窗口
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    /// <param name="mousePosition"></param>
    private void OnClickAddNode(Vector2 mousePosition)
    {
        if(nodes == null)
        {
            nodes = new List<NodeToolsEditorItem>();
        }
        nodes.Add(new NodeToolsEditorItem(mousePosition, 200, 100, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    private void OnClickInPoint(NodeToolsConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
            }
            ClearConnection();
        }
    }

    private void OnClickOutPoint(NodeToolsConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
            }
            ClearConnection();
        }
    }

    private void OnClickRemoveConnection(NodeToolsConnection connection)
    {
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if(connections == null)
        {
            connections = new List<NodeToolsConnection>();
        }
        connections.Add(new NodeToolsConnection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

    private void ClearConnection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private void OnClickRemoveNode(NodeToolsEditorItem node)
    {
        if (connections != null)
        {
            List<NodeToolsConnection> connectionsToRemove = new List<NodeToolsConnection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }
        nodes.Remove(node);
    }

}
