using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Rendering;
using static NodeTypeScriptableObject;
using System.Linq;
// Fluent metotlar için eklenti sınıfı
public static class GUIStyleExtensions
{
    public static GUIStyle WithBackground(this GUIStyle style, Texture2D texture)
    {
        style.normal.background = texture;
        return style;
    }

    public static GUIStyle WithTextColor(this GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        return style;
    }

    public static GUIStyle WithPadding(this GUIStyle style, int padding)
    {
        style.padding = new RectOffset(padding, padding, padding, padding);
        return style;
    }

    public static GUIStyle WithBorder(this GUIStyle style, int border)
    {
        style.border = new RectOffset(border, border, border, border);
        return style;
    }
}


public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static NodeGraphObject currentNodeGraph;
    private NodeScriptableObject currentNode = null;
    private NodeTypeListScriptableObject nodeTypeList;
    private const float nodeW = 200f;
    private const float nodeH = 100f;
    private const int nodeP = 25;
    private const int nodeB = 12;


    private const float connectingLineWidth = 3f;


    private const float connectingLineArrowSize = 6f;

    [MenuItem("Dungeon Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Dungeon Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Dungeon Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // Subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

        roomNodeStyle = new GUIStyle()
            .WithBackground(EditorGUIUtility.Load("node1") as Texture2D)
            .WithTextColor(Color.black)
            .WithPadding(nodeP)
            .WithBorder(nodeB);

        roomNodeSelectedStyle = new GUIStyle()
       .WithBackground(EditorGUIUtility.Load("node1 on") as Texture2D)
       .WithTextColor(Color.white)
       .WithPadding(nodeP)
       .WithBorder(nodeB);

        // Load Room node types
        nodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // Unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }


    [OnOpenAsset(0)]  // Need the namespace UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        NodeGraphObject nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraphObject;

        if (nodeGraph != null)
        {
            OpenWindow();

            currentNodeGraph = nodeGraph;

            return true;
        }
        return false;
    }


    private void OnGUI()
    {

        if (currentNodeGraph != null)
        {
            DrawDraggedLine();


            ProcessEvents(Event.current);

            DrawConnections();

            DrawNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Draw connections in the graph window between room nodes
    /// </summary>
    private void DrawConnections()
    {
        foreach (var roomNode in currentNodeGraph.nodeList.Where(node => node.childRoomNodeIDList.Count > 0))
        {
            foreach (var childRoomNodeID in roomNode.childRoomNodeIDList.Where(id => currentNodeGraph.roomNodeDictionary.ContainsKey(id)))
            {
                DrawConnectionLine(roomNode, currentNodeGraph.roomNodeDictionary[childRoomNodeID]);
                GUI.changed = true;
            }
        }
    }


    private void DrawConnectionLine(NodeScriptableObject parentRoomNode, NodeScriptableObject childRoomNode)
    {
        // Get line start and end positions
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // Calculate the midpoint of the line
        Vector2 midPoint = (startPosition + endPosition) / 2f;

        // Calculate direction and perpendicular vector
        Vector2 direction = (endPosition - startPosition).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // Define arrowhead size
        float arrowSize = 10f;

        // Calculate arrowhead points at the midpoint
        Vector2 arrowTip = midPoint;
        Vector2 arrowBase1 = midPoint - direction * arrowSize + perpendicular * (arrowSize / 2f);
        Vector2 arrowBase2 = midPoint - direction * arrowSize - perpendicular * (arrowSize / 2f);

        // Draw the main line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        // Draw the arrowhead at the midpoint
        Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase1);
        Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase2);

        // Optionally fill the arrowhead for a solid appearance
        Handles.color = Color.white;
        Handles.DrawAAConvexPolygon(arrowTip, arrowBase1, arrowBase2);

        // Mark GUI as changed
        GUI.changed = true;
    }
    private void DrawDraggedLine()
    {
        if (currentNodeGraph.linePosition != Vector2.zero)
        {
            // Get the start and end positions of the line
            Vector2 startPosition = currentNodeGraph.roomNodeToDrawLineFrom.rect.center;
            Vector2 endPosition = currentNodeGraph.linePosition;

            // Draw the main line
            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

            // Calculate the direction and perpendicular vector for the arrow
            Vector2 direction = (endPosition - startPosition).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            // Define arrowhead size
            float arrowSize = 10f;

            // Calculate arrowhead points at the end position
            Vector2 arrowTip = endPosition;
            Vector2 arrowBase1 = endPosition - direction * arrowSize + perpendicular * (arrowSize / 2f);
            Vector2 arrowBase2 = endPosition - direction * arrowSize - perpendicular * (arrowSize / 2f);

            // Draw the arrowhead at the end position
            Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase1);
            Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase2);

            // Optionally fill the arrowhead for a solid appearance
            Handles.color = Color.white;
            Handles.DrawAAConvexPolygon(arrowTip, arrowBase1, arrowBase2);

            // Mark GUI as changed
            GUI.changed = true;
        }
    }


    private void ProcessEvents(Event e)
    {
        if (currentNode == null || currentNode.isLeftClickDragging == false)
        {
            currentNode = IsMouseOverRoomNode(e);
        }
        if (currentNode == null || currentNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessNodeGraphEvents(e);
        }
        else
        {
            currentNode.ProcessEvents(e);
        }

    }

    private NodeScriptableObject IsMouseOverRoomNode(Event currentEvent)
    {
        return currentNodeGraph.nodeList
            .Find(node => node.rect.Contains(currentEvent.mousePosition));
    }


    private void ProcessNodeGraphEvents(Event e)
    {
        switch (e.type)
        {
            // Process Mouse Down Events
            case EventType.MouseDown:
                ProcessMouseDownEvent(e);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(e); 
                break;  

            case EventType.MouseDrag:
                ProcessMouseDragEvent(e); 
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse down events on the room node graph (not over a node)
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Process right click mouse down on graph event (show context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // Process left mouse down on graph event
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Clear selection from all room nodes
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (NodeScriptableObject roomNode in currentNodeGraph.nodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateNode, mousePosition);
  
        menu.ShowAsContext();
    }

    /// <summary>
    /// Create a room node at the mouse position
    /// </summary>
    private void CreateNode(object mousePositionObject)
    {
        // Assuming mousePositionObject is a Vector2, cast it to Vector2
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // If there are no nodes in the current graph, create an entrance room node first
        if (currentNodeGraph.nodeList.Count == 0)
        {
            CreateNode(mousePosition, nodeTypeList.list.Find(x => x.roomType == RoomType.Entrance));
        }
        else
        {
            // If there is already an entrance, create a regular room node (e.g., Corridor)
            CreateNode(mousePosition, nodeTypeList.list.Find(x => x.roomType == RoomType.None)); // Change to desired default room type
        }
    }
    /// <summary>
    /// Create a room node at the mouse position - overloaded to also pass in RoomNodeType
    /// </summary>
    private void CreateNode(object mousePositionObject, NodeTypeScriptableObject roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // create room node scriptable object asset
        NodeScriptableObject roomNode = ScriptableObject.CreateInstance<NodeScriptableObject>();

        // add room node to current room node graph room node list
        currentNodeGraph.nodeList.Add(roomNode);

        // set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeW, nodeH)), currentNodeGraph, roomNodeType);

        // add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentNodeGraph);

        AssetDatabase.SaveAssets();

        currentNodeGraph.OnValidate();  
    }

    /// <summary>
    /// Process mouse up events
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // Only proceed if the right mouse button was released and a line is being dragged
        if (currentEvent.button != 1 || currentNodeGraph.roomNodeToDrawLineFrom == null)
            return;

        // Attempt to find the room node under the mouse pointer
        NodeScriptableObject targetRoomNode = IsMouseOverRoomNode(currentEvent);

        // If a valid room node is detected
        if (targetRoomNode != null)
        {
            // Add the target as a child of the source node if the relationship is valid
            bool childAdded = currentNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(targetRoomNode.id);

            // If the child was successfully added, link the parent node in the target
            if (childAdded)
            {
                targetRoomNode.AddParentRoomNodeIDToRoomNode(currentNodeGraph.roomNodeToDrawLineFrom.id);
            }
        }

        // Reset the line dragging state
        ClearLineDrag();
    }


    /// <summary>
    /// Reset the line dragging state for the current room node
    /// </summary>
    private void ClearLineDrag()
    {
        if (currentNodeGraph.roomNodeToDrawLineFrom != null)
        {
            currentNodeGraph.roomNodeToDrawLineFrom = null;
        }

        if (currentNodeGraph.linePosition != Vector2.zero)
        {
            currentNodeGraph.linePosition = Vector2.zero;
        }

        // Mark the GUI as needing a redraw
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // process right click drag event - draw line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }

    }

    /// <summary>
    /// Process right mouse drag event - handles drawing a connecting line
    /// </summary>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentNodeGraph.roomNodeToDrawLineFrom == null) return;

        // Update the line position
        currentNodeGraph.linePosition += currentEvent.delta;

        // Mark GUI as changed to trigger a redraw
        GUI.changed = true;
    }


    /// <summary>
    /// Draw room nodes in the graph window
    /// </summary>
    private void DrawNodes()
    {
        // Loop through all room nodes and draw them
        foreach (NodeScriptableObject roomNode in currentNodeGraph.nodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
               
        }

        GUI.changed = true;
    }

/// <summary>
/// Handles selection change in the inspector
/// </summary>
private void InspectorSelectionChanged()
{
    if (Selection.activeObject is NodeGraphObject roomNodeGraph)
    {
        currentNodeGraph = roomNodeGraph;
        GUI.changed = true;
    }
}


}



