using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    /// <summary>
    /// Create player in scene at position
    /// </summary>
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
       // StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        // Subscribe to the points scored event
        //StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;

        // Subscribe to score multiplier event
       // StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;

        // Subscribe to player destroyed event
       // player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        // Unsubscribe from room enemies defeated event
      //  StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        // Unsubscribe from the points scored event
        //StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;

        // Unsubscribe from score multiplier event
       // StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        // Unubscribe from player destroyed event
        //player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;

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

                break;
        }

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
