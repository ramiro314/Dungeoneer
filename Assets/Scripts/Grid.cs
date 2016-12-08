using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {

    public int xSize, ySize;

    private Vector3[] vertices;

    private Mesh mesh;

    private Dungeon _dungeon;

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(_dungeon.stage.width + 1) * (_dungeon.stage.height + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y <= _dungeon.stage.height; y++)
        {
            for (int x = 0; x <= _dungeon.stage.width; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                uv[i] = new Vector2(
                    (float) x / _dungeon.stage.width,
                    (float) y / _dungeon.stage.height
                );
                tangents[i] = tangent;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] floor_triangles = new int[_dungeon.stage.width * _dungeon.stage.height * 6];
        int[] corridor_triangles = new int[_dungeon.stage.width * _dungeon.stage.height * 6];
        int[] wall_triangles = new int[_dungeon.stage.width * _dungeon.stage.height * 6];
        for (int ti = 0, vi = 0, y = 0; y < _dungeon.stage.height; y++, vi++)
        {
            for (int x = 0; x < _dungeon.stage.width; x++, ti += 6, vi++)
            {
                if (_dungeon.stage.tiles[x, y].type == Tiles.floor)
                {
                    floor_triangles[ti] = vi;
                    floor_triangles[ti + 3] = floor_triangles[ti + 2] = vi + 1;
                    floor_triangles[ti + 4] = floor_triangles[ti + 1] = vi + _dungeon.stage.width + 1;
                    floor_triangles[ti + 5] = vi + _dungeon.stage.width + 2;
                }
                else if (_dungeon.stage.tiles[x, y].type == Tiles.corridor)
                {
                    corridor_triangles[ti] = vi;
                    corridor_triangles[ti + 3] = corridor_triangles[ti + 2] = vi + 1;
                    corridor_triangles[ti + 4] = corridor_triangles[ti + 1] = vi + _dungeon.stage.width + 1;
                    corridor_triangles[ti + 5] = vi + _dungeon.stage.width + 2;
                }
                else
                {
                    wall_triangles[ti] = vi;
                    wall_triangles[ti + 3] = wall_triangles[ti + 2] = vi + 1;
                    wall_triangles[ti + 4] = wall_triangles[ti + 1] = vi + _dungeon.stage.width + 1;
                    wall_triangles[ti + 5] = vi + _dungeon.stage.width + 2;
                }
            }
        }
        mesh.subMeshCount = 3;
        mesh.SetTriangles(floor_triangles, 0);
        mesh.SetTriangles(corridor_triangles, 1);
        mesh.SetTriangles(wall_triangles, 2);
        mesh.RecalculateNormals();
    }

    private void Awake()
    {
        Stage stage = new Stage(xSize, ySize);
        _dungeon = new Dungeon();
        _dungeon.generate(stage);
//        Debug.Log(_dungeon.debug_connectors.Count);
        Generate();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;
    }

    private void OnDrawGizmos()
    {
        if (_dungeon == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        foreach (var connector in _dungeon.debug_connectors)
        {
            Gizmos.DrawSphere(connector, 0.1f);
        }
    }
}
