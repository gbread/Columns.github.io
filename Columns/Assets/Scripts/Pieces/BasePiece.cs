using Assets.Scripts;
using Assets.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BasePiece : MonoBehaviour
{
    public Board board;
    protected Tile[] tiles;
    public Vector3Int Position { get; protected set; }
    [SerializeField]
    protected float stepDelay = 0.5f;
    protected float stepTime = 0;
    protected IPieceCantFallDelegate pieceCantFallDelegate;


    protected void Initialize(Board board, Vector2Int position, IPieceCantFallDelegate pieceCantFallDelegate)
    {
        this.board = board;
        this.Position = (Vector3Int)position;
        this.pieceCantFallDelegate = pieceCantFallDelegate;
    }
    public static BasePiece CreatePiece(Board board, Vector2Int position, IEnumerable<TilePosition> tiles, GameObject prefab, IPieceCantFallDelegate pieceCantFallDelegate)
    {
        var newGameObject = Instantiate(prefab, board.transform);
        var result = newGameObject.GetComponent<BasePiece>();
        result.Initialize(board, position, pieceCantFallDelegate);
        result.tiles = tiles.Select(x => x.tile).ToArray();
        return result;
    }

    protected virtual float StepDelay
    {
        get
        {
            return stepDelay;
        }
    }

    public IEnumerable<TilePosition> TilePositions
    {
        get
        {
            return TilePositionsRelativeTo(this.Position);
        }
    }

    protected virtual IEnumerable<TilePosition> TilePositionsRelativeTo(Vector3Int position)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            yield return new TilePosition(tiles[i], position + i * Vector3Int.up);
        }
    }

    void Update()
    {
        board.Clear(this);

        CustomUpdate();

        Step();
        board.Set(this);
    }

    protected virtual void CustomUpdate() { }

    private void Step()
    {
        stepTime += Time.deltaTime;
        if (stepTime < StepDelay)
            return;

        stepTime = 0;
        bool didMoveDown = TryMoveIfValid(Vector2Int.down);
        if (!didMoveDown)
        {
            pieceCantFallDelegate?.PieceCantFallCallback(this);
        }
    }

    public void Drop()
    {
        while (TryMoveIfValid(Vector2Int.down)) { }
        pieceCantFallDelegate?.PieceCantFallCallback(this);
    }

    public bool TryMoveIfValid(Vector2Int translation)
    {
        Vector3Int newPostion = Position + (Vector3Int)translation;
        if (TilePositionsRelativeTo(newPostion).Any(x => !board.IsEmptyAndInsideTheBoard(x.position)))
            return false;

        Position = newPostion;
        return true;
    }
}
