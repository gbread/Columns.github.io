using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BasePiece : MonoBehaviour
{
    protected BoardMonoBehaviour board;
    protected Tile[] tiles;
    protected Vector3Int position;
    protected float stepDelay = 0.5f;
    protected float stepTime = 0;

    protected virtual float StepDelay
    {
        get
        {
            return stepDelay;
        }
    }
    IEnumerable<Vector3Int> CellPositions
    {
        get
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                yield return position + i * Vector3Int.up;
            }
        }
    }

    public IEnumerable<TilePosition> TilePositions
    {
        get
        {
            return TilePositionsRelativeTo(this.position);
        }
    }

    protected virtual IEnumerable<TilePosition> TilePositionsRelativeTo(Vector3Int position)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            yield return new TilePosition(tiles[i], position + i * Vector3Int.up);
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        board.Clear(this);

        Step();

        board.Set(this);
    }

    protected void Step()
    {
        stepTime += Time.deltaTime;
        if (stepTime < StepDelay)
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
}
