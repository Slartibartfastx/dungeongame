using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


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
