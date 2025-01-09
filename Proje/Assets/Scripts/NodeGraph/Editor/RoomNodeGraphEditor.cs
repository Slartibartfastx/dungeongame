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
    // GUIStyle arka planını ayarlayan eklenti metodu
    public static GUIStyle WithBackground(this GUIStyle style, Texture2D texture)
    {
        style.normal.background = texture;
        return style;
    }
    // GUIStyle metin rengini ayarlayan eklenti metodu
    public static GUIStyle WithTextColor(this GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        return style;
    }
    // GUIStyle padding değerini ayarlayan eklenti metodu
    public static GUIStyle WithPadding(this GUIStyle style, int padding)
    {
        style.padding = new RectOffset(padding, padding, padding, padding);
        return style;
    }
    // GUIStyle border değerini ayarlayan eklenti metodu
    public static GUIStyle WithBorder(this GUIStyle style, int border)
    {
        style.border = new RectOffset(border, border, border, border);
        return style;
    }
}


public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;// Oda düğümü için GUI stili
    private GUIStyle roomNodeSelectedStyle;// Seçili oda düğümü için GUI stili
    private static NodeGraphObject currentNodeGraph;// Şu an düzenlenen düğüm grafiği


    private Vector2 graphOffset; // Grafik ofseti (kaydırma pozisyonu)
    private Vector2 graphDrag;// Grafik sürükleme değeri


    private NodeScriptableObject currentNode = null; // Şu an üzerinde işlem yapılan düğüm
    private NodeTypeListScriptableObject nodeTypeList;// Düğüm tiplerini içeren liste
    private const float nodeW = 200f; // Düğüm genişliği
    private const float nodeH = 100f;// Düğüm yüksekliği
    private const int nodeP = 25;// Düğüm padding değeri
    private const int nodeB = 12; // Düğüm border değeri


    private const float connectingLineWidth = 3f;// Bağlantı çizgisi kalınlığı


    private const float connectingLineArrowSize = 6f;// Bağlantı ok ucu boyutu



    // Grid Spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    // Menüye Dungeon Room Node Graph Editor seçeneğini ekler
    [MenuItem("Dungeon Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Dungeon Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Dungeon Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // Seçim değiştiğinde çağrılacak metodu bağlar
        Selection.selectionChanged += InspectorSelectionChanged;

        // Oda düğümü stili ayarları
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

        // Oda düğümü türlerini yükler
        nodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // Seçim değişikliğine abone olmayı bırakır
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    // Çift tıklama ile grafiği açar
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
            // Arka plan ızgarasını çizer
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // Sürüklenen çizgiyi çizer
            DrawDraggedLine();

            // Olayları işler
            ProcessEvents(Event.current);
            // Bağlantıları çizer
            DrawConnections();

            // Düğümleri çizer
            DrawNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Oda düğüm grafiği için arka plan ızgarası çizer
    /// </summary>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
       
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

       
        graphOffset += graphDrag * 0.5f;

        
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        
        DrawGridLines(verticalLineCount, gridSize, gridOffset, true);

        
        DrawGridLines(horizontalLineCount, gridSize, gridOffset, false);

       
        Handles.color = Color.white;
    }

    /// <summary>
    /// Izgara çizgilerini çizer (dikey veya yatay)
    /// </summary>
    private void DrawGridLines(int lineCount, float gridSize, Vector3 offset, bool isVertical)
    {
        for (int i = 0; i < lineCount; i++)
        {
            // Calculate the start and end positions for the grid line
            Vector3 startPosition = isVertical
                ? new Vector3(gridSize * i, -gridSize, 0) + offset
                : new Vector3(-gridSize, gridSize * i, 0) + offset;
            Vector3 endPosition = isVertical
                ? new Vector3(gridSize * i, position.height + gridSize, 0) + offset
                : new Vector3(position.width + gridSize, gridSize * i, 0) + offset;

            // Draw the line
            Handles.DrawLine(startPosition, endPosition);
        }
    }


    /// <summary>
    /// Oda düğümleri arasındaki bağlantıları çizer
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


    /// <summary>
    /// İki düğüm arasında bir bağlantı çizgisi çizer
    /// </summary>
    private void DrawConnectionLine(NodeScriptableObject parentRoomNode, NodeScriptableObject childRoomNode)
    {
        
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

       
        Vector2 midPoint = (startPosition + endPosition) / 2f;

       
        Vector2 direction = (endPosition - startPosition).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

       
        float arrowSize = 10f;

        
        Vector2 arrowTip = midPoint;
        Vector2 arrowBase1 = midPoint - direction * arrowSize + perpendicular * (arrowSize / 2f);
        Vector2 arrowBase2 = midPoint - direction * arrowSize - perpendicular * (arrowSize / 2f);

        
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        
        Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase1);
        Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase2);

        
        Handles.color = Color.white;
        Handles.DrawAAConvexPolygon(arrowTip, arrowBase1, arrowBase2);

        
        GUI.changed = true;
    }

    /// <summary>
    /// Sürüklenen çizgiyi çizer
    /// </summary>
    private void DrawDraggedLine()
    {
        if (currentNodeGraph.linePosition != Vector2.zero)
        {
           
            Vector2 startPosition = currentNodeGraph.roomNodeToDrawLineFrom.rect.center;
            Vector2 endPosition = currentNodeGraph.linePosition;

           
            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

       
            Vector2 direction = (endPosition - startPosition).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float arrowSize = 10f;

       
            Vector2 arrowTip = endPosition;
            Vector2 arrowBase1 = endPosition - direction * arrowSize + perpendicular * (arrowSize / 2f);
            Vector2 arrowBase2 = endPosition - direction * arrowSize - perpendicular * (arrowSize / 2f);

  
            Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase1);
            Handles.DrawAAPolyLine(connectingLineWidth, arrowTip, arrowBase2);

            Handles.color = Color.white;
            Handles.DrawAAConvexPolygon(arrowTip, arrowBase1, arrowBase2);

    
            GUI.changed = true;
        }
    }


    private void ProcessEvents(Event e)
    {
        graphDrag = Vector2.zero;

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

    private void SelectAllRoomNodes()
    {
        foreach (NodeScriptableObject roomNode in currentNodeGraph.nodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }
    private void CreateNode(object mousePositionObject)
    {
        // mousePositionObject'in bir Vector2 olduğunu varsayarak, bunu Vector2'ye dönüştür
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // Eğer mevcut grafikte hiç düğüm yoksa, ilk olarak bir giriş odası düğümü oluştur
        if (currentNodeGraph.nodeList.Count == 0)
        {
            CreateNode(mousePosition, nodeTypeList.list.Find(x => x.roomType == RoomType.Entrance));
        }
        else
        {
            // Eğer zaten bir giriş odası varsa, sıradan bir oda düğümü oluştur (örneğin, Koridor)
            CreateNode(mousePosition, nodeTypeList.list.Find(x => x.roomType == RoomType.None)); // Varsayılan oda türünü burada değiştirebilirsiniz
        }
    }
    /// <summary>
    /// Fare pozisyonunda bir oda düğümü oluşturur - ayrıca RoomNodeType geçmek için aşırı yüklenmiş bir yöntem
    /// </summary>
    private void CreateNode(object mousePositionObject, NodeTypeScriptableObject roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

    
        NodeScriptableObject roomNode = ScriptableObject.CreateInstance<NodeScriptableObject>();

        
        currentNodeGraph.nodeList.Add(roomNode);

     
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeW, nodeH)), currentNodeGraph, roomNodeType);

      
        AssetDatabase.AddObjectToAsset(roomNode, currentNodeGraph);

        AssetDatabase.SaveAssets();

        currentNodeGraph.OnValidate();  
    }


    /// <summary>
    /// Seçilen oda düğümlerini sil
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // Seçili olan ve türü "Giriş" olmayan düğümleri filtrele
        var selectedNodes = currentNodeGraph.nodeList
            .Where(roomNode => roomNode.isSelected && roomNode.roomNodeType.roomType != NodeTypeScriptableObject.RoomType.Entrance)
            .ToList();

        // Her bir seçili oda düğümünü işle
        foreach (NodeScriptableObject roomNode in selectedNodes)
        {
            // Çocuk düğümleri işle
            foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
            {
                NodeScriptableObject childRoomNode = currentNodeGraph.GetRoomNode(childRoomNodeID);
                childRoomNode?.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
            }

            // Ebeveyn düğümleri işle
            foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
            {
                NodeScriptableObject parentRoomNode = currentNodeGraph.GetRoomNode(parentRoomNodeID);
                parentRoomNode?.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
            }

            // Oda düğümünü sil
            currentNodeGraph.roomNodeDictionary.Remove(roomNode.id);
            currentNodeGraph.nodeList.Remove(roomNode);
            DestroyImmediate(roomNode, true); // Varlık veri tabanından kaldır
        }

        // Silme işleminden sonra varlıkları kaydet
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Seçilen oda düğümleri arasındaki bağlantıları sil
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
       
        var selectedNodesWithChildren = currentNodeGraph.nodeList
            .Where(roomNode => roomNode.isSelected && roomNode.childRoomNodeIDList.Any())
            .ToList();

       
        foreach (NodeScriptableObject roomNode in selectedNodesWithChildren)
        {
            foreach (string childRoomNodeID in roomNode.childRoomNodeIDList.ToList()) 
            {
                NodeScriptableObject childRoomNode = currentNodeGraph.GetRoomNode(childRoomNodeID);

                if (childRoomNode?.isSelected == true)
                {
                    roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                    childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                }
            }
        }

        
        ClearAllSelectedRoomNodes();
    }


    /// <summary>
    /// Fare yukarı olaylarını işleyin
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
    /// Mevcut oda düğümü için çizgi sürükleme durumunu sıfırla
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

    /// <summary>
    /// Fare sürükleme olayını işleyin
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // process right click drag event - draw line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // process left click drag event - drag node graph
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// Sağ tıklama sürükleme olayını işleyin - bağlantı çizgisi çizmeyi ele alın
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
    /// Sol tıklama sürükleme olayını işleyin - oda düğüm grafiğini sürükleyin
    /// </summary>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        // Iterate through all the room nodes in the node graph and drag each node
        foreach (NodeScriptableObject roomNode in currentNodeGraph.nodeList)
        {
            roomNode.DragNode(dragDelta);
        }

        // Indicate that the GUI has been modified
        GUI.changed = true;
    }



    /// <summary>
    /// Grafikte oda düğümlerini çizin
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
    /// Inspector'daki seçim değişikliğini ele alın
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



