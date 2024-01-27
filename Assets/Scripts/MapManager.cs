using UnityEngine;

public partial class MapManager : MonoBehaviour
{
    public Transform dancefloor;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;

    public float TileSize => dancefloorTilePrefab.TileSize;

    public DancefloorTile[] tiles;
    private BeatManager beatManager;

    private void Awake()
    {
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();
        tiles = new DancefloorTile[width * height];
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = Instantiate(dancefloorTilePrefab, dancefloor);
                tile.SetPosition(x, y);
                
                // Create a deadly tile for testing
                tile.SetDeadly(x == 2 && y == 2 || x == 2 && y == 3 || x == 2 && y == 4);

                tiles[GetTileIndex(tile.position)] = tile;
            }
        }

        dancefloor.localPosition = -new Vector3(
            dancefloorTilePrefab.TileSize * (width - 1), 
            dancefloorTilePrefab.TileSize * (height - 1)
            ) / 2;

        beatManager.OnBeatUpdate -= UpdateDeadlyTilePositions;
        beatManager.OnBeatUpdate += UpdateDeadlyTilePositions;
    }

    public GameResult GetGameResult()
    {
        return GameResult.Draw;
    }
}