/*
Algoritmo de Dungeon
Define las propiedades de un TileType
*/
using UnityEngine;

public class TileType
{
    public string name;
    public bool isPassable;
    public Color appearance;

    public TileType opensTo;
    public TileType closesTo;

    public TileType(string name, bool isPassable, Color appearance)
    {
        this.name = name;
        this.isPassable = isPassable;
        this.appearance = appearance;
    }

}
