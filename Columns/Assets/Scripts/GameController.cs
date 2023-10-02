using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class GameController : MonoBehaviour, IPieceCantFallDelegate
    {
        private Board board;

        private readonly HashSet<TilePosition> freshPositions = new();
        private readonly List<BasePiece> activePieces = new();

        [SerializeField]
        private GameObject playerPiecePrefab;
        [SerializeField]
        private GameObject basePiecePrefab;

        private void Awake()
        {
            board = GetComponent<Board>();
        }

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

        private void DeletePiece(BasePiece piece)
        {
            piece.enabled = false;
            activePieces.Remove(piece);
            Destroy(piece.gameObject);
        }
        public void GameOver()
        {
            board.Clear();
            while (activePieces.Count() > 0)
            {
                DeletePiece(activePieces.Last());
            }

            SpawnPiece();
        }

        private void Explode()
        {
            var explodingTiles = board.GetExplodingTiles(freshPositions);
            board.Clear(explodingTiles);
            freshPositions.Clear();

            var newPieces = GetPiecesAboveExplodedPieces(explodingTiles);
            activePieces.AddRange(newPieces);
        }

        public void SpawnPiece()
        {
            var activePiece = Instantiate(playerPiecePrefab, transform).GetComponent<PlayerPiece>();
            activePiece.InitializeRandomTiles(board, board.spawnPosition, this);
            activePieces.Add(activePiece);

            if (!activePiece.TryMoveIfValid(Vector2Int.zero))
            {
                GameOver();
            }

            board.Set(activePiece);
        }
        private IEnumerable<BasePiece> GetPiecesAboveExplodedPieces(IEnumerable<Vector3Int> explosionPositions)
        {
            return explosionPositions
                .Select(explodedPosition => new { position = explodedPosition + Vector3Int.up, tiles = board.SliceTiles(Vector3Int.down, explodedPosition) })
                .Where(x => x.tiles.Count() > 0)
                .Select(x => BasePiece.CreatePiece(board, (Vector2Int)x.position, x.tiles.Reverse(), basePiecePrefab, this));
        }

        public void PieceCantFallCallback(BasePiece piece)
        {
            board.Set(piece);
            freshPositions.UnionWith(piece.TilePositions);
            DeletePiece(piece);
            if (activePieces.Count() == 0)
            {
                Explode();
            }
            if (activePieces.Count() == 0)
            {
                SpawnPiece();
            }
        }
    }
}
