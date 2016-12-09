/*
Algoritmo de Dungeon
Extendiendo de StageBuilder, define el algoritmo para generar Dungeons en un plano 2D.
Esta es una implementacion en C# del algoritmo descrito en este blog post:
http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dungeon : StageBuilder
{
    public List<Rect> rooms;

    private int[,] _regions;

    private int numRoomTries;
    private int roomExtraSize;
    private int windingPercent;


    // The index of the current region being carved.
    private int _currentRegion;

    private int _extraConnectorChance;

    public Dungeon()
    {
        rooms = new List<Rect>();
        numRoomTries = 200;
        roomExtraSize = 0;
        windingPercent = 0;
        _currentRegion = 0;
        _extraConnectorChance = 20;
    }


    public void generate(Stage stage)
    {
        // Validate sizes
        if (stage.width % 2 == 0 || stage.height % 2 == 0)
        {
            throw new ArgumentException(String.Format("The stage must be odd-sized. W: {0} H: {1}",
                stage.width, stage.height));
        }

        // Initialize stage
        bindStage(stage);
        fill(Tiles.wall);
        _regions = new int[stage.width, stage.height];

        _addRooms();
        _addMazes();
        _connectRegions();
        _removeDeadEnds();

    }

    private void _addMazes()
    {
        // Fill in all of the empty space with mazes.
        for (var y = 1; y < stage.bounds.height; y += 2)
        {
            for (var x = 1; x < stage.bounds.width; x += 2)
            {
                var pos = new Vector2(x, y);
                if (getTile(pos).type != Tiles.wall) continue;
                _growMaze(pos);
            }
        }
    }

    // Implementation of the "growing tree" algorithm from here:
    // http://www.astrolog.org/labyrnth/algrithm.htm
    private void _growMaze(Vector2 start)
    {
        var cells = new List<Vector2>();
        Vector2 lastDir = Directions.None;

        _startRegion();
        _carve(start, Tiles.corridor);

        cells.Add(start);
        while (cells.Count > 0)
        {
            var cell = cells.Last();

            // See which adjacent cells are open.
            var unmadeCells = new List<Vector2>();

            foreach (var dir in Directions.Cardinal)
            {
                if (_canCarve(cell, dir)) unmadeCells.Add(dir);
            }

            if (unmadeCells.Count > 0)
            {
                // Based on how "windy" passages are, try to prefer carving in the
                // same direction.
                Vector2 dir;
                if (unmadeCells.Contains(lastDir) && Random.Range(0, 100) > windingPercent)
                {
                    dir = lastDir;
                }
                else
                {
                    var dirIndex = Random.Range(0, unmadeCells.Count);
                    dir = unmadeCells.ElementAt(dirIndex);
                    unmadeCells.RemoveAt(dirIndex);
                }

                _carve(cell + dir, Tiles.corridor);
                _carve(cell + dir * 2, Tiles.corridor);

                cells.Add(cell + dir * 2);
                lastDir = dir;
            }
            else
            {
                // No adjacent uncarved cells.
                cells.Remove(cells.Last());

                // This path has ended.
                lastDir = Directions.None;
            }
        }
    }

    /// Places rooms ignoring the existing maze corridors.
    private void _addRooms()
    {
        for (var i = 0; i < numRoomTries; i++)
        {
            // Pick a random room size. The funny math here does two things:
            // - It makes sure rooms are odd-sized to line up with maze.
            // - It avoids creating rooms that are too rectangular: too tall and
            //   narrow or too wide and flat.
            // TODO: This isn't very flexible or tunable. Do something better here.
            var size = Random.Range(1, 3 + roomExtraSize) * 2 + 1;
            var rectangularity = Random.Range(0, 1 + size / 2) * 2;
            var width = size;
            var height = size;

            if (Helpers.OneIn(2))
            {
                width += rectangularity;
            }
            else
            {
                height += rectangularity;
            }

            var x = (int) Random.Range(0, (stage.bounds.width - width) / 2) * 2 + 1;
            var y = (int) Random.Range(0, (stage.bounds.height - height) / 2) * 2 + 1;

            var room = new Rect(x, y, width, height);

            var overlaps = false;
            foreach (var other in rooms)
            {
                overlaps = room.Overlaps(other);
                if (overlaps) break;
            }

            if (overlaps) continue;

            rooms.Add(room);

            _startRegion();
            foreach (var pos in Helpers.GetRoomTiles(room))
            {
                _carve(pos, Tiles.floor);
            }
        }
    }

    private void _connectRegions()
    {
        // Find all of the tiles that can connect two (or more) regions.
        // (TilePosition, [(int)regions])
        var connectorRegions = new Dictionary<Vector2, HashSet<int>>();

        foreach (var pos in Helpers.GetRoomTiles(Helpers.InflateRect(stage.bounds, -1)))
        {
            // If its not a wall, then it cannot be a connector
            if (getTile(pos).type != Tiles.wall) continue;

            var regions = new HashSet<int>();
            foreach (var dir in Directions.Cardinal)
            {
                var newPos = pos + dir;
                var region = _regions[(int) newPos.x, (int) newPos.y];
                if (region != 0) regions.Add(region);
            }

            // If it cannot connect 2 or more regions its not a connector
            if (regions.Count < 2) continue;

            connectorRegions[pos] = regions;
        }

        // Keep track of which regions have been merged. This maps an original
        // region index to the one it has been merged to.
        var merged = new Dictionary<int, int>();
        var openRegions = new HashSet<int>();
        for (var i = 0; i <= _currentRegion; i++)
        {
            merged[i] = i;
            openRegions.Add(i);
        }

        // Keep connecting regions until we're down to one.
        while (openRegions.Count > 2)
        {
            // Choose random connector
            var connectors = connectorRegions.Keys.ToList();
            var conIndex = Random.Range(0, connectors.Count);
            var connector = connectors.ElementAt(conIndex);

            // Carve the connection.
            _addJunction(connector);

            // Merge the connected regions. We'll pick one region (arbitrarily) and
            // map all of the other regions to its index.
            // Try this with a Select(lambda)
            HashSet<int> regions = new HashSet<int>();
            foreach (int cRegion in connectorRegions[connector])
            {
                regions.Add(merged[cRegion]);
            }
            var dest = regions.First();
            var sources = regions.Skip(1).ToList();

            // Merge all of the affected regions. We have to look at *all* of the
            // regions because other regions may have previously been merged with
            // some of the ones we're merging now.
            for (var i = 0; i <= _currentRegion; i++)
            {
                if (sources.Contains(merged[i]))
                {
                    merged[i] = dest;
                }
            }

            // The sources are no longer in use.
            foreach (var source in sources)
            {
                openRegions.Remove(source);
            }

            // Remove any connectors that aren't needed anymore.
            List<Vector2> notNeededConnectors = new List<Vector2>();
            foreach (var con in connectors)
            {
                // Don't allow connectors right next to each other.
                if (Vector2.Distance(connector, con) < 2)
                {
                    notNeededConnectors.Add(con);
                    continue;
                }

                // If the connector no long spans different regions, we don't need it.
                var l_regions = new HashSet<int>(connectorRegions[con].Select(region => merged[region]));

                if (l_regions.Count > 1) continue;

                // This connecter isn't needed, but connect it occasionally so that the
                // dungeon isn't singly-connected.
                if (Helpers.OneIn(_extraConnectorChance)) _addJunction(con);

                notNeededConnectors.Add(con);
            }

            foreach (var notNeededConnector in notNeededConnectors)
            {
                connectorRegions.Remove(notNeededConnector);
            }
        }
    }

    void _addJunction(Vector2 pos)
    {
        if (Helpers.OneIn(4))
        {
            setTile(pos, Helpers.OneIn(3) ? Tiles.openDoor : Tiles.floor);
        }
        else
        {
            setTile(pos, Tiles.closedDoor);
        }
        // Until I have textures for the doors:
        setTile(pos, Tiles.corridor);
    }

    void _removeDeadEnds()
    {
        var done = false;

        while (!done)
        {
            done = true;

            foreach (var pos in Helpers.GetRoomTiles(Helpers.InflateRect(stage.bounds, -1)))
            {
                if (getTile(pos).type == Tiles.wall) continue;

                // If it only has one exit, it's a dead end.
                var exits = 0;
                foreach (var dir in Directions.Cardinal)
                {
                    if (getTile(pos + dir).type != Tiles.wall) exits++;
                }

                if (exits != 1) continue;

                done = false;
                setTile(pos, Tiles.wall);
            }
        }
    }

    /// Gets whether or not an opening can be carved from the given starting
    /// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
    /// if the starting Cell is in bounds and the destination Cell is filled
    /// (or out of bounds).
    private bool _canCarve(Vector2 pos, Vector2 direction)
    {
        // Must end in bounds.
        if (!stage.bounds.Contains(pos + direction * 3)) return false;

        // Destination must not be open.
        return getTile(pos + direction * 2).type == Tiles.wall;
    }

    private void _startRegion()
    {
        _currentRegion++;
    }

    private void _carve(Vector2 pos, TileType type = null)
    {
        if (type == null) type = Tiles.floor;
        setTile(pos, type);
        _regions[(int) pos.x, (int) pos.y] = _currentRegion;
    }


}
