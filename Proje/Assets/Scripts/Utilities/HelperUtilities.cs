using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class HelperUtilities
{
    public static Camera mainCam;


    /// <summary>
    /// Get the mouse world position.
    /// </summary>
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCam == null) mainCam = Camera.main;

        Vector3 mouseScreenPosition = Input.mousePosition;

        // Clamp mouse position to screen size
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);

        Vector3 worldPosition = mainCam.ScreenToWorldPoint(mouseScreenPosition);

        worldPosition.z = 0f;

        return worldPosition;

    }

    public static Vector3 getMousePos()
    {
        if (mainCam == null) { mainCam = Camera.main; }
        Vector3 mousePos = Input.mousePosition;

        mousePos.x = Mathf.Clamp(mousePos.x, 0f, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0f, Screen.height);
        
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;

    }

    public static float getAngleFromVec(Vector3 pos)
    {

        float rad = Mathf.Atan2(pos.y, pos.x);
        float deg = rad * Mathf.Rad2Deg;

        return deg;
    }

    public static AimDir getAimdir(float angleDegrees)
    {
        AimDir aimDirection;
        Debug.Log(angleDegrees);
        // Set player direction
        //Up Right
        if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            aimDirection = AimDir.UpRight;
        }
        // Up
        else if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDir.Up;
        }
        // Up Left
        else if (angleDegrees > 112f && angleDegrees <= 158f)
        {
            aimDirection = AimDir.UpLeft;
        }
        // Left
        else if ((angleDegrees <= 180f && angleDegrees > 158f) || (angleDegrees > -180 && angleDegrees <= -135f))
        {
            aimDirection = AimDir.Left;
        }
        // Down
        else if ((angleDegrees > -135f && angleDegrees <= -45f))
        {
            aimDirection = AimDir.Down;
        }
        // Right
        else if ((angleDegrees > -45f && angleDegrees <= 0f) || (angleDegrees > 0 && angleDegrees < 22f))
        {
            aimDirection = AimDir.Right;
        }
        else
        {
            aimDirection = AimDir.Right;
        }
        Debug.Log(aimDirection);
        return aimDirection;


    }
    /// <summary>
    /// string null ya da empty mi kontrol eder
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (string.IsNullOrEmpty(stringToCheck))
        {
            Debug.LogWarning($"{fieldName} is empty or null and must contain a value in object {thisObject.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// objenin belli bi kismi bos mu degil mi kontrol eder
    /// </summary>
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is or null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the enumerable is null or contains any null elements.
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        if (enumerableObjectToCheck == null)
        {
            Debug.LogWarning($"{fieldName} is null in object {thisObject.name}");
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.LogWarning($"{fieldName} contains a null element in object {thisObject.name}");
                return true;
            }
        }

        return false;
    }

    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }
    public static bool ValidateCheckPositiveRange(Object thisObj, string fieldNameMin, float valueToCheckMin, string fieldNameMaximum, float valueToCheckMax, bool isZeroAllowed)
    {
        bool error = false;
        if (valueToCheckMin > valueToCheckMax)
        {
            Debug.Log(fieldNameMin + " must be less than or equal to " + fieldNameMaximum + " in object " + thisObj.name.ToString());
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObj, fieldNameMin, valueToCheckMin, isZeroAllowed)) error = true;

        if (ValidateCheckPositiveValue(thisObj, fieldNameMaximum, valueToCheckMax, isZeroAllowed)) error = true;

        return error;
    }

    public static Vector3 GetNearestSpawnPoint(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0f);

        // Loop through room spawn positions
        foreach (Vector2Int spawnPositionGrid in currentRoom.spawnPositionArray)
        {
            // convert the spawn grid positions to world positions
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);

            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;

    }
}
