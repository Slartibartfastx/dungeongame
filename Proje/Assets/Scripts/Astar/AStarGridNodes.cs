using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AStarGridNodes
{
    private int width;
    private int height;

    private AStarNode[,] gridNode;

    public AStarGridNodes(int width, int height)
    {
        this.width = width;
        this.height = height;

        gridNode = new AStarNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridNode[x, y] = new AStarNode(new Vector2Int(x, y));
            }
        }
    }

    public AStarNode GetGridNode(int xPosition, int yPosition)
    {
        if (xPosition < width && yPosition < height)
        {
            return gridNode[xPosition, yPosition];
        }
        else
        {
            Debug.Log("Requested grid node is out of range");
            return null;
        }
    }
}
