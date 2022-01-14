/*
 * Primary game-controller component for a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
 * 
 * This class describes the game board and the play area and provides an interface for external objects
 * to discern and mutate the nature and contents of its composite tiles.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LongRoadGames.PacMan
{
    public class GameMaster : MonoBehaviour
    {
        private const int _BOARD_WIDTH = 28;
        private const int _BOARD_HEIGHT = 31;
        private static Vector3Int _PLAYER_START = new Vector3Int(13, 7, 0);

        public Tilemap Gameboard;
        public Player PacMan;

        private Dictionary<Vector3Int, GameTile> _playArea;

        public Tile DotTile { get; private set; }
        public Tile PDotTile { get; private set; }
        public Tile EmptyTile { get; private set; }

        public void Start()
        {
            Debug.Assert(Gameboard != null, "CRITICAL ERROR: The gameboard cannot be null.");
            Debug.Assert(PacMan != null, "CRITICAL ERROR: PacMan cannot be null.");

            DotTile = Instantiate(Resources.Load<Tile>("Sprites/Dot"));
            PDotTile = Instantiate(Resources.Load<Tile>("Sprites/PDot"));
            EmptyTile = Instantiate(Resources.Load<Tile>("Sprites/Empty"));

            _initialize_board();
            _reset_board();

            PacMan.Initialize(this);
            PacMan.Warp(GetTile(_PLAYER_START), Direction.Right);
        }

        #region Board Initialization and Resets

        private void _initialize_board()
        {
            _playArea = new Dictionary<Vector3Int, GameTile>();

            for(int y = 0; y < _BOARD_HEIGHT; y++)
            {
                for(int x = 0; x < _BOARD_WIDTH; x++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    Tile tile = Gameboard.GetTile<Tile>(pos);

                    if (tile == null)
                    {
                        Gameboard.SetTile(pos, EmptyTile);
                    }
                    else if (tile.name.Contains("wall"))
                    {
                        _playArea.Add(pos, new GameTile(this, pos, TileState.Wall, tile));
                    }
                    else
                    {
                        TileState initialState = (TileState)Enum.Parse(typeof(TileState), tile.name);
                        GameTile gameTile = new GameTile(this, pos, initialState, tile);
                        _playArea.Add(pos, gameTile);
                    }
                }
            }
        }

        private void _reset_board()
        {
            foreach(GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
                gameTile.Reset();
        }

        #endregion

        #region Tile Access and Mutation

        private Tile _stateMap(TileState state) => state switch
        {
            TileState.Empty => EmptyTile,
            TileState.Dot => DotTile,
            TileState.PDot => PDotTile,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No tile was found for the given state: {state}."),
        };

        public void SetTile(Vector3Int position, TileState state)
        {
            Gameboard.SetTile(position, _stateMap(state));
        }

        public GameTile GetTile(Vector3Int point)
        {
            return _playArea[point];
        }

        public GameTile GetTile(Vector3 point)
        {
            Vector3Int cellPos = Gameboard.WorldToCell(point);
            return _playArea[cellPos];
        }

        public bool DirectionBlocked(Vector3 position, Direction direction)
        {
            Vector3Int cellPos = Gameboard.WorldToCell(position);

            switch(direction)
            {
                case Direction.Up:
                    cellPos.y += 1;
                    break;

                case Direction.Down:
                    cellPos.y -= 1;
                    break;

                case Direction.Right:
                    cellPos.x += 1;
                    break;

                case Direction.Left:
                    cellPos.x -= 1;
                    break;
            }

            GameTile gameTile = GetTile(cellPos);

            return gameTile.CurrentState == TileState.Wall;
        }

        #endregion
    }
}
