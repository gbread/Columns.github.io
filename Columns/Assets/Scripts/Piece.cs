using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    [SerializeField]
    Tile[] allTiles;

    BoardMonoBehaviour board;
    const int TILES_COUNT = 3;
    Tile[] tiles;
    int rotation=0;
    Vector3Int position;
    float stepDelay = 0.5f;
    float downPressedStepDelay = 0.05f;
    float stepTime = 0;
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
    public IEnumerable<TilePosition> TilePositions
    {
        get
        {
            return TilePositionsRelativeTo(this.position);
        }
    }

    private IEnumerable<TilePosition> TilePositionsRelativeTo(Vector3Int position)
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
    void Update()
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
        board.ActivePieceCantMoveDown();
    }

    void Step()
    {
        stepTime += Time.deltaTime;        
        if (stepTime < stepDelay && !Input.GetKey(KeyCode.DownArrow))
            return;
        if (Input.GetKey(KeyCode.DownArrow) && stepTime < downPressedStepDelay)
            return;

        stepTime = 0;
        bool didMoveDown = TryMoveIfValid(Vector2Int.down);
        if (!didMoveDown)
        {
            board.ActivePieceCantMoveDown();
        }
    }

    public bool TryMoveIfValid(Vector2Int translation)
    {
        Vector3Int newPostion = position + (Vector3Int)translation;
        if (TilePositionsRelativeTo(newPostion).Any(x => !board.IsValidPosition(x.position))) 
            return false;
        
        position = newPostion;
        return true;
    }

    void Rotate(int count)
    {
        rotation = (rotation + count) % tiles.Length;
    }
}
