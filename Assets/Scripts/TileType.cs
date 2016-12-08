using UnityEngine;

public class TileType
{
    public string name;
    public bool isPassable;
    public bool isTransparent;
    public Color appearance;

    public TileType opensTo;
    public TileType closesTo;

    public TileType(string name, bool isPassable, bool isTransparent, Color appearance)
    {
        this.name = name;
        this.isPassable = isPassable;
        this.isTransparent = isTransparent;
        this.appearance = appearance;
    }

}
