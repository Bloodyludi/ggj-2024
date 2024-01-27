using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform dancefloor;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;

    public float TileSize => dancefloorTilePrefab.TileSize;
    
    private void Awake()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = Instantiate(dancefloorTilePrefab, dancefloor);
                tile.SetPosition(x, y);
            }
        }

        dancefloor.localPosition = -new Vector3(
            dancefloorTilePrefab.TileSize * (width - 1), 
            dancefloorTilePrefab.TileSize * (height - 1)
            ) / 2;
    }


    public GameResult GetGameResult()
    {
        return GameResult.Draw;
    }
}