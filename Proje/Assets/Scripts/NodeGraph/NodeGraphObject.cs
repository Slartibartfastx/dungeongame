using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static NodeTypeScriptableObject;


[CreateAssetMenu(fileName = "Node Graph", menuName = "Scriptable Objects/Dungeon/Node Graph")]
public class NodeGraphObject : ScriptableObject
{
    [HideInInspector] public NodeTypeListScriptableObject nodeTypeList;
    [HideInInspector] public List<NodeScriptableObject> nodeList = new List<NodeScriptableObject>();
    [HideInInspector] public Dictionary<string, NodeScriptableObject> roomNodeDictionary = new Dictionary<string, NodeScriptableObject>();

    private void Awake()
    {
        LoadRoomNodeDictionary();

    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary = nodeList.ToDictionary(node => node.id);
    }



public NodeScriptableObject GetRoomNode(string roomNodeID)
{
    if (roomNodeDictionary.TryGetValue(roomNodeID, out NodeScriptableObject roomNode))
    {
        return roomNode;
    }
    return null;
}

    public NodeScriptableObject GetRoomNode(RoomType roomType)
    {
        var node = nodeList.FirstOrDefault(x => x.roomNodeType.roomType == roomType);

        if (node != null)
        {
            return node;
        }

        return null;
    }


    public IEnumerable<NodeScriptableObject> GetChildRoomNodes(NodeScriptableObject parentRoomNode)
    {
        return parentRoomNode.childRoomNodeIDList
            .Select(childNodeID => GetRoomNode(childNodeID))
            .Where(childNode => childNode != null); 
    }


    #region Editor Code

    // The following code should only run in the Unity Editor
#if UNITY_EDITOR

    [HideInInspector] public NodeScriptableObject roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;
    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(NodeScriptableObject node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }

#endif

    #endregion Editor Code
}
