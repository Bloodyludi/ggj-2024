using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class MapManager : MonoBehaviour
{
    public Transform dancefloor;
    private BeatManager beatManager;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;

    public float TileSize => dancefloorTilePrefab.TileSize;

    public DancefloorTile[] tiles;

    public Dictionary<Vector2Int, List<PlayerController>> TileOccupationDictionary = new Dictionary<Vector2Int, List<PlayerController>>();

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

        AddListeners();
    }

    private void AddListeners()
    {
        beatManager = FindObjectOfType<BeatManager>();
        beatManager.OnBeatUpdate -= ResolveBoardCollisions;
        beatManager.OnBeatUpdate += ResolveBoardCollisions;
    }

    private void ResolveBoardCollisions()
    {
        foreach (var occupiedCell in TileOccupationDictionary)
        {
            if (occupiedCell.Value == null || occupiedCell.Value.Count <= 1)
            {
                continue;
            }

            foreach (var player in occupiedCell.Value)
            {
              //   player.StunPlayer();
            }
        }
    }

    public GameResult GetGameResult()
    {
        return GameResult.Draw;
    }

    public void OnPlayerMovementFinished(Vector2 from, Vector2 to, PlayerController player)
    {
        var newPlayerTile = WorldToMap(to);
        var oldPlayerTile = WorldToMap(from);

        MovePlayerToTile(player, oldPlayerTile, newPlayerTile);
        HandleSameTileOccupancy(newPlayerTile);
    }

    private void HandleSameTileOccupancy(Vector2Int newPlayerTile)
    {
        if (TileOccupationDictionary[newPlayerTile].Count > 1)
        {
            foreach (var stunnedPlayer in TileOccupationDictionary[newPlayerTile])
            {
                //HandleFightCloudAnimation
            }
        }
    }

    private void MovePlayerToTile(PlayerController player, Vector2Int oldPlayerTile, Vector2Int newPlayerTile)
    {
        if (TileOccupationDictionary.ContainsKey(oldPlayerTile) && TileOccupationDictionary[oldPlayerTile].Contains(player))
        {
            Debug.Log($"removing {player.name} from {oldPlayerTile}");
            TileOccupationDictionary[oldPlayerTile].Remove(player);
        }

        if (TileOccupationDictionary.ContainsKey(newPlayerTile) == false)
        {
            TileOccupationDictionary.Add(newPlayerTile, new List<PlayerController>());
        }

        Debug.Log($"moving {player.name} to {newPlayerTile}");

        TileOccupationDictionary[newPlayerTile].Add(player);
    }

    public Vector2Int WorldToMap(Vector2 worldPos)
    {
        var boardOrigin = GetBoardOrigin();

        float boardWith = this.width * TileSize;
        float boardHeight = this.height * TileSize;

        float column = (worldPos.x - boardOrigin.x) / (boardWith - boardOrigin.x) * boardWith - 1;
        float row = (worldPos.y - boardOrigin.y) / (boardHeight - boardOrigin.y) * boardHeight - 1;
        Debug.Log($"Player at ({row}, {column}");

        return new Vector2Int(Mathf.FloorToInt(row), Mathf.FloorToInt(column));
    }
    
    public Vector2 MapToWorld(int row, int column)
    {
        var boardOrigin = GetBoardOrigin();
        Vector2Int cell = new Vector2Int(column, row);
        return boardOrigin + (cell + Vector2.one * 0.5f) * TileSize;

    }

    private Vector2 GetBoardOrigin()
    {
        Vector2 boardOrigin = dancefloor.localToWorldMatrix * dancefloor.localPosition;
        boardOrigin -= Vector2.one * TileSize;
        return boardOrigin;
    }
}