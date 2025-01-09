using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty;  // use this 2d array to store movement penalties from the tilemaps to be used in AStar pathfinding
    [HideInInspector] public int[,] aStarItemObstacles; // use to store position of moveable items that are obstacles
    [HideInInspector] public Bounds roomColliderBounds;

    #region Header OBJECT REFERENCES

    [Space(10)]
    [Header("OBJECT REFERENCES")]

    #endregion Header OBJECT REFERENCES

    #region Tooltip

    [Tooltip("Populate with the environment child placeholder gameobject ")]

    #endregion Tooltip

    [SerializeField] private GameObject environmentGameObject;


    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;

    }


    // Trigger room changed event when player enters a room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player triggered the collider
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            // Set room as visited
            this.room.isPreviouslyVisited = true;

            // Call room changed event
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }
    /// <summary>
    /// Initialise The Instantiated Room
    /// </summary>
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);
        BlockOffUnusedDoorWays();
        AddObstaclesAndPreferredPaths();
        AddDoorsToRooms();
        DisableCollisionTilemapRenderer();

    }

    /// <summary>
    /// Update obstacles used by AStar pathfinmding.
    /// </summary>
    private void AddObstaclesAndPreferredPaths()
    {
        // this array will be populated with wall obstacles 
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];


        // Loop thorugh all grid squares
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                // Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                // Add preferred path for enemies (1 is the preferred path value, default value for
                // a grid location is specified in the Settings).
                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }

            }
        }

    }


    /// <summary>
    /// Block Off Unused Doorways In The Room
    /// </summary>
    private void BlockOffUnusedDoorWays()
    {
        if (room == null || room.doorWayList == null) return;

        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected) continue;

            // Block unconnected doorways across all relevant tilemaps
            Tilemap[] tilemaps = { collisionTilemap, minimapTilemap, groundTilemap, decoration1Tilemap, decoration2Tilemap, frontTilemap };
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap != null)
                    BlockDoorway(tilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Block a doorway on a tilemap layer
    /// </summary>
    private void BlockDoorway(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayTiles(tilemap, doorway, isHorizontal: true);
                break;

            case Orientation.east:
            case Orientation.west:
                BlockDoorwayTiles(tilemap, doorway, isHorizontal: false);
                break;

            case Orientation.none:
                // Do nothing for no orientation
                break;
        }
    }

    /// <summary>
    /// Block doorway tiles based on orientation
    /// </summary>
    private void BlockDoorwayTiles(Tilemap tilemap, Doorway doorway, bool isHorizontal)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        int width = doorway.doorwayCopyTileWidth;
        int height = doorway.doorwayCopyTileHeight;

        // Determine the direction of blocking
        int xOffset = isHorizontal ? 1 : 0;
        int yOffset = isHorizontal ? 0 : -1;

        // Loop through tiles and copy to block the doorway
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int sourcePosition = new Vector3Int(startPosition.x + x, startPosition.y - y, 0);
                Vector3Int targetPosition = new Vector3Int(startPosition.x + x + xOffset, startPosition.y - y + yOffset, 0);

                // Copy tile and transformation matrix
                TileBase tile = tilemap.GetTile(sourcePosition);
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(sourcePosition);

                tilemap.SetTile(targetPosition, tile);
                tilemap.SetTransformMatrix(targetPosition, transformMatrix);
            }
        }
    }

    /// <summary>
    /// Populate the tilemap and grid memeber variables.
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        // Get the grid component.
        grid = roomGameobject.GetComponentInChildren<Grid>();

        // Get tilemaps in children.
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }

        }

    }

    /// <summary>
    /// Disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        // Disable collision tilemap renderer
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;

    }



    /// <summary>
    /// Add opening doors if this is not a corridor room
    /// </summary>
    private void AddDoorsToRooms()
    {
        // if the room is a corridor then return
        if (room.roomNodeType.roomType == NodeTypeScriptableObject.RoomType.CorridorEW ||
     room.roomNodeType.roomType == NodeTypeScriptableObject.RoomType.CorridorNS) return;

            // Instantiate door prefabs at doorway positions
            foreach (Doorway doorway in room.doorWayList)
        {

            // if the doorway prefab isn't null and the doorway is connected
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                if (doorway.orientation == Orientation.north)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0f);
                }
                else if (doorway.orientation == Orientation.south)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0f);
                }
                else if (doorway.orientation == Orientation.east)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance * 1.25f, 0f);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0f);
                }

                // Get door component
                DoorScript doorComponent = door.GetComponent<DoorScript>();

                // Set if door is part of a boss room
                if (room.roomNodeType.roomType == NodeTypeScriptableObject.RoomType.BossRoom)
                {
                    doorComponent.isBossRoomDoor = true;

                    // lock the door to prevent access to the room
                    doorComponent.LockDoor();

                    // Instantiate skull icon for minimap by door
                   // GameObject skullIcon = Instantiate(GameResources.Instance.minimapSkullPrefab, gameObject.transform);
                    //skullIcon.transform.localPosition = door.transform.localPosition;

                }

            }

        }

    }


    /// <summary>
    /// Lock the room doors
    /// </summary>
    public void LockDoors()
    {
        // Get all DoorScript components in children of this room object
        DoorScript[] doorArray = GetComponentsInChildren<DoorScript>();

        // Debugging: Print the number of doors found in the room
        Debug.Log($"Found {doorArray.Length} doors in the room.");

        // Trigger lock doors for each door found
        foreach (DoorScript door in doorArray)
        {
            // Debugging: Print the door being locked
            Debug.Log($"Locking door: {door.gameObject.name}");

            door.LockDoor();  // Lock the door
        }

        // Disable room trigger collider after locking doors
        DisableRoomCollider();
    }


    /// <summary>
    /// Disable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    /// <summary>
    /// Unlock the room doors
    /// </summary>
    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    /// <summary>
    /// Unlock the room doors routine
    /// </summary>
    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);

        DoorScript[] doorArray = GetComponentsInChildren<DoorScript>();

        // Trigger open doors
        foreach (DoorScript door in doorArray)
        {
            door.UnlockDoor();
        }

        // Enable room trigger collider
        EnableRoomCollider();
    }
    /// <summary>
    /// Enable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(environmentGameObject), environmentGameObject);
    }

#endif

    #endregion Validation
}