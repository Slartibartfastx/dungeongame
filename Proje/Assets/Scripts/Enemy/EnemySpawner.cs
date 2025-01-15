using System.Collections;
using UnityEngine;
using static NodeTypeScriptableObject;
using static StaticEventHandler;
using static UnityEngine.RuleTile.TilingRuleOutput;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonoBehaviour<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawnedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParameters roomEnemySpawnParameters;

    private void OnEnable()
    {
        // subscribe to room changed event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        // unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

 
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;

        currentRoom = roomChangedEventArgs.room;

        // Update music for room
        // MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

        // if the room is a corridor or the entrance then return
        if (currentRoom.roomNodeType.roomType == RoomType.CorridorEW ||
     currentRoom.roomNodeType.roomType == RoomType.CorridorNS ||
     currentRoom.roomNodeType.roomType == RoomType.Entrance)
            return;

        // if the room has already been defeated then return
        if (currentRoom.isClearedOfEnemies) return;

        // Get random number of enemies to spawn
        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        // Get room enemy spawn parameters
        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(GameManager.Instance.GetCurrentDungeonLevel());

        // If no enemies to spawn return
        if (enemiesToSpawn == 0)
        {
            // Mark the room as cleared
            currentRoom.isClearedOfEnemies = true;

            return;
        }

        // Get concurrent number of enemies to spawn
        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();

        // Update music for room
        //MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

        // Lock doors
        currentRoom.instantiatedRoom.LockDoors();

        // Spawn enemies
        SpawnEnemies();
    }

 
    private void SpawnEnemies()
    {
        // Set gamestate engaging boss
        if (GameManager.Instance.gameState == GameState.BossBattlePreparation)
        {
            GameManager.Instance.previousGameState = GameState.BossBattlePreparation;
            GameManager.Instance.gameState = GameState.FacingBoss;
        }
      

        // Set gamestate engaging enemies
       /* else */ if  (GameManager.Instance.gameState == GameState.GameInitialization)
        {
            GameManager.Instance.previousGameState = GameState.GameInitialization;
            GameManager.Instance.gameState = GameState.EncounteringEnemies;
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

  
    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Create an instance of the helper class used to select a random enemy
        RandomSpawnableObject<EnemyDetails> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetails>(currentRoom.enemiesByLevelList);

        // Check we have somewhere to spawn the enemies
        if (currentRoom.spawnPositionArray.Length > 0)
        {
            // Loop through to create all the enemeies
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // wait until current enemy count is less than max concurrent enemies
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
                {
                    yield return null;
                }

                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                // Create Enemy - Get next enemy type to spawn 
                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

  
    private float GetEnemySpawnInterval()
    {
        return (Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval));
    }


    private int GetConcurrentEnemies()
    {
        return (Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies));
    }


    private void CreateEnemy(EnemyDetails enemyDetails, Vector3 position)
    {
        // keep track of the number of enemies spawned so far 
        enemiesSpawnedSoFar++;

        // Add one to the current enemy count - this is reduced when an enemy is destroyed
        currentEnemyCount++;

        // Get current dungeon level
        DungeonLevelScriptableObject dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);

        // Initialize Enemy
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);

        // subscribe to enemy destroyed event
        enemy.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;

    }
    

    private void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        // Unsubscribe from event
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        // reduce current enemy count
        currentEnemyCount--;

        // Score points - call points scored event
       // StaticEventHandler.CallPointsScoredEvent(destroyedEventArgs.points);

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoom.isClearedOfEnemies = true;

            // Set game state
            if (GameManager.Instance.gameState == GameState.EncounteringEnemies)
            {
                GameManager.Instance.gameState = GameState.InLevelPlay;
                GameManager.Instance.previousGameState = GameState.EncounteringEnemies;
            }

            else if (GameManager.Instance.gameState == GameState.FacingBoss)
            {
                GameManager.Instance.gameState = GameState.BossBattlePreparation;
                GameManager.Instance.previousGameState = GameState.FacingBoss;
            }

            // unlock doors
            currentRoom.instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);

            // Update music for room
           // MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

            // Trigger room enemies defeated event
            StaticEventHandler.CallRoomEnemiesDefeatedEvent(currentRoom);
        }
    }
    

}