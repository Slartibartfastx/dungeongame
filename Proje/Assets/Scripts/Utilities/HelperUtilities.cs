using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static Camera mainCam;

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
        float deg = rad * Mathf.Deg2Rad;

        return deg;
    }

    public static AimDir getAimdir(float angle)
    {
        AimDir aimdir;

        if (angle >= 22f && angle <= 67f)
        {
            aimdir = AimDir.UpRight;
        }
        else if (angle > 67f && angle <= 112f)
        {
            aimdir = AimDir.Up;
        }
        else if (angle > 112f && angle <= 158f)
        {
            aimdir = AimDir.UpLeft;
        }
        else if ((angle > 158f && angle <= 180f) || (angle > -180 && angle <= -135f))
        {
            aimdir = AimDir.Left;
        }
        else if (angle > -135f && angle <= -45f)
        {
            aimdir = AimDir.Down;
        }
        else if ((angle > -45f && angle <= 0f) || (angle > 0 && angle <= 22f))
        {
            aimdir = AimDir.Right;
        }
        else
        {
            aimdir = AimDir.Right;
        }
        return aimdir;

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

}
