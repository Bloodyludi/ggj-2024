using System;
using System.Collections.Generic;
using UnityEngine;

public partial class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject HustleCloud;
    public Transform dancefloor;
    public Transform decorations;
    private BeatManager beatManager;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    private Dictionary<Vector2Int, GameObject> spawnedHustles = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;
    private TileSpawnConfig[] deadlyTileSpawns;

    public void SetDeadlyTileSpawns(TileSpawnConfig[] spawns)
    {
        deadlyTileSpawns = spawns;
    }

    public float TileSize => dancefloorTilePrefab.TileSize;
    public Vector2 Dimensions => new Vector2(width, height);

    [NonSerialized] public DancefloorTile[] tiles;

    private List<PlayerController> playersInMap = new List<PlayerController>();

    private List<Vector2> stunDirections { get; } =
        new List<Vector2>()
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0),
        };

    // Reusable caches to avoid per-beat GC allocations
    private readonly Dictionary<Vector2Int, List<PlayerController>> occupancyCache = new();
    private readonly List<Vector2> shuffledDirections = new(4);

    private void Awake()
    {
        Services.Register(this);
        beatManager = Services.Get<BeatManager>();
        tiles = new DancefloorTile[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = Instantiate(dancefloorTilePrefab, dancefloor);
                tile.Init(beatManager);
                tile.SetPosition(x, y);

                tiles[GetTileIndex(tile.position)] = tile;
            }
        }

        dancefloor.localPosition = -new Vector3(
            dancefloorTilePrefab.TileSize * (width - 1),
            dancefloorTilePrefab.TileSize * (height - 1)
        ) / 2;
    }

    private void OnEnable()
    {
        beatManager.OnPostBeat += ResolveBoardCollisions;
        beatManager.OnPostBeat += UpdateDeadlyTiles;
    }

    private void OnDisable()
    {
        beatManager.OnPostBeat -= ResolveBoardCollisions;
        beatManager.OnPostBeat -= UpdateDeadlyTiles;
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

            // Fisher-Yates shuffle on reusable list
            shuffledDirections.Clear();
            shuffledDirections.AddRange(stunDirections);
            for (int i = shuffledDirections.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffledDirections[i], shuffledDirections[j]) = (shuffledDirections[j], shuffledDirections[i]);
            }

            int dirIndex = 0;
            foreach (var player in occupiedCell.Value)
            {
                player.StunPlayer(shuffledDirections[dirIndex]);
                dirIndex++;
            }
        }
    }

    public void OnPlayerPositionUpdated(PlayerController player)
    {
        ResolvePlayerDeaths();
        HandleSameTileOccupancy(player);
    }

    public void RegisterPlayer(PlayerController player)
    {
        playersInMap.Add(player);
    }

    private Dictionary<Vector2Int, List<PlayerController>> GetTileOccupancy()
    {
        // Clear and reuse cached dictionary
        foreach (var kvp in occupancyCache)
        {
            kvp.Value.Clear();
        }

        foreach (var player in playersInMap)
        {
            var pos = WorldToMap(player.transform.position);
            if (!occupancyCache.TryGetValue(pos, out var list))
            {
                list = new List<PlayerController>(2);
                occupancyCache[pos] = list;
            }
            list.Add(player);
        }

        return occupancyCache;
    }

    private void HandleSameTileOccupancy(PlayerController playerController)
    {
        var currentBoardOccupancy = GetTileOccupancy();
        TryAddClouds(currentBoardOccupancy, playerController);
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

    private void TryAddClouds(Dictionary<Vector2Int, List<PlayerController>> currentBoardOccupancy, PlayerController controller)
    {
        foreach (var occupiedTiles in currentBoardOccupancy)
        {
            if (occupiedTiles.Value.Count > 1 && spawnedHustles.ContainsKey(occupiedTiles.Key) == false)
            {
                var hustleCloud = GameObject.Instantiate(HustleCloud, decorations.transform);
                spawnedHustles.Add(occupiedTiles.Key, hustleCloud);
                hustleCloud.transform.position = MapToWorld(occupiedTiles.Key.x, occupiedTiles.Key.y) + TileSize * Vector2.one * 0.5f;
                foreach (var playerController in occupiedTiles.Value)
                {
                    playerController.SetPlayerFighting(hustleCloud.transform.position);
                }
            }
        }
    }

    public Vector2Int WorldToMap(Vector2 worldPos)
    {
        var boardOrigin = GetBoardOrigin();
        boardOrigin += Vector2.one * TileSize * 0.5f;

        float boardWith = this.width * TileSize;
        float boardHeight = this.height * TileSize;

        float column = (worldPos.x - boardOrigin.x) / (boardWith - boardOrigin.x) * boardWith;
        float row = (worldPos.y - boardOrigin.y) / (boardHeight - boardOrigin.y) * boardHeight;

        return new Vector2Int(Mathf.FloorToInt(row), Mathf.FloorToInt(column));
    }

    public Vector2 MapToWorld(int row, int column)
    {
        return MapToWorld((float)row, (float)column);
    }

    public Vector2 MapToWorld(float row, float column)
    {
        var boardOrigin = GetBoardOrigin();
        Vector2 cellPosition = new Vector2(column, row);
        return boardOrigin + ((Vector2)cellPosition) * TileSize;
    }

    private Vector2 GetBoardOrigin()
    {
        Vector2 boardOrigin = dancefloor.localToWorldMatrix * dancefloor.localPosition;
        boardOrigin -= Vector2.one * TileSize * 0.5f;
        return boardOrigin;
    }

    public Vector3 GetLoopPosition(Vector2 currentPosition)
    {
        var boardOrigin = GetBoardOrigin();

        float boardWith = this.width * TileSize;
        float boardHeight = this.height * TileSize;
        var boardSize = new Vector2(boardWith, boardHeight);
        var boardEnd = GetBoardOrigin() + boardSize;

        Vector2 uvCoords = (currentPosition - boardOrigin).DivideVector(boardEnd - boardOrigin);

        if (uvCoords.x > 1)
        {
            uvCoords.x -= 1;
        }
        else if (uvCoords.x < 0)
        {
            uvCoords.x += 1;
        }

        if (uvCoords.y > 1)
        {
            uvCoords.y -= 1;
        }
        else if (uvCoords.y < 0)
        {
            uvCoords.y += 1;
        }

        currentPosition = boardOrigin + uvCoords * boardSize;

        return currentPosition;
    }
}
