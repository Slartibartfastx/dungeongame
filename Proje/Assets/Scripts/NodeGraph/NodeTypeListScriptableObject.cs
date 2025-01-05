using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NodeTypeListScriptableObject", menuName = "Scriptable Objects/Dungeon/Node Type List")]

public class NodeTypeListScriptableObject : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Space(10)]
    [Header("ROOM NODE TYPE LIST")]
    #endregion
    #region Tooltip
    [Tooltip("This list should be populated with all the NodeTypeScriptableObject for the game - it is used instead of an enum")]
    #endregion
    public List<NodeTypeScriptableObject> list;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}
