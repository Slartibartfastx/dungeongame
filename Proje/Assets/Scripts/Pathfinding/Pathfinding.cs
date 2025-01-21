using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    public static Stack<Vector3> GeneratePath(Room map, Vector3Int startCell, Vector3Int endCell)
    {
        Vector3Int adjustedStart = startCell - (Vector3Int)map.templateLowerBounds;
        Vector3Int adjustedEnd = endCell - (Vector3Int)map.templateLowerBounds;

        var openList = new List<PathfindingNode>();
        var closedSet = new HashSet<PathfindingNode>();

        var pathfindingGrid = new PathfindingGrid(
            map.templateUpperBounds.x - map.templateLowerBounds.x + 1,
            map.templateUpperBounds.y - map.templateLowerBounds.y + 1
        );

        PathfindingNode startNode = pathfindingGrid.GetNode(adjustedStart.x, adjustedStart.y);
        PathfindingNode endNode = pathfindingGrid.GetNode(adjustedEnd.x, adjustedEnd.y);

        PathfindingNode finalNode = CalculateShortestPath(startNode, endNode, pathfindingGrid, openList, closedSet, map.instantiatedRoom);
        if (finalNode != null)
        {
            return CreateMovementPath(finalNode, map);
        }

        return null;
    }

    private static Stack<Vector3> CreateMovementPath(PathfindingNode endNode, Room map)
    {
        if (endNode == null || map == null || map.instantiatedRoom == null)
        {
            Debug.LogError("Invalid input parameters for CreateMovementPath.");
            return null;
        }

        var pathStack = new Stack<Vector3>();
        var currentNode = endNode;

        Vector3 cellCenterOffset = map.instantiatedRoom.grid.cellSize * 0.5f;
        cellCenterOffset.z = 0f;

        while (currentNode != null)
        {
            Vector3 worldPosition = map.instantiatedRoom.grid.CellToWorld(
                new Vector3Int(currentNode.PositionOnGrid.x + map.templateLowerBounds.x, currentNode.PositionOnGrid.y + map.templateLowerBounds.y, 0)
            );
            worldPosition += cellCenterOffset;

            pathStack.Push(worldPosition);

            currentNode = currentNode.PreviousNode;
        }

        return pathStack;
    }

    private static PathfindingNode CalculateShortestPath(PathfindingNode startNode, PathfindingNode endNode, PathfindingGrid pathfindingGrid, List<PathfindingNode> openList, HashSet<PathfindingNode> closedSet, InstantiatedRoom room)
    {
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            openList.Sort();

            PathfindingNode currentNode = openList[0];
            openList.RemoveAt(0);

            if (currentNode.Equals(endNode))
            {
                return currentNode; 
            }
            closedSet.Add(currentNode);

            CheckNeighbours(currentNode, endNode, pathfindingGrid, openList, closedSet, room);
        }

        return null;
    }

    private static void CheckNeighbours(PathfindingNode currentNode, PathfindingNode endNode, PathfindingGrid pathfindingGrid, List<PathfindingNode> openList, HashSet<PathfindingNode> closedSet, InstantiatedRoom room)
    {
        Vector2Int currentPosition = currentNode.PositionOnGrid;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                PathfindingNode neighbor = GetValidNeighbor(currentPosition.x + i, currentPosition.y + j, pathfindingGrid, closedSet, room);
                if (neighbor != null)
                {
                    int newCostToNeighbor = currentNode.StartCost + GetDistance(currentNode, neighbor) + room.aStarMovementPenalty[neighbor.PositionOnGrid.x, neighbor.PositionOnGrid.y];
                    bool inOpenList = openList.Contains(neighbor);
                    if (newCostToNeighbor < neighbor.StartCost || !inOpenList)
                    {
                        neighbor.UpdateStartCost(newCostToNeighbor); 
                        neighbor.UpdateEndCost(GetDistance(neighbor, endNode)); 
                        neighbor.PreviousNode = currentNode; 

                        if (!inOpenList)
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
        }
    }

    private static int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.PositionOnGrid.x - nodeB.PositionOnGrid.x);
        int dstY = Mathf.Abs(nodeA.PositionOnGrid.y - nodeB.PositionOnGrid.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);  
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private static PathfindingNode GetValidNeighbor(int x, int y, PathfindingGrid pathfindingGrid, HashSet<PathfindingNode> closedSet, InstantiatedRoom room)
    {
        if (!IsInBounds(x, y, room))
        {
            return null;
        }

        PathfindingNode neighborNode = pathfindingGrid.GetNode(x, y);

        if (IsWalkable(x, y, room) && !closedSet.Contains(neighborNode))
        {
            return neighborNode; 
        }

        return null; 
    }

    private static bool IsInBounds(int x, int y, InstantiatedRoom room)
    {
        return x >= 0 && x < room.room.templateUpperBounds.x - room.room.templateLowerBounds.x &&
               y >= 0 && y < room.room.templateUpperBounds.y - room.room.templateLowerBounds.y;
    }

    private static bool IsWalkable(int x, int y, InstantiatedRoom room)
    {
        int movementPenalty = room.aStarMovementPenalty[x, y];

        return movementPenalty > 0;
    }
}
