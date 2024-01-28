using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject HustleCloud;
    public Transform dancefloor;
    private BeatManager beatManager;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    private Dictionary<Vector2Int, GameObject> spawnedHustles = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;

    public float TileSize => dancefloorTilePrefab.TileSize;

    public DancefloorTile[] tiles;

    private List<PlayerController> playersInMap = new List<PlayerController>();

    private List<Vector2> stunDirections { get; } =
        new List<Vector2>()
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0),
        };

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
                //tile.SetDeadly(x == 2 && y == 2 || x == 2 && y == 3 || x == 2 && y == 4);

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

        beatManager.OnBeatUpdate -= UpdateDeadlyTilePositions;
        beatManager.OnBeatUpdate += UpdateDeadlyTilePositions;
    }

    private void ResolveBoardCollisions()
    {
        var tileOccupationDictionary = GetTileOccupancy();
        foreach (var occupiedCell in tileOccupationDictionary)
        {
            if (occupiedCell.Value.Count <= 1)
            {
                continue;
            }

            var directions = stunDirections.OrderBy(_ => Guid.NewGuid()).ToList();
            foreach (var player in occupiedCell.Value)
            {
                player.StunPlayer(directions[0]);
                directions.RemoveAt(0);
            }
        }
    }

    public GameResult GetGameResult()
    {
        return GameResult.Draw;
    }

    public void OnPLayerPositionUpdated(PlayerController player)
    {
        HandleSameTileOccupancy();
    }

    public void RegisterPlayer(PlayerController player)
    {
        playersInMap.Add(player);
    }

    private Dictionary<Vector2Int, List<PlayerController>> GetTileOccupancy()
    {
        return playersInMap
            .GroupBy(x => WorldToMap(x.transform.position), y => y)
            .ToDictionary(x => x.Key, y => y.ToList());
    }

    private void HandleSameTileOccupancy()
    {
        var currentBoardOccupancy = GetTileOccupancy();
        AddClouds(currentBoardOccupancy);
        CleanOldClouds(currentBoardOccupancy);
    }

    private void CleanOldClouds(Dictionary<Vector2Int, List<PlayerController>> currentBoardOccupancy)
    {
        List<Vector2Int> toDelete = new List<Vector2Int>();
        foreach (var hustle in spawnedHustles)
        {
            if (currentBoardOccupancy.ContainsKey(hustle.Key) == false || currentBoardOccupancy[hustle.Key].Count < 1)
            {
                hustle.Value.SetActive(false);
                toDelete.Add(hustle.Key);
            }
        }

        foreach (var orphanHustle in toDelete)
        {
            spawnedHustles.Remove(orphanHustle);
        }
    }

    private void AddClouds(Dictionary<Vector2Int, List<PlayerController>> currentBoardOccupancy)
    {
        foreach (var occupiedTiles in currentBoardOccupancy)
        {
            if (occupiedTiles.Value.Count > 1)
            {
                var hustleCloud = GameObject.Instantiate(HustleCloud);
                spawnedHustles.Add(occupiedTiles.Key, hustleCloud);
                hustleCloud.transform.position = MapToWorld(occupiedTiles.Key.x, occupiedTiles.Key.y);
                Debug.Log($"Cloud at {occupiedTiles.Key}");
            }
        }
    }

    public Vector2Int WorldToMap(Vector2 worldPos)
    {
        var boardOrigin = GetBoardOrigin();

        float boardWith = this.width * TileSize;
        float boardHeight = this.height * TileSize;

        float column = (worldPos.x - boardOrigin.x) / (boardWith - boardOrigin.x) * boardWith - 1;
        float row = (worldPos.y - boardOrigin.y) / (boardHeight - boardOrigin.y) * boardHeight - 1;

        return new Vector2Int(Mathf.FloorToInt(row), Mathf.FloorToInt(column));
    }

    public Vector2 MapToWorld(int row, int column)
    {
        var boardOrigin = GetBoardOrigin();
        Vector2Int cellPosition = new Vector2Int(column + 1, row + 1);
        return boardOrigin + ((Vector2)cellPosition) * TileSize;
    }

    private Vector2 GetBoardOrigin()
    {
        Vector2 boardOrigin = dancefloor.localToWorldMatrix * dancefloor.localPosition;
        boardOrigin -= Vector2.one * TileSize;
        return boardOrigin;
    }
}