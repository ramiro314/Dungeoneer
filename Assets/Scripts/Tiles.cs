/*
Algoritmo de Dungeon
Objeto Stage que guarda la informacion del dungeon generado.
*/
using UnityEngine;

public static class Tiles
{
    public static TileType floor;
    public static TileType corridor;
    public static TileType wall;
    public static TileType openDoor;
    public static TileType closedDoor;

    static Tiles()
    {
        // Define the tile types.
        floor = new TileType("floor", true, Color.white);

        corridor = new TileType("corridor", true, Color.white);

        wall = new TileType("wall", false, Color.black);

        openDoor = new TileType("open door", true, Color.blue);
        closedDoor = new TileType("closed door", false, Color.red);

        openDoor.closesTo = closedDoor;
        closedDoor.opensTo = openDoor;

    }
}
