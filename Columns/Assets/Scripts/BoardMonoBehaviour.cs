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
    Tilemap tilemap;
    [SerializeField]
    private Vector2Int spawnPosition;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(10, 20);
    [SerializeField]
    private GameObject playerPiecePrefab;
    [SerializeField]
    private GameObject basePiecePrefab;
    private HashSet<TilePosition> freshPositions = new HashSet<TilePosition>();

    private List<BasePiece> activePieces = new List<BasePiece>();

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

    // Start is called before the first frame update
    void Start()
    {
        SpawnPiece();   
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameOver();
        }

    }

    public void SpawnPiece()
    {
        var activePiece = Instantiate(playerPiecePrefab, transform).GetComponent<PlayerPiece>();
        activePiece.InitializeRandomTiles(this, spawnPosition);
        activePieces.Add(activePiece);

        if (!activePiece.TryMoveIfValid(Vector2Int.zero))
        {
            GameOver();
        }

        

        Set(activePiece);
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

    private void DeletePiece(BasePiece piece)
    {
        piece.enabled = false;
        activePieces.Remove(piece);
        Destroy(piece.gameObject);
    }


    public void GameOver()
    {
        tilemap.ClearAllTiles();
        while (activePieces.Count() > 0)
        {
            DeletePiece(activePieces.Last());
        }

        SpawnPiece();
    }

    public void ActivePieceCantMoveDown(BasePiece piece)
    {
        Set(piece);
        freshPositions.UnionWith(piece.TilePositions);
        DeletePiece(piece);
        if (activePieces.Count() == 0)
        {
            Explode();
            SpawnPiece();
        }
    }

    private void Explode()
    {
        var explodingTiles = GetExplodingTiles();
        foreach (var explodingTile in explodingTiles)
        {
            tilemap.SetTile(explodingTile, null);
        }
        freshPositions.Clear();
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

        foreach (var tilePosition in freshPositions)
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

    private IEnumerable<BasePiece> GetPiecesAboveExplodedPieces(IEnumerable<Vector3Int> explosionPositions)
    {
        return explosionPositions
            .Select(explodedPosition => new { position = explodedPosition, tiles = SliceTiles(Vector3Int.down, explodedPosition)})
            .Where(x => x.tiles.Count() > 0)
            .Select(x => BasePiece.CreatePiece(this, (Vector2Int)x.position, x.tiles));
    }
}
