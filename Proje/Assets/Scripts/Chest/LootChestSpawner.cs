using System.Collections.Generic;
using UnityEngine;

using static StaticEventHandler;

public class LootChestSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct SpawnRangeByLevel
    {
        public DungeonLevelScriptableObject levelData;
        [Range(0, 100)] public int minSpawnChance;
        [Range(0, 100)] public int maxSpawnChance;
    }

    [SerializeField] private GameObject lootChestPrefab;
    [SerializeField][Range(0, 100)] private int globalSpawnChanceMin;
    [SerializeField][Range(0, 100)] private int globalSpawnChanceMax;
    [SerializeField] private List<SpawnRangeByLevel> spawnChanceByLevelList;
    [SerializeField] private ChestSpawnCondition spawnCondition;
    [SerializeField] private ChestSpawnLocation spawnLocation;
    [SerializeField][Range(0, 3)] private int minItemsToSpawn;
    [SerializeField][Range(0, 3)] private int maxItemsToSpawn;
    [SerializeField] private List<SpawnableObjectsByLevel<WeaponDetails>> weaponSpawnListByLevel;
    [SerializeField] private List<SpawnRangeByLevel> healthSpawnListByLevel;
    [SerializeField] private List<SpawnRangeByLevel> ammoSpawnListByLevel;

    private bool isChestSpawned = false;
    private Room currentRoom;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += HandleRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += HandleRoomEnemiesDefeated;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= HandleRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= HandleRoomEnemiesDefeated;
    }

    private void HandleRoomChanged(RoomChangedEventArgs args)
    {
        if (currentRoom == null)
        {
            currentRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        if (!isChestSpawned && spawnCondition == ChestSpawnCondition.onRoomEntry && currentRoom == args.room)
        {
            CreateLootChest();
        }
    }

    private void HandleRoomEnemiesDefeated(RoomEnemiesDefeatedArgs args)
    {
        if (currentRoom == null)
        {
            currentRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        if (!isChestSpawned && spawnCondition == ChestSpawnCondition.onEnemiesDefeated && currentRoom == args.room)
        {
            CreateLootChest();
        }
    }

    private void CreateLootChest()
    {
        isChestSpawned = true;

        if (!ShouldSpawnChest()) return;

        DetermineItemsToSpawn(out int ammoCount, out int healthCount, out int weaponCount);

        GameObject chestInstance = Instantiate(lootChestPrefab, transform);

        if (spawnLocation == ChestSpawnLocation.SpawnerPosition)
        {
            chestInstance.transform.position = transform.position;
        }
        else if (spawnLocation == ChestSpawnLocation.PlayerPosition)
        {
            Vector3 playerPosition = GameManager.Instance.GetPlayer().transform.position;
            Vector3 spawnPoint = HelperUtilities.GetNearestSpawnPoint(playerPosition);
            Vector3 positionVariation = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            chestInstance.transform.position = spawnPoint + positionVariation;
        }

        TreasureChest lootChest = chestInstance.GetComponent<TreasureChest>();
        bool shouldUnlockChest = spawnCondition == ChestSpawnCondition.onRoomEntry;

        lootChest.InitializeChest(
            shouldUnlockChest,
            GetHealthSpawnPercentage(healthCount),
            GetWeaponToSpawn(weaponCount),
            GetAmmoSpawnPercentage(ammoCount)
        );
    }

    private bool ShouldSpawnChest()
    {
        int spawnChance = Random.Range(globalSpawnChanceMin, globalSpawnChanceMax + 1);

        foreach (SpawnRangeByLevel levelRange in spawnChanceByLevelList)
        {
            if (levelRange.levelData == GameManager.Instance.GetCurrentDungeonLevel())
            {
                spawnChance = Random.Range(levelRange.minSpawnChance, levelRange.maxSpawnChance + 1);
                break;
            }
        }

        int randomValue = Random.Range(1, 101);
        return randomValue <= spawnChance;
    }

    private void DetermineItemsToSpawn(out int ammo, out int health, out int weapons)
    {
        ammo = 0;
        health = 0;
        weapons = 0;

        int totalItemsToSpawn = Random.Range(minItemsToSpawn, maxItemsToSpawn + 1);

        if (totalItemsToSpawn == 1)
        {
            int randomChoice = Random.Range(0, 3);
            if (randomChoice == 0) { weapons++; }
            else if (randomChoice == 1) { ammo++; }
            else if (randomChoice == 2) { health++; }
        }
        else if (totalItemsToSpawn == 2)
        {
            int randomChoice = Random.Range(0, 3);
            if (randomChoice == 0) { weapons++; ammo++; }
            else if (randomChoice == 1) { ammo++; health++; }
            else if (randomChoice == 2) { health++; weapons++; }
        }
        else if (totalItemsToSpawn >= 3)
        {
            weapons++;
            ammo++;
            health++;
        }
    }

    private int GetAmmoSpawnPercentage(int ammoCount)
    {
        if (ammoCount == 0) return 0;

        foreach (SpawnRangeByLevel spawnRange in ammoSpawnListByLevel)
        {
            if (spawnRange.levelData == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnRange.minSpawnChance, spawnRange.maxSpawnChance);
            }
        }

        return 0;
    }

    private int GetHealthSpawnPercentage(int healthCount)
    {
        if (healthCount == 0) return 0;

        foreach (SpawnRangeByLevel spawnRange in healthSpawnListByLevel)
        {
            if (spawnRange.levelData == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnRange.minSpawnChance, spawnRange.maxSpawnChance);
            }
        }

        return 0;
    }

    private WeaponDetails GetWeaponToSpawn(int weaponCount)
    {
        if (weaponCount == 0) return null;

        RandomSpawnableObject<WeaponDetails> randomWeapon = new RandomSpawnableObject<WeaponDetails>(weaponSpawnListByLevel);
        return randomWeapon.GetItem();
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(lootChestPrefab), lootChestPrefab);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(globalSpawnChanceMin), globalSpawnChanceMin, nameof(globalSpawnChanceMax), globalSpawnChanceMax, true);

        if (spawnChanceByLevelList != null && spawnChanceByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnChanceByLevelList), spawnChanceByLevelList);

            foreach (SpawnRangeByLevel range in spawnChanceByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(range.levelData), range.levelData);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(range.minSpawnChance), range.minSpawnChance, nameof(range.maxSpawnChance), range.maxSpawnChance, true);
            }
        }

        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minItemsToSpawn), minItemsToSpawn, nameof(maxItemsToSpawn), maxItemsToSpawn, true);

        // Similar validation for weapons, health, and ammo.
    }
#endif
    #endregion Validation
}
