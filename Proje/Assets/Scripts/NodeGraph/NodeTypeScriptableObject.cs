using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node Type", menuName = "Scriptable Objects/Dungeon/ Node Type")]

public class NodeTypeScriptableObject : ScriptableObject
{
    public string NodeTypeName;

    // Whether to display the room node in the node graph editor
    public bool displayInNodeGraphEditor = true;

    // Enum to define different room types
    public RoomType roomType;

    // Validation to check if the roomNodeTypeName is empty
    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(NodeTypeName), NodeTypeName);
    }
#endif
    #endregion

    // Enum representing different room types
    public enum RoomType
    {
        None,        // Unassigned room type
        Corridor,    // Corridor room type
        CorridorNS,  // North-South Corridor
        CorridorEW,  // East-West Corridor
        Entrance,    // Entrance room
        BossRoom    // Boss room

    }
}
