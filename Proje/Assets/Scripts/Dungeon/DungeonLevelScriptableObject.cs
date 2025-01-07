using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeTypeScriptableObject;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelScriptableObject : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS

    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]

    #endregion Header BASIC LEVEL DETAILS

    #region Tooltip

    [Tooltip("The name for the level")]

    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL

    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]

    #endregion Header ROOM TEMPLATES FOR LEVEL

    #region Tooltip

    [Tooltip("Populate the list with the room templates that you want to be part of the level.  You need to ensure that room templates are included for all room node types that are specified in the Room Node Graphs for the level.")]

    #endregion Tooltip

    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL

    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]

    #endregion Header ROOM NODE GRAPHS FOR LEVEL

    #region Tooltip

    [Tooltip("Populate this list with the room node grpahs which should be randomly selected from for the level.")]

    #endregion Tooltip

    public List<NodeGraphObject> roomNodeGraphList;

    #region Validation

#if UNITY_EDITOR

    // Validate scriptable object details enetered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        // Check to make sure that room templates are specified for all the node types in the
        // specified node graphs

        // First check that north/south corridor, east/west corridor and entrance types have been specified
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        // Loop through all room templates to check for specific room types
        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return; // Exit early if a null template is encountered

            // Check if room type matches specific enum values using 'if' statements
            if (roomTemplateSO.roomNodeType.roomType == RoomType.CorridorEW)
            {
                isEWCorridor = true;
            }

            if (roomTemplateSO.roomNodeType.roomType == RoomType.CorridorNS)
            {
                isNSCorridor = true;
            }

            if (roomTemplateSO.roomNodeType.roomType == RoomType.Entrance)
            {
                isEntrance = true;
            }
        }


        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No E/W Corridor Room Type Specified");
        }

        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No N/S Corridor Room Type Specified");
        }

        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No Entrance Corridor Room Type Specified");
        }

        // Loop through all node graphs
        foreach (NodeGraphObject roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            // Loop through all nodes in node graph
            foreach (NodeScriptableObject roomNodeSO in roomNodeGraph.nodeList)
            {
                if (roomNodeSO == null)
                    continue;

                // Check that a room template has been specified for each roomNode type

             // Corridors and entrance already checked
            if (roomNodeSO.roomNodeType.roomType == RoomType.Entrance || 
                roomNodeSO.roomNodeType.roomType == RoomType.CorridorEW || 
                 roomNodeSO.roomNodeType.roomType == RoomType.CorridorNS || 
                roomNodeSO.roomNodeType.roomType == RoomType.Corridor || 
                roomNodeSO.roomNodeType.roomType == RoomType.None)
                {
    continue;
}

                bool isRoomNodeTypeFound = false;

                // Loop through all room templates to check that this node type has been specified
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {

                    if (roomTemplateSO == null)
                        continue;

                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }

                }

                if (!isRoomNodeTypeFound)
                    Debug.Log("In " + this.name.ToString() + " : No room template " + roomNodeSO.roomNodeType.name.ToString() + " found for node graph " + roomNodeGraph.name.ToString());


            }
        }
    }

#endif

    #endregion Validation
}
