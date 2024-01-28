using System;
using System.Collections.Generic;
using System.Linq;
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
    public void UpdateDeadlyTiles()
    {
        List<DancefloorTile> updated = new List<DancefloorTile>();
        MoveDeadlyTiles(updated);
        ResolvePlayerDeaths();
        SpawnNewTiles();
    }

    private void MoveDeadlyTiles(List<DancefloorTile> updated)
    {
        foreach (var tile in tiles)
        {
            if (updated.Contains(tile) || !tile.isDeadly) continue;

            var newDeadlyTilePos = tile.position + tile.movementDirection;
            if (tile.position != newDeadlyTilePos)
            {
                tile.SetDeadly(false);
                var newTile = tiles[GetTileIndex(newDeadlyTilePos)];
                newTile.SetDeadly(true);
                newTile.movementDirection = tile.movementDirection;
                updated.Add(newTile);
            }
        }
    }

    private void ResolvePlayerDeaths()
    {
        var deadlyTiles = tiles.Where(x => x.isDeadly).Select(x => x.position);

        Dictionary<Vector2Int, List<PlayerController>> occupancy = GetTileOccupancy();
        foreach (var tile in deadlyTiles)
        {
            KillPlayersAtTile(occupancy, tile);
        }
    }

    private void SpawnNewTiles()
    {
        var prev = beatManager.BeatCounter * beatManager.SecondsPerBeat;
        var next = (beatManager.BeatCounter + 1) * beatManager.SecondsPerBeat;
        var tilesToSpawn = deadlyTileSpawns
            .Where(
            v => prev <= v.spawnTimeSeconds && next > v.spawnTimeSeconds
        );

        foreach (var sp in tilesToSpawn)
        {
            var tile = tiles[GetTileIndex(sp.spawnPosition)];
            tile.SetDeadly(true);
            tile.movementDirection = sp.movementDirection;
        }
    }

    private void KillPlayersAtTile(Dictionary<Vector2Int, List<PlayerController>> occupancy, Vector2Int tile)
    {
        occupancy.TryGetValue(new Vector2Int(tile.y, tile.x), out var players);
        if (players != null && players.Count > 0)
        {
            foreach (var player in players)
            {
                player.Kill();
                playersInMap.Remove(player);
            }

            players.Clear();
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