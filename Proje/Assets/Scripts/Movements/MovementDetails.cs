using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]
public class MovementDetails : ScriptableObject
{
    
    public float minMovSpeed = 8f;
   
    public float maxMovSpeed = 8f;
    
    public float rollSpeed; // for player
    
    public float rollDistance; // for player

    public float rollCooldownTime; // for player

    /// <summary>
    /// Get a random movement speed between the minimum and maximum values
    /// </summary>
    public float GetMoveSpeed()
    {
        if (minMovSpeed == maxMovSpeed)
        {
            return minMovSpeed;
        }
        else
        {
            return Random.Range(minMovSpeed, maxMovSpeed);
        }
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMovSpeed), minMovSpeed, nameof(maxMovSpeed), maxMovSpeed, false);

        /*if (rollDistance != 0f || rollSpeed != 0 || rollCooldownTime != 0)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollCooldownTime), rollCooldownTime, false);
        }*/

    }

#endif
    #endregion Validation
}
