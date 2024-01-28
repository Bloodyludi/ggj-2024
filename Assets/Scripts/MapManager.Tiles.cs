using System.Collections.Generic;
using UnityEngine;

public partial class MapManager : MonoBehaviour
{
    public void DeadlyTilePositions()
    {
        List<DancefloorTile> updated = new List<DancefloorTile>();
        foreach (var tile in tiles)
        {
            if (updated.Contains(tile) || !tile.isDeadly) continue;
            
            var newDeadlyTilePos = tile.position + tile.movementDirection;
            if (tile.position != newDeadlyTilePos)
            {
                tile.SetDeadly(false);
                var newTile = tiles[GetTileIndex(newDeadlyTilePos)];
                newTile.SetDeadly(true);
                updated.Add(newTile);

                GetTileOccupancy().TryGetValue(new Vector2Int(newTile.position.y, newTile.position.x), out var players);
                if (players != null && players.Count > 0)
                {
                    foreach (var player in players)
                    {
                        player.Kill();
                    }

                    players.Clear();
                }
            }
        }
    }

    public int GetTileIndex(Vector2Int position)
    {
        var clamped = new Vector2Int(position.x % width, position.y % height);
        return clamped.y * width + clamped.x;
    }
}