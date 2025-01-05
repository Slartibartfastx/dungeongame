using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
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
}
