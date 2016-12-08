using UnityEngine;

public class Stage
{
    public int width;
    public int height;
    public Rect bounds;

    public Tile[,] tiles;

    public Stage(int width, int height)
    {
        this.width = width;
        this.height = height;
        bounds = new Rect(0, 0, width, height);
        tiles = new Tile[width, height];
    }

}
