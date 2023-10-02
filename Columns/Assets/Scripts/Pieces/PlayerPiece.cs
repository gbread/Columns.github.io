using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerPiece : BasePiece
{
    [SerializeField]
    Tile[] allTiles;


    const int TILES_COUNT = 3;
    int rotation=0;

    float downPressedStepDelay = 0.05f;

    protected override float StepDelay => Input.GetKey(KeyCode.DownArrow) ? downPressedStepDelay : base.StepDelay;

    IEnumerable<Vector3Int> CellPositions {
        get
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                yield return position + i * Vector3Int.up;
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

    public void InitializeRandomTiles(BoardMonoBehaviour board, Vector2Int position)
    {
        this.board = board;
        this.position = (Vector3Int)position;
        tiles = new Tile[TILES_COUNT];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = allTiles[Random.Range(0, allTiles.Length)];
        }
    }


    // Update is called once per frame
    public override void Update()
    {
        board.Clear(this);

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

        Step();

        board.Set(this);
    }

    void Drop()
    {
        while (TryMoveIfValid(Vector2Int.down)) { }
        board.ActivePieceCantMoveDown(this);
    }


    void Rotate(int count)
    {
        rotation = (rotation + count) % tiles.Length;
    }
}