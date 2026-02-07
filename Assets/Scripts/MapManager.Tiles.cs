using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TileSpawnConfig
{
    [SerializeField, Range(1, 60)] public float spawnTimeSeconds;
    public Vector2Int spawnPosition;
    public Vector2Int movementDirection;
}

public partial class MapManager : MonoBehaviour
{
    // Reusable caches for tile operations
    private readonly HashSet<DancefloorTile> updatedTilesCache = new();
    private readonly HashSet<Vector2Int> deadlyPositionsCache = new();

    public void UpdateDeadlyTiles()
    {
        MoveDeadlyTiles();
        ResolvePlayerDeaths();
        SpawnNewTiles();
    }


    private void MoveDeadlyTiles()
    {
        updatedTilesCache.Clear();

        foreach (var tile in tiles)
        {
            if (updatedTilesCache.Contains(tile) || !tile.isDeadly) continue;

            var newDeadlyTilePos = FindFreePosition(tile.position + tile.movementDirection, tile.movementDirection);
            if (tile.position != newDeadlyTilePos)
            {
                tile.SetDeadly(false);
                var newTile = tiles[GetTileIndex(newDeadlyTilePos)];
                newTile.SetDeadly(true);
                newTile.movementDirection = tile.movementDirection;
                updatedTilesCache.Add(newTile);
            }
        }
    }

    private Vector2Int FindFreePosition(Vector2Int position, Vector2Int movementDirection)
    {
        var targetPositon = position;
        while (true)
        {
            var newTile = tiles[GetTileIndex(targetPositon)];
            if (newTile.isDeadly)
            {
                targetPositon = newTile.position + movementDirection;
            }
            else
            {
                return targetPositon;
            }
        }
    }

    private void ResolvePlayerDeaths()
    {
        // Build deadly positions set without LINQ
        deadlyPositionsCache.Clear();
        foreach (var tile in tiles)
        {
            if (tile.isDeadly)
            {
                deadlyPositionsCache.Add(new Vector2Int(tile.position.y, tile.position.x));
            }
        }

        // Iterate backwards to safely remove while iterating
        for (var i = playersInMap.Count - 1; i >= 0; i--)
        {
            var player = playersInMap[i];
            var playerPos = WorldToMap(player.transform.position);
            if (deadlyPositionsCache.Contains(playerPos))
            {
                player.Kill();
                playersInMap.RemoveAt(i);
            }
        }
    }

    private void SpawnNewTiles()
    {
        var prev = beatManager.BeatCounter * beatManager.BeatInterval;
        var next = (beatManager.BeatCounter + 1) * beatManager.BeatInterval;

        for (int i = 0; i < deadlyTileSpawns.Length; i++)
        {
            var spawn = deadlyTileSpawns[i];
            if (prev <= spawn.spawnTimeSeconds && next > spawn.spawnTimeSeconds)
            {
                var targetPos = FindFreePosition(spawn.spawnPosition, spawn.movementDirection);
                var tile = tiles[GetTileIndex(targetPos)];
                tile.SetDeadly(true);
                tile.movementDirection = spawn.movementDirection;
            }
        }
    }

    public int GetTileIndex(Vector2Int position)
    {
        var clamped = new Vector2Int(MathMod(position.x, width), MathMod(position.y, height));
        return clamped.y * width + clamped.x;
    }

    static int MathMod(int a, int b)
    {
        return (Mathf.Abs(a * b) + a) % b;
    }
}
