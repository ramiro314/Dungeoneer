/*
Algoritmo de Dungeon
En esta clase se definen constantes que representan las direcciones cardinales en un plano 2D.
*/
using System.Collections.Generic;
using UnityEngine;

public static class Directions
{
    public static readonly Vector2 E;
    public static readonly Vector2 N;
    public static readonly Vector2 W;
    public static readonly Vector2 S;

    public static readonly Vector2 NE;
    public static readonly Vector2 NW;
    public static readonly Vector2 SE;
    public static readonly Vector2 SW;

    public static readonly Vector2 None;

    public static readonly List<Vector2> All;
    public static readonly List<Vector2> Cardinal;
    public static readonly List<Vector2> Intercardinal;


    static Directions()
    {
        E = new Vector2(1, 0);
        N = new Vector2(0, 1);
        W = new Vector2(-1, 0);
        S = new Vector2(0, -1);

        NE = new Vector2(1, 1);
        NW = new Vector2(-1, 1);
        SE = new Vector2(1, -1);
        SW = new Vector2(-1, -1);

        None = new Vector2(0, 0);

        All = new List<Vector2>{N, NE, E, SE, S, SW, W, NW};
        Cardinal = new List<Vector2>{N, E, S, W};
        Intercardinal = new List<Vector2>{NE, SE, SW, NW};
    }
}
