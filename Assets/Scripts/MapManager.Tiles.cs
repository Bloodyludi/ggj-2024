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
        var deadlyTiles = tiles.Where(x => x.isDeadly).Select(t => new Vector2Int(t.position.y, t.position.x)).ToArray();

        for (var i = 0; i < playersInMap.Count; i++)
        {
            var player = playersInMap[i];
            var playerPos = WorldToMap(player.transform.position);
            if (deadlyTiles.Contains(playerPos))
            {
                player.Kill();
                playersInMap.Remove(player);
            }
        }
    }

    private void SpawnNewTiles()
    {
        var prev = beatManager.BeatCounter * beatManager.SecondsPerBeat;
        var next = (beatManager.BeatCounter + 1) * beatManager.SecondsPerBeat;
        var tilesToSpawn = deadlyTileSpawns.Where(
            v => prev <= v.spawnTimeSeconds && next > v.spawnTimeSeconds
        );

        foreach (var sp in tilesToSpawn)
        {
            var tile = tiles[GetTileIndex(sp.spawnPosition)];
            tile.SetDeadly(true);
            tile.movementDirection = sp.movementDirection;
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