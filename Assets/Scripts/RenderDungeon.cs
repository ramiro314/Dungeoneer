﻿using UnityEngine;
using System.Collections;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class RenderDungeon : NetworkBehaviour {

    private Vector3[] _vertices;
    private bool[,] _placedObjects;

    private Object _floorPrefab;
    private Object _wallPrefab;
    private Object _coinPrefab;
    private Object _speedPotionPrefab;
    private Object _stealSpellPrefab;

    private Dungeon _dungeon;

    private float itemHeigh = 0.25f;

    private void Generate()
    {
        _floorPrefab = Resources.Load("Prefabs/Floor");
        _wallPrefab = Resources.Load("Prefabs/Wall");
        _coinPrefab = Resources.Load("Prefabs/Coin");
        _speedPotionPrefab = Resources.Load("Prefabs/SpeedPotion");
        _stealSpellPrefab = Resources.Load("Prefabs/StealSpell");
        _placedObjects = new bool[_dungeon.stage.width, _dungeon.stage.height];
        AddRoomsFloors();
        AddCorridorFloors();
        AddWalls();
        AddCoins();
        AddItems();
    }

    private void AddItems()
    {
        Quaternion forward = Quaternion.LookRotation(Vector3.forward);

        int speedPotionNumber = NetworkGameManager.sInstance.playerNumber;
        int placedSpeedPotions = 0;

        while (placedSpeedPotions < speedPotionNumber)
        {
            int x = Helpers.RandomRange(1, _dungeon.stage.width - 1);
            int y = Helpers.RandomRange(1, _dungeon.stage.height - 1);

            var tile = _dungeon.stage.tiles[x, y];

            if ((tile.type == Tiles.corridor || tile.type == Tiles.floor) && !_placedObjects[x, y])
            {
                GameObject speedPotion = (GameObject)Instantiate(
                    _speedPotionPrefab, // Prefab ref
                    new Vector3( // Position
                        x + 0.5f, // Fix possition based on scale
                        itemHeigh,
                        y + 0.5f // Fix possition based on scale
                    ),
                    forward,
                    transform // Set this object as parent
                );
                NetworkServer.Spawn(speedPotion);

                placedSpeedPotions++;
                _placedObjects[x, y] = true;
            }
        }

        int stealSpellNumber = NetworkGameManager.sInstance.playerNumber;
        int placedStealSpell = 0;

        while (placedStealSpell < stealSpellNumber)
        {
            int x = Helpers.RandomRange(1, _dungeon.stage.width - 1);
            int y = Helpers.RandomRange(1, _dungeon.stage.height - 1);

            var tile = _dungeon.stage.tiles[x, y];

            if ((tile.type == Tiles.corridor || tile.type == Tiles.floor) && !_placedObjects[x, y])
            {
                GameObject stealSpell = (GameObject)Instantiate(
                    _stealSpellPrefab, // Prefab ref
                    new Vector3( // Position
                        x + 0.5f, // Fix possition based on scale
                        itemHeigh,
                        y + 0.5f // Fix possition based on scale
                    ),
                    forward,
                    transform // Set this object as parent
                );
                NetworkServer.Spawn(stealSpell);

                placedStealSpell++;
                _placedObjects[x, y] = true;
            }
        }
    }

    private void AddCoins()
    {
        int coinNumber = NetworkGameManager.sInstance.playerNumber * NetworkGameManager.sInstance.coinsToWin;
        int placedCoins = 0;
        Quaternion forward = Quaternion.LookRotation(Vector3.forward);

        while (placedCoins < coinNumber)
        {
            int x = Helpers.RandomRange(1, _dungeon.stage.width - 1);
            int y = Helpers.RandomRange(1, _dungeon.stage.height - 1);

            var tile = _dungeon.stage.tiles[x, y];

            if ((tile.type == Tiles.corridor || tile.type == Tiles.floor) && !_placedObjects[x, y])
            {
                GameObject coin = (GameObject)Instantiate(
                    _coinPrefab, // Prefab ref
                    new Vector3( // Position
                        x + 0.5f, // Fix possition based on scale
                        itemHeigh,
                        y + 0.5f // Fix possition based on scale
                    ),
                    forward,
                    transform // Set this object as parent
                );
                NetworkServer.Spawn(coin);

                placedCoins++;
                _placedObjects[x, y] = true;
            }
        }
    }

    private void AddCorridorFloors()
    {
        ArrayList corridor = new ArrayList();

        // Vertical corridors.
        for (int x = 1; x <= _dungeon.stage.width - 1; x++)
        {
            for (int y = 1; y <= _dungeon.stage.height - 1; y++)
            {
                var tile = _dungeon.stage.tiles[x, y];

                if (tile.type == Tiles.corridor)
                {
                    if (corridor.Count == 0)
                    {
                        corridor.Add(new Vector2(x, y));
                    }
                    else
                    {
                        corridor.Insert(1, new Vector2(x, y));
                    }
                }
                else
                {
                    if (corridor.Count == 0) continue;
                    if (corridor.Count == 1)
                    {
                        corridor.Clear();
                        continue;
                    }
                    corridor.Insert(1, new Vector2(x, y));
                    CreateCorridor(corridor);
                    foreach (Vector2 cTile in corridor)
                    {
                        _dungeon.stage.tiles[(int) cTile.x, (int) cTile.y].visited = true;
                    }
                    corridor.Clear();
                }
            }
        }

        // Horizontal corridors.
        for (int y = 1; y <= _dungeon.stage.height - 1; y++)
        {
            for (int x = 1; x <= _dungeon.stage.width - 1; x++)
            {
                var tile = _dungeon.stage.tiles[x, y];

                if (tile.type == Tiles.corridor && !tile.visited)
                {
                    if (corridor.Count == 0)
                    {
                        corridor.Add(new Vector2(x, y));
                    }
                    else
                    {
                        corridor.Insert(1, new Vector2(x, y));
                    }
                }
                else
                {
                    if (corridor.Count == 0) continue;
                    corridor.Insert(1, new Vector2(x, y));
                    CreateCorridor(corridor);
                    corridor.Clear();
                }
            }
        }
    }

    private void AddWalls()
    {
        ArrayList wall = new ArrayList();

        // Vertical walls. Iterate all but the last.
        for (int x = 0; x < _dungeon.stage.width - 1; x++)
        {
            for (int y = 0; y <= _dungeon.stage.height - 1; y++)
            {
                var tileA = new Vector2(x, y);
                var tileB = new Vector2(x + 1, y);

                if (HasWall(tileA, tileB))
                {
                    if (wall.Count == 0)
                    {
                        wall.Add(tileB);
                    }
                    else
                    {
                        wall.Insert(1, tileB);
                    }
                }
                else
                {
                    if (wall.Count == 0) continue;
                    wall.Insert(1, tileB);
                    CreateWall(wall);
                    wall.Clear();
                }
            }
        }

        // Horizontal walls. Iterate all but the last.
        for (int y = 0; y < _dungeon.stage.height - 1; y++)
        {
            for (int x = 0; x <= _dungeon.stage.width - 1; x++)
            {
                var tileA = new Vector2(x, y);
                var tileB = new Vector2(x, y + 1);

                if (HasWall(tileA, tileB))
                {
                    if (wall.Count == 0)
                    {
                        wall.Add(tileB);
                    }
                    else
                    {
                        wall.Insert(1, tileB);
                    }
                }
                else
                {
                    if (wall.Count == 0) continue;
                    wall.Insert(1, tileB);
                    CreateWall(wall);
                    wall.Clear();
                }
            }
        }
    }

    private void AddRoomsFloors()
    {
        Quaternion up = Quaternion.LookRotation(Vector3.up);
        foreach (var room in _dungeon.rooms)
        {
            GameObject newFloor = (GameObject)Instantiate(
                _floorPrefab, // Prefab ref
                new Vector3( // Position
                    room.x + room.width * 0.5f, // Fix possition based on scale
                    0,
                    room.y + room.height * 0.5f // Fix possition based on scale
                ),
                up, // Face up, since its a _floorPrefab
                transform // Set this object as parent
            );
            FixScaling fixScale = newFloor.GetComponent<FixScaling>();
            fixScale.scale = new Vector3(room.width, room.height, 1f); // Fix the size scaling
            NetworkServer.Spawn(newFloor);

            // Set spawning point
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.parent = newFloor.transform;
            spawnPoint.transform.position = newFloor.transform.position;
            NetworkManager.RegisterStartPosition(spawnPoint.transform);
        }
    }

    private void CreateCorridor(ArrayList corridor)
    {
        Vector2 corridorStart = (Vector2) corridor[0];
        Vector2 corridorEnd = (Vector2) corridor[1];
        var corridorLength = corridorEnd.x - corridorStart.x + corridorEnd.y - corridorStart.y;

        float xScale;
        float yScale;
        var corridorDirection = corridorEnd - corridorStart;
        if (corridorDirection.x > 0)
        {
            xScale = corridorLength;
            yScale = 1f;
        }
        else
        {
            xScale = 1f;
            yScale = corridorLength;
        }

        GameObject newCorridor = (GameObject) Instantiate(
            _floorPrefab, // Prefab ref
            new Vector3( // Position
                corridorStart.x + xScale * 0.5f,
                0,
                corridorStart.y + yScale * 0.5f
            ),
            Quaternion.LookRotation(Vector3.up), // Face up, since its a _floorPrefab
            transform // Set this object as parent
        );
        FixScaling fixScale = newCorridor.GetComponent<FixScaling>();
        fixScale.scale = new Vector3(xScale, yScale, 1f); // Fix the size scaling
        NetworkServer.Spawn(newCorridor);
    }

    private void CreateWall(ArrayList wall)
    {
        Vector2 wallStart = (Vector2) wall[0];
        Vector2 wallEnd = (Vector2) wall[1];
        var wallLength = wallEnd.x - wallStart.x + wallEnd.y - wallStart.y;

        float x;
        float y;
        var wallDirection = wallEnd - wallStart;
        if (wallDirection.x > 0)
        {
            x = wallStart.x + wallLength * 0.5f;
            y = wallStart.y;
        }
        else
        {
            x = wallStart.x;
            y = wallStart.y + wallLength * 0.5f;
        }

        GameObject newWall = (GameObject) Instantiate(
            _wallPrefab, // Prefab ref
            new Vector3( // Position
                x,
                1f,
                y
            ),
            GetWallRotation(wall), // Face up, since its a _floorPrefab
            transform // Set this object as parent
        );
        FixScaling fixScale = newWall.GetComponent<FixScaling>();
        fixScale.scale = new Vector3(wallLength, 2f, 1f); // Fix the size scaling
        NetworkServer.Spawn(newWall);
    }

    private bool HasWall(Vector2 a, Vector2 b)
    {
        var aType = _dungeon.stage.tiles[(int)a.x, (int)a.y].type;
        var bType = _dungeon.stage.tiles[(int)b.x, (int)b.y].type;

        return (aType == Tiles.wall || bType == Tiles.wall) && aType != bType;
    }

    private Quaternion GetWallRotation(ArrayList wall)
    {
        Vector2 wallStart = (Vector2) wall[0];
        Vector2 wallEnd = (Vector2) wall[1];

        var mergedVector = wallEnd - wallStart;
        mergedVector.Normalize();
        var previousCellDirection = new Vector2(mergedVector.y, mergedVector.x);
        var previousCell = wallStart - previousCellDirection;

        var aType = _dungeon.stage.tiles[(int)wallStart.x, (int)wallStart.y].type;

        if (aType == Tiles.wall)
        {
            return LookFromAtoB(wallStart, previousCell);

        }
        return LookFromAtoB(previousCell, wallStart);
    }

    private Quaternion LookFromAtoB(Vector2 a, Vector2 b)
    {
        return Quaternion.LookRotation(
            new Vector3(b.x, 0, b.y) -
            new Vector3(a.x, 0, a.y)
        );
    }

    private void Start()
    {
        Debug.Log("Generating Dungeon");
        int size = NetworkGameManager.sInstance.playerNumber * 10 + 1;
        Stage stage = new Stage(size, size);
        _dungeon = new Dungeon();
        _dungeon.generate(stage);
        Generate();
    }
}
