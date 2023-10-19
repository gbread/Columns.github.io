using Assets.Scripts;
using Assets.Scripts.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerPiece : BasePiece
{
    [SerializeField]
    Tile[] allTiles;
    const int TILES_COUNT = 3;
    int rotation = 0;
    [SerializeField]
    readonly float downPressedStepDelay = 0.05f;
    protected override float StepDelay => Input.GetKey(KeyCode.DownArrow) ? downPressedStepDelay : base.StepDelay;

    IEnumerable<Vector3Int> CellPositions
    {
        get
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                yield return Position + i * Vector3Int.up;
            }
        }
    }

    Tile RotatedTile(int index)
    {
        return tiles[(index + rotation) % tiles.Length];
    }

    protected override IEnumerable<TilePosition> TilePositionsRelativeTo(Vector3Int position)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            yield return new TilePosition(RotatedTile(i), position + i * Vector3Int.up);
        }
    }

    public void InitializeRandomTiles(Board board, Vector2Int position, IPieceCantFallDelegate pieceCantFallDelegate)
    {
        Initialize(board, position, pieceCantFallDelegate);
        tiles = new Tile[TILES_COUNT];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = allTiles[Random.Range(0, allTiles.Length)];
        }
    }

    override protected void CustomUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMoveIfValid(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMoveIfValid(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(1);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Drop();
        }
    }



    void Rotate(int count)
    {
        rotation = (rotation + count) % tiles.Length;
    }
}
