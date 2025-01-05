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

        // Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }


 /// <summary>
/// Draw node with the node style
/// </summary>
public void Draw(GUIStyle nodeStyle)
{
    // Draw Node Box Using Begin Area
    GUILayout.BeginArea(rect, nodeStyle);

    // Start Region To Detect Popup Selection Changes
    EditorGUI.BeginChangeCheck();

    // Check if the node has a parent or is of type entrance, and lock the room type accordingly
    if (parentRoomNodeIDList.Count > 0 || roomNodeType.roomType == NodeTypeScriptableObject.RoomType.Entrance)
    {
        // Display a label that can't be changed for entrance or parent-connected rooms
        EditorGUILayout.LabelField(roomNodeType.NodeTypeName);  // Lock current value and prevent change
    }
    else
    {
        // Filter the list to include only room types that are marked as displayable
        List<NodeTypeScriptableObject> visibleRoomTypes = roomNodeTypeList.list.FindAll(x => x.displayInNodeGraphEditor);

        // Find the current selected room type's index in the filtered list
        int selected = visibleRoomTypes.FindIndex(x => x == roomNodeType);

        // Display the popup with the visible room types only
        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay(visibleRoomTypes));


            // If a room node type has been changed and it already has children, delete the parent-child links since we need to revalidate any
            if (childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in childRoomNodeIDList.ToList()) // Use ToList() to safely iterate over the original list
                {
                    // Get child room node
                    NodeScriptableObject childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeID);

                    // If the child room node is not null
                    if (childRoomNode != null)
                    {
                        // Remove childID from parent room node
                        RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // Remove parentID from child room node
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                    }
                }
            }


            // Only update roomNodeType if the user selects a new type, keep current value if no selection is made
            if (selection != selected && selection >= 0 && selection < visibleRoomTypes.Count)
        {
            roomNodeType = visibleRoomTypes[selection];
        }
    }

    if (EditorGUI.EndChangeCheck())
        EditorUtility.SetDirty(this);  // Mark object as dirty to save changes

    GUILayout.EndArea();
}

/// <summary>
/// Get the room node types to display in the popup.
/// </summary>
public string[] GetRoomNodeTypesToDisplay(List<NodeTypeScriptableObject> visibleRoomTypes)
{
    List<string> roomTypeNames = new List<string>();

    // Iterate through the visible room types and add their names
    foreach (var nodeType in visibleRoomTypes)
    {
        roomTypeNames.Add(nodeType.NodeTypeName);
    }

    // Return as string array
    return roomTypeNames.ToArray();
}

    public void ProcessEvents(Event currentEvent)
    {
        // Define a dictionary to map event types to methods
        var eventHandlers = new Dictionary<EventType, Action<Event>>()
    {
        { EventType.MouseDown, ProcessMouseDownEvent },
        { EventType.MouseUp, ProcessMouseUpEvent },
        { EventType.MouseDrag, ProcessMouseDragEvent }
    };

        // Check if the event type exists in the dictionary and invoke the corresponding method
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
            case 0: // Left click
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

    /// <summary>
    /// Process left click down event
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        isSelected = !isSelected;
    }

    /// <summary>
    /// Process right click down event
    /// </summary>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// Process left click up event
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// Process left mouse drag event
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drag node
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }



    /// <summary>
    /// Add childID to the node (returns true if the node has been added, false otherwise)
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // Check child node can be added validly to parent
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Determines whether a child node can validly be added to the parent node.
    /// Returns true if valid, otherwise returns false.
    /// </summary>
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

    /// <summary>
    /// Add parentID to the node (returns true if the node has been added, false otherwise)
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// Attempts to remove a child ID from the node. Returns true if removed, false otherwise.
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        return childRoomNodeIDList.Remove(childID);
    }

    /// <summary>
    /// Attempts to remove a parent ID from the node. Returns true if removed, false otherwise.
    /// </summary>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        return parentRoomNodeIDList.Remove(parentID);
    }


#endif

}
