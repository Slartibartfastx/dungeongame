using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using static NodeTypeScriptableObject;
using System.Linq;
public class NodeScriptableObject : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public NodeGraphObject roomNodeGraph;
    public NodeTypeScriptableObject roomNodeType;
    [HideInInspector] public NodeTypeListScriptableObject roomNodeTypeList;


#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
    public void Initialise(Rect rect, NodeGraphObject nodeGraph, NodeTypeScriptableObject roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }


    /// <summary>
    /// Dü?ümü dü?üm stili ile çizin
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
{
    
    GUILayout.BeginArea(rect, nodeStyle);

    
    EditorGUI.BeginChangeCheck();

    
    if (parentRoomNodeIDList.Count > 0 || roomNodeType.roomType == NodeTypeScriptableObject.RoomType.Entrance)
    {
        
        EditorGUILayout.LabelField(roomNodeType.NodeTypeName);  
    }
    else
    {
       
        List<NodeTypeScriptableObject> visibleRoomTypes = roomNodeTypeList.list.FindAll(x => x.displayInNodeGraphEditor);

      
        int selected = visibleRoomTypes.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay(visibleRoomTypes));


           
            if (childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in childRoomNodeIDList.ToList()) 
                {
                    
                    NodeScriptableObject childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeID);

                 
                    if (childRoomNode != null)
                    {
                        
                        RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                    }
                }
            }


            
            if (selection != selected && selection >= 0 && selection < visibleRoomTypes.Count)
        {
            roomNodeType = visibleRoomTypes[selection];
        }
    }

    if (EditorGUI.EndChangeCheck())
        EditorUtility.SetDirty(this);  

    GUILayout.EndArea();
}

public string[] GetRoomNodeTypesToDisplay(List<NodeTypeScriptableObject> visibleRoomTypes)
{
    List<string> roomTypeNames = new List<string>();


    foreach (var nodeType in visibleRoomTypes)
    {
        roomTypeNames.Add(nodeType.NodeTypeName);
    }

    return roomTypeNames.ToArray();
}

    public void ProcessEvents(Event currentEvent)
    {
       
        var eventHandlers = new Dictionary<EventType, Action<Event>>()
    {
        { EventType.MouseDown, ProcessMouseDownEvent },
        { EventType.MouseUp, ProcessMouseUpEvent },
        { EventType.MouseDrag, ProcessMouseDragEvent }
    };

        
        if (eventHandlers.ContainsKey(currentEvent.type))
        {
            eventHandlers[currentEvent.type].Invoke(currentEvent);
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        HandleMouseEvent(currentEvent, "down");
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        HandleMouseEvent(currentEvent, "up");
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        HandleMouseEvent(currentEvent, "drag");
    }

    private void HandleMouseEvent(Event currentEvent, string eventType)
    {
        switch (currentEvent.button)
        {
            case 0: 
                if (eventType == "down")
                    ProcessLeftClickDownEvent();
                else if (eventType == "up")
                    ProcessLeftClickUpEvent();
                else if (eventType == "drag")
                    ProcessLeftMouseDragEvent(currentEvent);
                break;

            case 1: // Right click
                if (eventType == "down")
                    ProcessRightClickDownEvent(currentEvent);
                break;

            default:
                break;
        }
    }


    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        isSelected = !isSelected;
    }


    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

  
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

   
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

  
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }



    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }



    public bool IsChildRoomValid(string childID)
    {
        // Check if there is already a connected BossRoom in the node graph using LINQ
        bool isConnectedBossNodeAlready = roomNodeGraph.nodeList
            .Any(node => node.roomNodeType.roomType == NodeTypeScriptableObject.RoomType.BossRoom
                         && node.parentRoomNodeIDList.Count > 0);

        NodeScriptableObject childNode = roomNodeGraph.GetRoomNode(childID);
        NodeTypeScriptableObject.RoomType childRoomType = childNode.roomNodeType.roomType;

        // Use a set of conditional checks with early exits
        if (childRoomType == NodeTypeScriptableObject.RoomType.BossRoom && isConnectedBossNodeAlready)
            return false;

        if (childRoomType == NodeTypeScriptableObject.RoomType.None)
            return false;

        if (childRoomNodeIDList.Contains(childID))
            return false;

        if (id == childID)
            return false;

        if (parentRoomNodeIDList.Contains(childID))
            return false;

        if (childNode.parentRoomNodeIDList.Count > 0)
            return false;

        if (childRoomType == NodeTypeScriptableObject.RoomType.Corridor
            && roomNodeType.roomType == NodeTypeScriptableObject.RoomType.Corridor)
            return false;

        if (childRoomType != NodeTypeScriptableObject.RoomType.Corridor
            && roomNodeType.roomType != NodeTypeScriptableObject.RoomType.Corridor)
            return false;

        if (childRoomType == NodeTypeScriptableObject.RoomType.Corridor
            && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        if (childRoomType == NodeTypeScriptableObject.RoomType.Entrance)
            return false;

        if (childRoomType != NodeTypeScriptableObject.RoomType.Corridor
            && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

  
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        return childRoomNodeIDList.Remove(childID);
    }

  
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        return parentRoomNodeIDList.Remove(parentID);
    }


#endif

}
