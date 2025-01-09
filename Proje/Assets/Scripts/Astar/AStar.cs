using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition, and adds
    /// movement steps to the returned Stack. Returns null if no path is found.
    /// </summary>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        // Adjust positions by lower bounds
        Vector3Int adjustedStart = startGridPosition - (Vector3Int)room.templateLowerBounds;
        Vector3Int adjustedEnd = endGridPosition - (Vector3Int)room.templateLowerBounds;

        // Initialize data structures
        var openNodes = new List<AStarNode>();
        var closedNodes = new HashSet<AStarNode>();

        // Setup grid for pathfinding
        var gridNodes = new AStarGridNodes(
            room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1
        );

        // Retrieve start and target nodes
        AStarNode startNode = gridNodes.GetGridNode(adjustedStart.x, adjustedStart.y);
        AStarNode targetNode = gridNodes.GetGridNode(adjustedEnd.x, adjustedEnd.y);

        // Attempt to find the shortest path
        AStarNode pathEndNode = FindShortestPath(startNode, targetNode, gridNodes, openNodes, closedNodes, room.instantiatedRoom);

        // If path is found, create and return the movement path
        if (pathEndNode != null)
        {
            return BuildMovementPath(pathEndNode, room);
        }

        // Return null if no path is found
        return null;
    }

    /// <summary>
    /// Creates a movement path from the target node to the start node by traversing the parent nodes.
    /// </summary>
    private static Stack<Vector3> BuildMovementPath(AStarNode targetNode, Room room)
    {
        if (targetNode == null || room == null || room.instantiatedRoom == null)
        {
            Debug.LogError("Invalid input parameters for BuildMovementPath.");
            return null;
        }

        var path = new Stack<Vector3>();
        var currentNode = targetNode;

        // Get midpoint of grid cells for proper alignment
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        // Trace back the path from target to start
        while (currentNode != null)
        {
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(
                new Vector3Int(currentNode.GridPosition.x + room.templateLowerBounds.x, currentNode.GridPosition.y + room.templateLowerBounds.y, 0)
            );

            // Adjust position to the center of the grid cell
            worldPosition += cellMidPoint;

            path.Push(worldPosition);

            // Move to the next parent node
            currentNode = currentNode.ParentNode;
        }

        return path;
    }

    /// <summary>
    /// Finds the shortest path from the start node to the target node. Returns the target node if a path is found; otherwise, returns null.
    /// </summary>
    private static AStarNode FindShortestPath(AStarNode startNode, AStarNode targetNode, AStarGridNodes gridNodes, List<AStarNode> openNodes, HashSet<AStarNode> closedNodes, InstantiatedRoom instantiatedRoom)
    {
        // Initialize the open list with the starting node
        openNodes.Add(startNode);

        // Process the open list until it is empty or the path is found
        while (openNodes.Count > 0)
        {
            // Sort the open list to prioritize nodes with the lowest fCost
            openNodes.Sort();

            // Pop the node with the lowest fCost from the open list
            AStarNode currentNode = openNodes[0];
            openNodes.RemoveAt(0);

            // Check if the current node is the target node
            if (currentNode.Equals(targetNode))
            {
                return currentNode; // Path found, return the target node
            }

            // Add the current node to the closed list to avoid revisiting it
            closedNodes.Add(currentNode);

            // Evaluate the neighbors of the current node
            EvaluateNeighbours(currentNode, targetNode, gridNodes, openNodes, closedNodes, instantiatedRoom);
        }

        // No path found, return null
        return null;
    }


    /// <summary>
    /// Evaluate neighbour nodes
    /// </summary>
    /// <summary>
    /// Evaluates and processes each valid neighbor of the current node.
    /// </summary>
    private static void EvaluateNeighbours(AStarNode currentNode, AStarNode targetNode, AStarGridNodes gridNodes, List<AStarNode> openNodeList, HashSet<AStarNode> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentPosition = currentNode.GridPosition;

        // Check all 8 possible directions (adjacent cells)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Skip the current node itself (0,0)
                if (i == 0 && j == 0)
                    continue;

                // Get the valid neighboring node
                AStarNode neighbor = GetValidNodeNeighbour(currentPosition.x + i, currentPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                // If the neighbor is valid, process it
                if (neighbor != null)
                {
                    // Calculate the new cost to the neighbor
                    int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor) + instantiatedRoom.aStarMovementPenalty[neighbor.GridPosition.x, neighbor.GridPosition.y];

                    // Check if the neighbor is not in the open list or if a cheaper path is found
                    bool inOpenList = openNodeList.Contains(neighbor);
                    if (newCostToNeighbor < neighbor.GCost || !inOpenList)
                    {
                        neighbor.SetGCost(newCostToNeighbor); // Update the GCost
                        neighbor.SetHCost(GetDistance(neighbor, targetNode)); // Update the HCost
                        neighbor.ParentNode = currentNode; // Set the parent node

                        // If the neighbor is not already in the open list, add it
                        if (!inOpenList)
                        {
                            openNodeList.Add(neighbor);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Returns the distance int between nodeA and nodeB
    /// </summary>
    private static int GetDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x);
        int dstY = Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);  // 10 used instead of 1, and 14 is a pythagoras approximation SQRT(10*10 + 10*10) - to avoid using floats
        return 14 * dstX + 10 * (dstY - dstX);
    }

    /// <summary>
    /// Evaluates a neighbor node based on its position and checks its validity against the grid, closed list, and obstacles.
    /// Returns the valid neighbor node if it exists; otherwise, returns null.
    /// </summary>
    private static AStarNode GetValidNodeNeighbour(int x, int y, AStarGridNodes gridNodes, HashSet<AStarNode> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // Ensure the neighbor node is within bounds
        if (!IsWithinBounds(x, y, instantiatedRoom))
        {
            return null; // If out of bounds, return null
        }

        // Get the grid node at the given position
        AStarNode neighborNode = gridNodes.GetGridNode(x, y);

        // Check if the neighbor is walkable (no obstacles)
        if (IsWalkable(x, y, instantiatedRoom) && !closedNodeHashSet.Contains(neighborNode))
        {
            return neighborNode; // Return the valid neighbor node
        }

        return null; // If not walkable or already in the closed list, return null
    }

    /// <summary>
    /// Checks if the given position is within the grid bounds.
    /// </summary>
    private static bool IsWithinBounds(int x, int y, InstantiatedRoom instantiatedRoom)
    {
        return x >= 0 && x < instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x &&
               y >= 0 && y < instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y;
    }

    /// <summary>
    /// Checks if the given grid position is walkable (not blocked by an obstacle).
    /// </summary>
    private static bool IsWalkable(int x, int y, InstantiatedRoom instantiatedRoom)
    {
        // Get the movement penalty for the grid space (0 indicates an obstacle)
        int movementPenalty = instantiatedRoom.aStarMovementPenalty[x, y];

        // Return true if the movement penalty is non-zero (indicating a walkable space)
        return movementPenalty > 0;
    }

}
