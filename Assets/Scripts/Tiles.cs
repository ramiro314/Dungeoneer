using UnityEngine;

public static class Tiles
{
    public static TileType floor;
    public static TileType corridor;
    public static TileType wall;
    public static TileType lowWall;
    public static TileType table;
    public static TileType openDoor;
    public static TileType closedDoor;
    public static TileType stairs;

    public static TileType grass;
    public static TileType tree;
    public static TileType treeAlt1;
    public static TileType treeAlt2;

    static Tiles()
    {
        // Define the tile types.
        floor = new TileType("floor", true, true, Color.white);

        corridor = new TileType("corridor", true, true, Color.white);

        wall = new TileType("wall", false, false, Color.black);

        table = new TileType("table", false, true, Color.cyan);

        lowWall = new TileType("low wall", false, true, Color.grey);

        openDoor = new TileType("open door", true, true, Color.blue);
        closedDoor = new TileType("closed door", false, false, Color.red);

        openDoor.closesTo = closedDoor;
        closedDoor.opensTo = openDoor;

        stairs = new TileType("stairs", true, true, Color.yellow);

        grass = new TileType("grass", true, true, Color.green);

        tree = new TileType("tree", false, false, Color.cyan);
    }
}
