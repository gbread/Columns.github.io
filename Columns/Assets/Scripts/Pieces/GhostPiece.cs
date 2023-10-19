using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostPiece : BasePiece
{
    [SerializeField]
    private Tile ghostTile;

    private BasePiece piece;

    public BasePiece PlayerPiece
    {
        get { return piece; }
        set {
            piece = value; 
            if (piece == null)
            {
                board.Clear(this);
                tiles = new Tile[0];
                return;
            }
            tiles = Enumerable.Repeat(ghostTile, 3).ToArray();
            board = piece.board;
        }
    }

    protected override void CustomUpdate()
    {
        base.CustomUpdate();
        
        if (piece == null) return;

        Position = piece.Position;
        board.Clear(piece);
        Drop();
        board.Set(piece);
    }

}
