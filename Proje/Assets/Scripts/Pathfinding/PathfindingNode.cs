using System;
using UnityEngine;

public class PathfindingNode : IComparable<PathfindingNode>
{
    public Vector2Int PositionOnGrid { get; }
    public int StartCost { get; private set; } 
    public int EndCost { get; private set; }
    public PathfindingNode PreviousNode { get; set; }

    private int _totalCost = int.MaxValue;

    public PathfindingNode(Vector2Int positionOnGrid)
    {
        PositionOnGrid = positionOnGrid;
        PreviousNode = null;
        _totalCost = int.MaxValue; 
    }

    public int TotalCost
    {
        get
        {
            if (_totalCost == int.MaxValue)
            {
                _totalCost = StartCost + EndCost;
            }
            return _totalCost;
        }
    }

    public void UpdateStartCost(int cost)
    {
        StartCost = cost;
        _totalCost = int.MaxValue; 
    }

    public void UpdateEndCost(int cost)
    {
        EndCost = cost;
        _totalCost = int.MaxValue; 
    }

    public int CompareTo(PathfindingNode nodeToCompare)
    {
        int compare = TotalCost.CompareTo(nodeToCompare.TotalCost);

        if (compare == 0)
        {
            compare = EndCost.CompareTo(nodeToCompare.EndCost); 
        }

        return compare;
    }
}
