/*
Basandose en el dugeon generado en la clase Dungeon crea la estructura usando prefabs.
Luego rellena el dungeon con los objetos.
*/
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters;
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

        // Set desired number of Speed Potions
        int speedPotionNumber = NetworkGameManager.sInstance.playerNumber;
        int placedSpeedPotions = 0;

        // Try to place in an empty tile until all potions are placed
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
                        x + 0.5f,
                        itemHeigh,
                        y + 0.5f
                    ),
                    forward,
                    transform // Set this object as parent
                );
                NetworkServer.Spawn(speedPotion);

                placedSpeedPotions++;
                _placedObjects[x, y] = true;
            }
        }

        // Set desired number of Steal Spell
        int stealSpellNumber = NetworkGameManager.sInstance.playerNumber;
        int placedStealSpell = 0;

        // Try to place in an empty tile until all potions are placed
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
                        x + 0.5f,
                        itemHeigh,
                        y + 0.5f
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
        // Set desired number of coins
        int coinNumber = NetworkGameManager.sInstance.playerNumber * NetworkGameManager.sInstance.coinsToWin;
        int placedCoins = 0;
        Quaternion forward = Quaternion.LookRotation(Vector3.forward);

        // Try to place in an empty tile until all coins are placed
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
                        x + 0.5f,
                        itemHeigh,
                        y + 0.5f
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
        // Don't iterate 0 and n since those always are walls.
        for (int x = 1; x <= _dungeon.stage.width - 1; x++)
        {
            for (int y = 1; y <= _dungeon.stage.height - 1; y++)
            {
                var tile = _dungeon.stage.tiles[x, y];

                // If its a corridor start counting how long vertically is
                if (tile.type == Tiles.corridor)
                {
                    // If its the first one, set the start at the position 0 of the ArrayList
                    if (corridor.Count == 0)
                    {
                        corridor.Add(new Vector2(x, y));
                    }
                    // If its not the first one, set the end at the position 1 of the ArrayList
                    else
                    {
                        corridor.Insert(1, new Vector2(x, y));
                    }
                }
                else
                {
                    // If its not a corridor and the corridor is empty continue
                    if (corridor.Count == 0) continue;
                    // If its only one tile in the corridor we don't want to render it as a vertical corridor
                    if (corridor.Count == 1)
                    {
                        corridor.Clear();
                        continue;
                    }
                    // In any other case, set this tile as the end of the corridor and create it.
                    corridor.Insert(1, new Vector2(x, y));
                    CreateCorridor(corridor);
                    // Set every tile of the corridor as a visited tile
                    foreach (Vector2 cTile in corridor)
                    {
                        _dungeon.stage.tiles[(int) cTile.x, (int) cTile.y].visited = true;
                    }
                    // Clean the corridor Array for the next corridor.
                    corridor.Clear();
                }
            }
        }

        // Horizontal corridors.
        // Don't iterate 0 and n since those always are walls.
        for (int y = 1; y <= _dungeon.stage.height - 1; y++)
        {
            for (int x = 1; x <= _dungeon.stage.width - 1; x++)
            {
                var tile = _dungeon.stage.tiles[x, y];

                // If its a corridor and its not visited start counting how long horizontally is
                if (tile.type == Tiles.corridor && !tile.visited)
                {
                    // If its the first one, set the start at the position 0 of the ArrayList
                    if (corridor.Count == 0)
                    {
                        corridor.Add(new Vector2(x, y));
                    }
                    // If its not the first one, set the end at the position 1 of the ArrayList
                    else
                    {
                        corridor.Insert(1, new Vector2(x, y));
                    }
                }
                else
                {
                    // If its not a corridor and the corridor is empty continue
                    if (corridor.Count == 0) continue;
                    // In any other case, set this tile as the end of the corridor and create it.
                    corridor.Insert(1, new Vector2(x, y));
                    CreateCorridor(corridor);
                    // Clean the corridor Array for the next corridor.
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
                // Check this tile and the ono under it
                var tileA = new Vector2(x, y);
                var tileB = new Vector2(x + 1, y);

                // Check if there should be a wall bewteen A and B
                if (HasWall(tileA, tileB))
                {
                    // Check how long it should be
                    if (wall.Count == 0)
                    {
                        wall.Add(tileB);
                    }
                    else
                    {
                        wall.Insert(1, tileB);
                    }
                }
                // If not, see if we should render one or move on
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
                // Check this tile and the one to the right
                var tileA = new Vector2(x, y);
                var tileB = new Vector2(x, y + 1);

                // Check if there should be a wall bewteen A and B
                if (HasWall(tileA, tileB))
                {
                    // Check how long it should be
                    if (wall.Count == 0)
                    {
                        wall.Add(tileB);
                    }
                    else
                    {
                        wall.Insert(1, tileB);
                    }
                }
                // If not, see if we should render one or move on
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
        // Since we have the info of the rooms lets iterate over each of them
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
            // This is to fix the scale on the clients and fix the texture
            FixScaling fixScale = newFloor.GetComponent<FixScaling>();
            fixScale.scale = new Vector3(room.width, room.height, 1f);
            // Spawn on clients
            NetworkServer.Spawn(newFloor);

            // The middle of all rooms are valid spawn points
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.parent = newFloor.transform;
            spawnPoint.transform.position = newFloor.transform.position;
            NetworkManager.RegisterStartPosition(spawnPoint.transform);
        }
    }

    private void CreateCorridor(ArrayList corridor)
    {
        // Item 0 and 1 of the array represent start and end of the corridor.
        Vector2 corridorStart = (Vector2) corridor[0];
        Vector2 corridorEnd = (Vector2) corridor[1];
        // Figure out its lenght
        var corridorLength = corridorEnd.x - corridorStart.x + corridorEnd.y - corridorStart.y;

        float xScale;
        float yScale;
        // Figure out if its horizontal or vertical and set scale properly
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
                corridorStart.x + xScale * 0.5f, // Fix possition based on scale
                0,
                corridorStart.y + yScale * 0.5f // Fix possition based on scale
            ),
            Quaternion.LookRotation(Vector3.up), // Face up, since its a _floorPrefab
            transform // Set this object as parent
        );
        // This is to fix the scale on the clients and fix the texture
        FixScaling fixScale = newCorridor.GetComponent<FixScaling>();
        fixScale.scale = new Vector3(xScale, yScale, 1f);
        NetworkServer.Spawn(newCorridor);
    }

    private void CreateWall(ArrayList wall)
    {
        // Item 0 and 1 of the array represent start and end of the wall.
        Vector2 wallStart = (Vector2) wall[0];
        Vector2 wallEnd = (Vector2) wall[1];
        // Figure out its lenght
        var wallLength = wallEnd.x - wallStart.x + wallEnd.y - wallStart.y;

        float x;
        float y;
        // Figure out if its horizontal or vertical and set position properly
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
            GetWallRotation(wall),
            transform // Set this object as parent
        );
        // This is to fix the scale on the clients and fix the texture
        FixScaling fixScale = newWall.GetComponent<FixScaling>();
        fixScale.scale = new Vector3(wallLength, 2f, 1f);
        NetworkServer.Spawn(newWall);
    }

    private bool HasWall(Vector2 a, Vector2 b)
    {
        var aType = _dungeon.stage.tiles[(int)a.x, (int)a.y].type;
        var bType = _dungeon.stage.tiles[(int)b.x, (int)b.y].type;

        // If any of the two tiles is a wall and they are different,
        // we should place a wall in there.
        return (aType == Tiles.wall || bType == Tiles.wall) && aType != bType;
    }

    private Quaternion GetWallRotation(ArrayList wall)
    {
        // Item 0 and 1 of the array represent start and end of the wall.
        Vector2 wallStart = (Vector2) wall[0];
        Vector2 wallEnd = (Vector2) wall[1];

        // Create a normalized vector looking to the start of the wall.
        var mergedVector = wallEnd - wallStart;
        mergedVector.Normalize();
        // Get the coordinates of the previous tile
        var previousTile = wallStart - new Vector2(mergedVector.y, mergedVector.x);

        var aType = _dungeon.stage.tiles[(int)wallStart.x, (int)wallStart.y].type;

        // Get wall faceing direction based on that.
        if (aType == Tiles.wall)
        {
            return LookFromAtoB(wallStart, previousTile);

        }
        return LookFromAtoB(previousTile, wallStart);
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
