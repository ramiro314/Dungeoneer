using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static Random rnd;

    static Helpers()
    {
        rnd = new Random();
    }

    public static int RandomRange(int min, int max)
    {
        return Random.Range(min, max);
    }

    public static bool OneIn(int chance)
    {
        return 1 == Random.Range(1, chance);
    }

    public static List<Vector2> GetRoomTiles(Rect room)
    {
        List<Vector2> tiles = new List<Vector2>();
        for (int x = (int) room.x ; x < room.x + room.width ; x++)
        {
            for (int y = (int) room.y; y < room.y + room.height; y++)
            {
                tiles.Add(new Vector2(x, y));
            }
        }
        return tiles;
    }

    public static Rect InflateRect(Rect r, int distance)
    {
        var rect = new Rect(r.x - distance, r.y - distance, r.width + distance * 2, r.height + distance * 2);
//        MonoBehaviour.print(rect.ToString());
        return rect;
    }
}