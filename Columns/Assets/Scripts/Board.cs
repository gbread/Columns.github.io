using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Board : MonoBehaviour
{
    Tilemap tilemap;
    [SerializeField]
    public Vector2Int spawnPosition;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(10, 20);

    [SerializeField]
    private Tile ghostTile;

    private RectInt Bounds
    {
        get
        {
            Vector2Int position = -boardSize / 2;
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
    }

    public void Set(BasePiece piece)
    {
        if (!piece.enabled) return;

        foreach (var tilePosition in piece.TilePositions)
        {
            tilemap.SetTile(tilePosition.position, tilePosition.tile);
        }
    }

    public void Clear(BasePiece piece)
    {
        if (piece is GhostPiece)
        {
            Clear((GhostPiece)piece);
            return;
        }

        Clear(piece.TilePositions.Select(tilePosition => tilePosition.position));
    }
    public void Clear(GhostPiece piece)
    {
        Clear(
            piece.TilePositions
            .Select(tilePosition => tilePosition.position)
            .Where(position => IsEmptyOrOutside(position))
            );
    }
    public void Clear()
    {
        tilemap.ClearAllTiles();
    }
    public void Clear(IEnumerable<Vector3Int> positions)
    {
        foreach (var position in positions)
        {
            tilemap.SetTile(position, null);
        }
    }

    public bool IsEmptyAndInsideTheBoard(Vector3Int position)
    {
        if (!Bounds.Contains((Vector2Int)position)) return false;
        return IsEmptyOrOutside(position);
    }

    bool IsEmptyOrOutside(Vector3Int position)
    {
        if (!tilemap.HasTile(position)) return true;
        if (tilemap.GetTile<Tile>(position) == ghostTile) return true;
        return false;
    }

    IEnumerable<Vector3Int> SliceTilesOnlyPositions(Vector3Int direction, Vector3Int position)
    {
        while (!IsEmptyOrOutside(position - direction))
        {
            position -= direction;
        }
        while (!IsEmptyOrOutside(position))
        {
            yield return position;
            position += direction;
        }
    }
    public IEnumerable<TilePosition> SliceTiles(Vector3Int direction, Vector3Int position)
    {
        return SliceTilesOnlyPositions(direction, position).Select(x => new TilePosition(tilemap.GetTile<Tile>(x), x));
    }

    private IEnumerable<TilePosition> GetExplodingTilesFromRow(IEnumerable<TilePosition> tiles)
    {
        if (tiles.Count() == 0) return new TilePosition[0];
        Func<TilePosition, TilePosition, bool> tilesAreEqual = (a, b) => a.tile.name == b.tile.name;
        var currentTiles = new HashSet<TilePosition>();
        var result = new List<TilePosition>();
        const int countInARowToExplode = 3;
        currentTiles.Add(tiles.First());
        var lastTile = tiles.First();
        foreach (var tile in tiles.Skip(1))
        {
            if (tilesAreEqual(lastTile, tile))
            {
                currentTiles.Add(tile);
            }
            else
            {
                if (currentTiles.Count >= countInARowToExplode)
                {
                    result.AddRange(currentTiles);
                }
                currentTiles.Clear();
                currentTiles.Add(tile);
            }
            lastTile = tile;
        }
        if (currentTiles.Count >= countInARowToExplode)
        {
            result.AddRange(currentTiles);
        }
        return result;
    }

    public IEnumerable<Vector3Int> GetExplodingTiles(IEnumerable<TilePosition> freshPositions)
    {
        var directions = new Vector2Int[] { Vector2Int.down, Vector2Int.right, Vector2Int.right + Vector2Int.down, Vector2Int.right - Vector2Int.down };
        var result = new HashSet<Vector3Int>();

        foreach (var tilePosition in freshPositions)
        {
            var tile = tilePosition.tile;
            foreach (var direction in directions)
            {
                var slice = SliceTiles((Vector3Int)direction, tilePosition.position);
                if (slice.Count() == 0) continue;
                var explodingTiles = GetExplodingTilesFromRow(slice);
                result.UnionWith(explodingTiles.Select(x => x.position));
            }
        }
        return result;
    }


}
