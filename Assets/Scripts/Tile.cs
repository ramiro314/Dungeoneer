/*
Algoritmo de Dungeon
Objeto Tile, contiene el TileType y informacion de visita usada para la generacion de corredores.
*/
public class Tile
{
    public TileType type;
    public bool visited;

    public Tile()
    {
        visited = false;
    }
}
