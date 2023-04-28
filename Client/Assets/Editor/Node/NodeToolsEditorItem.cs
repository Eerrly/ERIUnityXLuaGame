using UnityEngine;
using UnityEditor;
using System;

public class NodeToolsEditorItem
{

    public Rect rect;
    public string title = "node item";

    public bool isDragged;
    public bool isSelected;

    public NodeToolsConnectionPoint inPoint;
    public NodeToolsConnectionPoint outPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<NodeToolsEditorItem> OnRemoveNode;

    public NodeToolsEditorItem(
        Vector2 position, float width, float height, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<NodeToolsConnectionPoint> OnClickInPoint, Action<NodeToolsConnectionPoint> OnClickOutPoint, Action<NodeToolsEditorItem> onRemoveNode)
    {
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;
        inPoint = new NodeToolsConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new NodeToolsConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = onRemoveNode;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        GUI.Box(rect, title, style);
        inPoint.Draw();
        outPoint.Draw();
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                // 鼠标左键
                if(e.button == 0)
                {
                    isDragged = rect.Contains(e.mousePosition);
                    isSelected = rect.Contains(e.mousePosition);
                    GUI.changed = true;
                    style = rect.Contains(e.mousePosition) ? selectedNodeStyle : defaultNodeStyle;
                }
                if(e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                isDragged = false;
                break;
            case EventType.MouseDrag:
                if(e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode()
    {
        if(OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }

}
