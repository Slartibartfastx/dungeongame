using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static NodeTypeScriptableObject;
using static StaticEventHandler;

[DisallowMultipleComponent]
public class GameManager : SingletonMonoBehaviour<GameManager>
{

    #region Header DUNGEON LEVELS

    [Space(10)]
    [Header("DUNGEON LEVELS")]

    #endregion Header DUNGEON LEVELS

    #region Tooltip

    [Tooltip("Populate with the dungeon level scriptable objects")]

    #endregion Tooltip

    [SerializeField] private List<DungeonLevelScriptableObject> dungeonLevelList;

    #region Tooltip

    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]

    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetails playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    private InstantiatedRoom bossRoom;
    [SerializeField] private GameResources gameResources;


    protected override void Awake()
    {
        // Call base class
        base.Awake();

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // Instantiate player
        Debug.Log(GameResources.Instance);
        Debug.Log(GameResources.Instance.currentPlayer);
        Debug.Log(playerDetails);
        InstantiatePlayer();

    }

    private void InstantiatePlayer()
    {
        // Instantiate player
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        // Initialize Player
        player = playerGameObject.GetComponent<Player>();
        Debug.Log(player);
        Debug.Log(playerDetails);
        player.Initalize(playerDetails);

    }

    // Start is called before the first frame update
    private void Start()
    {
        previousGameState = GameState.GameInitialization;
        gameState = GameState.GameInitialization;

        if (gameResources == null)
        {
            Debug.LogError("GameResources is not assigned!");
        }
        else
        {
            Debug.Log("GameResources loaded successfully.");
        }
    }


    // Update is called once per frame
    private void Update()
    {
        HandleGameState();


    }

    private void OnEnable()
    {
        // Subscribe to room changed event.
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        // Subscribe to room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        // Subscribe to the points scored event
        //StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;

        // Subscribe to score multiplier event
       // StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;

        // Subscribe to player destroyed event
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        // Unsubscribe from room enemies defeated event
       StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        // Unsubscribe from the points scored event
        //StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;

        // Unsubscribe from score multiplier event
       // StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        // Unubscribe from player destroyed event
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;

    }

    /// <summary>
    /// Handle player destroyed event
    /// </summary>
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.DefeatEncountered;
    }


    /// <summary>
    /// Handle room enemies defeated event
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    /// <summary>
    /// Room enemies defated - test if all dungeon rooms have been cleared of enemies - if so load
    /// next dungeon game level
    /// </summary>
    private void RoomEnemiesDefeated()
    {
        // Initialise dungeon as being cleared - but then test each room
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        // Loop through all dungeon rooms to see if cleared of enemies
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            // skip boss room for time being
            if (keyValuePair.Value.roomNodeType.roomType == RoomType.BossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            // check if other rooms have been cleared of enemies
            if (!keyValuePair.Value.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        // Set game state
        // If dungeon level completly cleared (i.e. dungeon cleared apart from boss and there is no boss room OR dungeon cleared apart from boss and boss room is also cleared)
        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            // Are there more dungeon levels then
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.LevelSuccess;
            }
            else
            {
                gameState = GameState.VictoryAchieved;
            }
        }
        // Else if dungeon level cleared apart from boss room
        else if (isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.BossBattlePreparation;

            StartCoroutine(BossStage());
        }

    }

    /// <summary>
    /// Enter boss stage
    /// </summary>
    private IEnumerator BossStage()
    {
        // Activate boss room
        bossRoom.gameObject.SetActive(true);

        // Unlock boss room
        bossRoom.UnlockDoors(0f);

        // Wait 2 seconds
        yield return new WaitForSeconds(2f);

        Debug.Log("Boss Stage");
    }

    /// <summary>
    /// Handle game state
    /// </summary>
    private void HandleGameState()
    {
        // Handle game state
        switch (gameState)
        {
            case GameState.GameInitialization:

                // Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.InLevelPlay;
                // Trigger room enemies defeated since we start in the entrance where there are no enemies (just in case you have a level with just a boss room!)
                RoomEnemiesDefeated();

                break;

            // handle the level being completed
            case GameState.LevelSuccess:

                // Display level completed text
                StartCoroutine(LevelSuccess());

                break;

            // handle the game being won (only trigger this once - test the previous game state to do this)
            case GameState.VictoryAchieved:

                if (previousGameState != GameState.VictoryAchieved)
                    StartCoroutine(VictoryAchieved());

                break;

            // handle the game being lost (only trigger this once - test the previous game state to do this)
            case GameState.DefeatEncountered:

                if (previousGameState != GameState.DefeatEncountered)
                {
                    StopAllCoroutines(); // Prevent messages if you clear the level just as you get killed
                    StartCoroutine(DefeatEncountered());
                }

                break;

            // restart the game
            case GameState.ResettingGame:

                ResettingGame();

                break;

        }

    }

    /// <summary>
    /// Show level as being completed - load next level
    /// </summary>
    private IEnumerator LevelSuccess()
    {
        // Play next level
        gameState = GameState.InLevelPlay;

        // Wait 2 seconds
        yield return new WaitForSeconds(2f);

        Debug.Log("Level completed - Press T to proceed to the next level");
        // When player presses the T key, proceed to the next level
        while (!Input.GetKeyDown(KeyCode.T))
        {
            yield return null;
        }


        yield return null; // to avoid enter being detected twice

        // Increase index to next level
        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    /// <summary>
    /// Game Won
    /// </summary>
    private IEnumerator VictoryAchieved()
    {
        previousGameState = GameState.VictoryAchieved;

        Debug.Log("Game Won");
        // Wait 1 seconds
        yield return new WaitForSeconds(10f);


        // Set game state to restart game
        gameState = GameState.ResettingGame;
    }

    /// <summary>
    /// Game Lost
    /// </summary>
    private IEnumerator DefeatEncountered()
    {
        previousGameState = GameState.DefeatEncountered;


        Debug.Log("You Lost");

        // Wait 1 seconds
        yield return new WaitForSeconds(1f);

        gameState = GameState.ResettingGame;
    }

    /// <summary>
    /// Restart the game
    /// </summary>
    private void ResettingGame()
    {

        SceneManager.LoadScene("MainGameScene");
    }


    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);
        
        player.gameObject.transform.position = HelperUtilities.GetNearestSpawnPoint(player.gameObject.transform.position);

    }

    public Player GetPlayer()   
    {

        return player;
    }
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
 public void setRoom(Room room)
{
    previousRoom = currentRoom;
    currentRoom = room;

    Debug.Log($"Room changed: PreviousRoom={previousRoom?.templateID}, CurrentRoom={currentRoom?.templateID}");
}

    /// <summary>
    /// Get the current dungeon level
    /// </summary>
    public DungeonLevelScriptableObject GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        setRoom(roomChangedEventArgs.room);
    }


    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    { 
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation
}
