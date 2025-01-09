using System;
using UnityEngine;

public class AStarNode : IComparable<AStarNode>
{
    public Vector2Int GridPosition { get; }
    public int GCost { get; private set; } // Distance from the start node
    public int HCost { get; private set; } // Distance to the finish node
    public AStarNode ParentNode { get; set; }

    // Cached FCost to avoid recalculating it every time
    private int _fCost = int.MaxValue;

    public AStarNode(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        ParentNode = null;
        _fCost = int.MaxValue; // Initialize the cached FCost value
    }

    public int FCost
    {
        get
        {
            // If FCost is not calculated yet, calculate and cache it
            if (_fCost == int.MaxValue)
            {
                _fCost = GCost + HCost;
            }
            return _fCost;
        }
    }

    public void SetGCost(int cost)
    {
        GCost = cost;
        _fCost = int.MaxValue; // Invalidate cached FCost since GCost changed
    }

    public void SetHCost(int cost)
    {
        HCost = cost;
        _fCost = int.MaxValue; // Invalidate cached FCost since HCost changed
    }

    public int CompareTo(AStarNode nodeToCompare)
    {
        // Compare by FCost first, then by HCost
        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0)
        {
            compare = HCost.CompareTo(nodeToCompare.HCost); // Tiebreak by HCost
        }

        return compare;
    }
}
