using UnityEngine;

public abstract class StageBuilder
{
    public Stage stage;

    public void bindStage(Stage stage)
    {
        this.stage = stage;
    }

    public Tile getTile(Vector2 pos)
    {
        return stage.tiles[(int) pos.x, (int) pos.y];
    }

    public void setTile(Vector2 pos, TileType type)
    {
        stage.tiles[(int) pos.x, (int) pos.y].type = type;
    }

    public void fill(TileType tile)
    {
        for (var y = 0; y < stage.height; y++)
        {
            for (var x = 0; x < stage.width; x++)
            {
                stage.tiles[x ,y] = new Tile();
                setTile(new Vector2(x, y), tile);
            }
        }
    }
}
