using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BoardMonoBehaviour : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;
    private Piece activePiece;
    [SerializeField]
    private Vector2Int spawnPosition;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(10, 20);
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
        activePiece = GetComponent<Piece>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnPiece();   
    }

    public void SpawnPiece()
    {
        activePiece.InitializeRandomTiles(this, spawnPosition);

        if (!activePiece.TryMoveIfValid(Vector2Int.zero))
        {
            GameOver();
        }

        Set(activePiece);
    }

    public void Set(Piece piece)
    {
        foreach (var tilePosition in piece.TilePositions)
        {
            tilemap.SetTile(tilePosition.position, tilePosition.tile);
        } 
    }
    public void Clear(Piece piece)
    {
        foreach (var tilePosition in piece.TilePositions)
        {
            tilemap.SetTile(tilePosition.position, null);
        }
    }

    /// <summary>
    /// Returns true for empty tile inside the board
    /// </summary>
    /// <param name="position"></param>
    public bool IsValidPosition(Vector3Int position)
    {
        if (!Bounds.Contains((Vector2Int)position)) return false;
        return IsEmptyOrOutside(position);
    }

    bool IsEmptyOrOutside(Vector3Int position)
    {
        if (tilemap.HasTile(position)) return false;
        return true;
    }


    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            tilemap.ClearAllTiles();
            SpawnPiece();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        SpawnPiece();
    }

    public void ActivePieceCantMoveDown()
    {
        Set(activePiece);

        Explode();

        SpawnPiece();
    }

    private void Explode()
    {
        foreach (var explodingTile in GetExplodingTiles())
        {
            tilemap.SetTile(explodingTile, null);
        }
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
    IEnumerable<TilePosition> SliceTiles(Vector3Int direction, Vector3Int position)
    {
        return SliceTilesOnlyPositions(direction, position).Select(x => new TilePosition(tilemap.GetTile<Tile>(x), x));
    }

    private IEnumerable<TilePosition> GetExplodingTilesFromRow(IEnumerable<TilePosition> tiles)
    {
        var tilesArray = tiles.ToArray();
        Func<TilePosition, TilePosition, bool> tilesAreEqual = (a, b) => a.tile.name == b.tile.name;
        var currentTiles = new HashSet<TilePosition>();
        var result = new List<TilePosition>();
        const int countInARowToExplode = 3;
        currentTiles.Add(tilesArray[0]);
        var lastTile = tilesArray[0];
        for (int i = 1; i < tilesArray.Length; i++)
        {
            if (tilesAreEqual(lastTile, tilesArray[i]))
            {
                currentTiles.Add(tilesArray[i]);
            } 
            else
            {
                if (currentTiles.Count >= countInARowToExplode)
                {
                    result.AddRange(currentTiles);
                }
                currentTiles.Clear();
                currentTiles.Add(tilesArray[i]);
            }
            lastTile = tilesArray[i];
        }
        if (currentTiles.Count >= countInARowToExplode)
        {
            result.AddRange(currentTiles);
        }
        return result;
    }

    private IEnumerable<Vector3Int> GetExplodingTiles()
    {
        var directions = new Vector2Int[] { Vector2Int.down, Vector2Int.right, Vector2Int.right + Vector2Int.down, Vector2Int.right - Vector2Int.down };
        var result = new HashSet<Vector3Int>();

        foreach (var tilePosition in activePiece.TilePositions)
        {
            var tile = tilePosition.tile;
            foreach (var direction in directions)
            {
                var slice = SliceTiles((Vector3Int)direction, tilePosition.position);
                var explodingTiles = GetExplodingTilesFromRow(slice);
                if (explodingTiles.Count() > 0)
                {
                    Console.WriteLine(tilePosition.position);
                }
                result.UnionWith(explodingTiles.Select(x => x.position));
            }
        }
        return result;
    } 
}
