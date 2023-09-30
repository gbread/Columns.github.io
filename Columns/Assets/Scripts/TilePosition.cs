using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public struct TilePosition
    {
        public Tile tile;
        public Vector3Int position;
       
        public TilePosition(Tile tile, Vector3Int position)
        {
            this.tile = tile;
            this.position = (Vector3Int)position;
        }
    }
}
