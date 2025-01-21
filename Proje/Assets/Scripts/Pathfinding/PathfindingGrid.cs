using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathfindingGrid
{
    private int gridWidth;
    private int gridHeight;

    private PathfindingNode[,] nodes;

    public PathfindingGrid(int gridWidth, int gridHeight)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        nodes = new PathfindingNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                nodes[x, y] = new PathfindingNode(new Vector2Int(x, y));
            }
        }
    }

    public PathfindingNode GetNode(int x, int y)
    {
        if (x < gridWidth && y < gridHeight)
        {
            return nodes[x, y];
        }
        else
        {
            return null;
        }
    }
}
