
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class PoolManager : SingletonMonoBehaviour<PoolManager>
{
    #region Tooltip
    [Tooltip("Populate this array with prefabs that you want to add to the pool, and specify the number of gameobjects to be created for each.")]
    #endregion
    [SerializeField] private Pool[] poolArray = null;
    private Transform objPoolTrans;
    private Dictionary<int, Queue<Component>> poolDict = new Dictionary<int, Queue<Component>>();


    [System.Serializable]
    public struct Pool
    {
        public int size;
        public GameObject prefab;
        public string type;
    }

    public void Start()
    {
        objPoolTrans = this.gameObject.transform;

        for (int i = 0; i < poolArray.Length; i++)
        {
            createPool(poolArray[i].prefab, poolArray[i].size, poolArray[i].type);
        }
    }





    private void createPool(GameObject prefab, int size, string type)
    {
        int key = prefab.GetInstanceID();
        string name = prefab.name;
        GameObject parent = new GameObject(name + "Anchor");
        parent.transform.SetParent(objPoolTrans);

        if (!poolDict.ContainsKey(key))
        {
            poolDict.Add(key, new Queue<Component>());

            for (int i = 0; i < size; i++)
            {
                GameObject newObj = Instantiate(prefab, parent.transform) as GameObject;
                newObj.SetActive(false);
                poolDict[key].Enqueue(newObj.GetComponent(Type.GetType(type)));
            }
        }
    }


    public Component Reuse(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        int key = prefab.GetInstanceID();
        if (poolDict.ContainsKey(key))
        {
            Component reuseComp = GetCompFromPool(key);
            ResetObj(pos, rot, reuseComp, prefab);
            return reuseComp;

        }
        else
        {
            Debug.Log("no obj to return");
            return null;
        }


    }

    private Component GetCompFromPool(int poolKey)
    {
        Component componentToReuse = poolDict[poolKey].Dequeue();
        poolDict[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf == true)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Reset the gameobject.
    /// </summary>
    private void ResetObj(Vector3 pos, Quaternion rot, Component reuseComp, GameObject prefab)
    {
        reuseComp.transform.position = pos;
        reuseComp.transform.rotation = rot;
        reuseComp.gameObject.transform.localScale = prefab.transform.localScale;
    }
}