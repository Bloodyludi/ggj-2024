using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct TileSpawnConfig
{
    [SerializeField, Range(1, 60)] public float spawnTimeSeconds;
    public Vector2Int spawnPosition;
    public Vector2Int movementDirection;
}

public partial class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private GameObject HustleCloud;
    public Transform dancefloor;
    public Transform decorations;
    private BeatManager beatManager;
    [SerializeField] private DancefloorTile dancefloorTilePrefab;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 8;
    
    [SerializeField] private TileSpawnConfig[] deadlyTileSpawns;

    // --- NEW: Added for GameController compatibility ---
    public void SetDeadlyTileSpawns(TileSpawnConfig[] spawns)
    {
        deadlyTileSpawns = spawns;
    }

    [Header("Pickup Settings")] 
    [SerializeField] private PickupItem pickupPrefab;
    [SerializeField] private float pickupSpawnInterval = 5.0f;
    private float pickupTimer;
    private PickupItem activePickup;

    private Dictionary<Vector2Int, GameObject> spawnedHustles = new Dictionary<Vector2Int, GameObject>();
    public float TileSize => dancefloorTilePrefab.TileSize;
    public Vector2 Dimensions => new Vector2(width, height);

    [NonSerialized] public DancefloorTile[] tiles;
    private List<PlayerController> playersInMap = new List<PlayerController>();

    private List<Vector2> stunDirections { get; } = new List<Vector2>()
    {
        new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0),
    };

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
        
        pickupTimer = pickupSpawnInterval;
    }

    private void Update()
    {
        if (activePickup == null)
        {
            pickupTimer -= Time.deltaTime;
            if (pickupTimer <= 0)
            {
                SpawnPickup();
                pickupTimer = pickupSpawnInterval;
            }
        }
    }

    private void SpawnPickup()
    {
        if (pickupPrefab == null) return;

        for (int i = 0; i < 20; i++) 
        {
            int randX = Random.Range(0, width);
            int randY = Random.Range(0, height);
            Vector2Int candidatePos = new Vector2Int(randX, randY);
            var tileIndex = GetTileIndex(candidatePos);
            
            if (tiles[tileIndex].isDeadly) continue;

            bool isPlayerOnTile = false;
            foreach (var player in playersInMap)
            {
                Vector2Int pGridPos = WorldToMap(player.transform.position);
                if (pGridPos.x == candidatePos.y && pGridPos.y == candidatePos.x)
                {
                    isPlayerOnTile = true;
                    break;
                }
            }

            if (!isPlayerOnTile)
            {
                activePickup = Instantiate(pickupPrefab, dancefloor);
                activePickup.transform.localPosition = new Vector3(randX, randY) * TileSize;
                activePickup.GridPosition = candidatePos;
                break;
            }
        }
    }

    private void OnEnable()
    {
        if (beatManager == null) beatManager = Services.Get<BeatManager>();
        beatManager.OnPostBeat += ResolveBoardCollisions;
        beatManager.OnPostBeat += UpdateDeadlyTiles;
    }

    private void OnDisable()
    {
        if (beatManager != null)
        {
            beatManager.OnPostBeat -= ResolveBoardCollisions;
            beatManager.OnPostBeat -= UpdateDeadlyTiles;
        }
    }

    public void UpdateDeadlyTiles()
    {
        MoveDeadlyTiles();
        ResolvePlayerDeaths();
        SpawnNewTiles();
    }

    private void MoveDeadlyTiles()
    {
        var updated = new List<DancefloorTile>();
        foreach (var tile in tiles)
        {
            if (updated.Contains(tile) || !tile.isDeadly) continue;
            var newPos = FindFreePosition(tile.position + tile.movementDirection, tile.movementDirection);
            if (tile.position != newPos)
            {
                tile.SetDeadly(false);
                var newTile = tiles[GetTileIndex(newPos)];
                newTile.SetDeadly(true);
                newTile.movementDirection = tile.movementDirection;
                updated.Add(newTile);
            }
        }
    }

    private void SpawnNewTiles()
    {
        var prev = beatManager.BeatCounter * beatManager.BeatInterval;
        var next = (beatManager.BeatCounter + 1) * beatManager.BeatInterval;
        if (deadlyTileSpawns == null) return;

        foreach (var sp in deadlyTileSpawns)
        {
            if (prev <= sp.spawnTimeSeconds && next > sp.spawnTimeSeconds)
            {
                var targetPos = FindFreePosition(sp.spawnPosition, sp.movementDirection);
                var tile = tiles[GetTileIndex(targetPos)];
                tile.SetDeadly(true);
                tile.movementDirection = sp.movementDirection;
            }
        }
    }

    private Vector2Int FindFreePosition(Vector2Int position, Vector2Int movementDirection)
    {
        var targetPositon = position;
        int safetyCount = 0; 
        while (safetyCount < width * height)
        {
            targetPositon = new Vector2Int(MathMod(targetPositon.x, width), MathMod(targetPositon.y, height));
            var newTile = tiles[GetTileIndex(targetPositon)];
            if (newTile.isDeadly) targetPositon = newTile.position + movementDirection;
            else return targetPositon;
            safetyCount++;
        }
        return position;
    }

    private void ResolvePlayerDeaths()
    {
        var deadlyCoords = new List<Vector2Int>();
        foreach(var t in tiles) if(t.isDeadly) deadlyCoords.Add(new Vector2Int(t.position.y, t.position.x));

        for (var i = 0; i < playersInMap.Count; i++)
        {
            var player = playersInMap[i];
            if (deadlyCoords.Contains(WorldToMap(player.transform.position)))
            {
                player.Kill();
                playersInMap.RemoveAt(i--);
            }
        }
    }

    private void ResolveBoardCollisions()
    {
        var occupancy = GetTileOccupancy();
        foreach (var cell in occupancy)
        {
            if (cell.Value.Count <= 1) continue;
            shuffledDirections.Clear();
            shuffledDirections.AddRange(stunDirections);
            for (int i = shuffledDirections.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffledDirections[i], shuffledDirections[j]) = (shuffledDirections[j], shuffledDirections[i]);
            }
            int dirIndex = 0;
            foreach (var p in cell.Value) if (dirIndex < shuffledDirections.Count) p.StunPlayer(shuffledDirections[dirIndex++]);
        }
    }

    public void OnPlayerPositionUpdated(PlayerController player)
    {
        ResolvePlayerDeaths();
        if (activePickup != null)
        {
            Vector2Int pPos = WorldToMap(player.transform.position);
            if (pPos.x == activePickup.GridPosition.y && pPos.y == activePickup.GridPosition.x)
            {
                activePickup.OnCollected(player);
                activePickup = null;
            }
        }
        HandleSameTileOccupancy(player);
    }

    public void RegisterPlayer(PlayerController player) => playersInMap.Add(player);

    private Dictionary<Vector2Int, List<PlayerController>> GetTileOccupancy()
    {
        foreach (var kvp in occupancyCache) kvp.Value.Clear();
        foreach (var p in playersInMap)
        {
            var pos = WorldToMap(p.transform.position);
            if (!occupancyCache.TryGetValue(pos, out var list)) occupancyCache[pos] = list = new List<PlayerController>(2);
            list.Add(p);
        }
        return occupancyCache;
    }

    private void HandleSameTileOccupancy(PlayerController player)
    {
        var occ = GetTileOccupancy();
        TryAddClouds(occ, player);
        CleanOldClouds(occ);
    }

    private void CleanOldClouds(Dictionary<Vector2Int, List<PlayerController>> occ)
    {
        List<Vector2Int> toDel = new();
        foreach (var h in spawnedHustles) if (!occ.ContainsKey(h.Key) || occ[h.Key].Count < 1) { h.Value.SetActive(false); toDel.Add(h.Key); }
        foreach (var k in toDel) spawnedHustles.Remove(k);
    }

    private void TryAddClouds(Dictionary<Vector2Int, List<PlayerController>> occ, PlayerController c)
    {
        foreach (var t in occ)
        {
            if (t.Value.Count > 1 && !spawnedHustles.ContainsKey(t.Key))
            {
                var cloud = Instantiate(HustleCloud, decorations);
                spawnedHustles.Add(t.Key, cloud);
                cloud.transform.position = MapToWorld(t.Key.x, t.Key.y) + TileSize * Vector2.one * 0.5f;
                foreach (var p in t.Value) p.SetPlayerFighting(cloud.transform.position);
            }
        }
    }

    public Vector2Int WorldToMap(Vector2 worldPos)
    {
        var origin = GetBoardOrigin() + Vector2.one * TileSize * 0.5f;
        float bW = width * TileSize;
        float bH = height * TileSize;
        float col = (worldPos.x - origin.x) / (bW - origin.x) * bW;
        float row = (worldPos.y - origin.y) / (bH - origin.y) * bH;
        // FIXED: Changed 'column' to 'col' to match local variable
        return new Vector2Int(Mathf.FloorToInt(row), Mathf.FloorToInt(col));
    }

    public Vector2 MapToWorld(float r, float c) => GetBoardOrigin() + new Vector2(c, r) * TileSize;

    private Vector2 GetBoardOrigin() => (Vector2)(dancefloor.localToWorldMatrix * dancefloor.localPosition) - Vector2.one * TileSize * 0.5f;
    
    public int GetTileIndex(Vector2Int p) => MathMod(p.y, height) * width + MathMod(p.x, width);

    static int MathMod(int a, int b) => (Mathf.Abs(a * b) + a) % b;

    public Vector3 GetLoopPosition(Vector2 pos)
    {
        var origin = GetBoardOrigin();
        var size = new Vector2(width * TileSize, height * TileSize);
        Vector2 diff = pos - origin;
        Vector2 uv = new Vector2(diff.x / size.x, diff.y / size.y);
        if (uv.x > 1) uv.x -= 1; else if (uv.x < 0) uv.x += 1;
        if (uv.y > 1) uv.y -= 1; else if (uv.y < 0) uv.y += 1;
        return (Vector3)(origin + uv * size);
    }
}