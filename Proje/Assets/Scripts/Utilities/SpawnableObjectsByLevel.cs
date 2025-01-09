using System.Collections.Generic;

[System.Serializable]
public class SpawnableObjectsByLevel<T>
{
    public DungeonLevelScriptableObject dungeonLevel;
    public List<SpawnableObjectRatio<T>> spawnableObjectRatioList;

}