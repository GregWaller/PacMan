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

#define _DEV
//#define _DEV_LEVELPROGRESSION

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LongRoadGames.PacMan
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right,
    };

    public class Gameboard : MonoBehaviour
    {
        private const int _BOARD_WIDTH = 28;
        private const int _BOARD_HEIGHT = 31;
        private static Vector3Int _PLAYER_START = new Vector3Int(14, 7, 0);

        public Tilemap Tilemap;
        public PacMan PacMan;

        public Tile DotTile { get; private set; }
        public Tile PDotTile { get; private set; }
        public Tile EmptyTile { get; private set; }
        public UIController GUI { get; private set; }

        public bool PowerPhase { get; private set; } = false;
        private float _powerPhaseDuration = 10.0f;
        private int _currentPowerPhase = 0;

        private Dictionary<Vector3Int, GameTile> _playArea;
        private int _dotCounter = 0;

        private int _points;
        private int _currentLevel = 1;

        public void Start()
        {
            Debug.Assert(Tilemap != null, "CRITICAL ERROR: The gameboard cannot be null.");
            Debug.Assert(PacMan != null, "CRITICAL ERROR: PacMan cannot be null.");

            GUI = gameObject.AddComponent<UIController>();
            GUI.Initialize();

            DotTile = Instantiate(Resources.Load<Tile>("Sprites/Dot"));
            PDotTile = Instantiate(Resources.Load<Tile>("Sprites/PDot"));
            EmptyTile = Instantiate(Resources.Load<Tile>("Sprites/Empty"));

            _initialize_board();
#if _DEV_LEVELPROGRESSION
            _clear_board();
#endif
            _reset_board();

            _currentLevel = 0;
            GUI.SetLevel(_currentLevel);

            PacMan.Initialize(this);
            PacMan.Warp(GetTile(_PLAYER_START), Direction.Right);
        }

        public void Update()
        {
#if _DEV
            GUI.DebugPowerPhase(PowerPhase, _currentPowerPhase);
            GUI.DebugDotCounter(_dotCounter);
#endif
            if (PowerPhase)
            {
                // we'll count down the power phase duration and terminate it.

                

            }
        }

        #region Board Initialization and Setup

        private void _initialize_board()
        {
            _playArea = new Dictionary<Vector3Int, GameTile>();

            for(int y = 0; y < _BOARD_HEIGHT; y++)
            {
                for(int x = 0; x < _BOARD_WIDTH; x++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    Tile tile = Tilemap.GetTile<Tile>(pos);

                    if (tile == null)
                    {
                        Tilemap.SetTile(pos, EmptyTile);
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
            _dotCounter = 0;
#if _DEV_LEVELPROGRESSION

            // we're going to add a single dot to 20, 7, 0
            GameTile tile = GetTile(new Vector3Int(20, 7, 0));
            tile.Reset();
            _dotCounter++;
#else
            foreach(GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
            {
                gameTile.Reset();
                if (gameTile.CurrentState == TileState.Dot || gameTile.CurrentState == TileState.PDot)
                    _dotCounter++;
            }
#endif

        }

#if _DEV_LEVELPROGRESSION
        private void _clear_board()
        {
            foreach (GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
            {
                gameTile.SetState(TileState.Empty);
            }
        }
#endif

        #endregion

        #region Tile Access and Mutation

        private Tile _stateMap(TileState state) => state switch
        {
            TileState.Empty => EmptyTile,
            TileState.Dot => DotTile,
            TileState.PDot => PDotTile,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No tile was found for the given state: {state}."),
        };
        private Vector3Int _neighbourMap(Vector3Int cell, Direction direction) => direction switch
        {
            Direction.Up => cell + Vector3Int.up,
            Direction.Down => cell + Vector3Int.down,
            Direction.Left => cell + Vector3Int.left,
            Direction.Right => cell + Vector3Int.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No neighbour could be resolved for the direction {direction} from the perspective of the cell at {cell.x},{cell.y}."),
        };

        public void SetTile(Vector3Int position, TileState state)
        {
            Tilemap.SetTile(position, _stateMap(state));
        }

        public GameTile GetTile(Vector3Int cell)
        {
            return _playArea[cell];
        }
        public GameTile GetTile(Vector3 point)
        {
            return GetTile(Tilemap.WorldToCell(point));
        }

        public GameTile GetTileNeighbour(Vector3Int cell, Direction direction)
        {
            return _playArea[_neighbourMap(cell, direction)];
        }
        public GameTile GetTileNeighbour(Vector3 point, Direction direction)
        {
            Vector3Int cellPos = Tilemap.WorldToCell(point);
            return GetTileNeighbour(cellPos, direction);
        }

        public bool DirectionBlocked(Vector3Int cellPos, Direction direction)
        {
            GameTile gameTile = GetTile(_neighbourMap(cellPos, direction));
            return gameTile.CurrentState == TileState.Wall;
        }
        public bool DirectionBlocked(Vector3 position, Direction direction)
        {
            Vector3Int cellPos = Tilemap.WorldToCell(position);
            return DirectionBlocked(cellPos, direction);
        }

        #endregion

        #region Points, Scoring, and Levels

        public void AddPoints(int points)
        {
            _points += points;
            GUI.SetScore(_points);
        }

        public void ConsumeDot(GameTile tile)
        {
            bool isPowerDot = tile.CurrentState == TileState.PDot;

            tile.SetState(TileState.Empty);
            AddPoints(isPowerDot ? 50 : 10);

            _dotCounter--;

            if (_dotCounter == 0)
            {
                PacMan.Warp(GetTile(_PLAYER_START), Direction.Right);

                // 1b. warp ghosts to the ghost house

                _reset_board();
                _currentLevel++;

                GUI.SetLevel(_currentLevel);
                GUI.DebugLevel(_currentLevel);

                return;
            }

            if (isPowerDot)
            {
                // activate power phase for ghosts.
                PowerPhase = true;

                // For the first four levels, the first two scatter periods last for seven seconds each.They change to five seconds each for level
                // five and beyond. The third scatter mode is always set to five seconds.The fourth scatter period lasts for five seconds on level one,
                // but then is only 1 / 60th of a second for the rest of play.

                // how long should this power phase be?
                _currentPowerPhase++;

                float duration = 5.0f;  // default to 5 seconds.

                if (_currentLevel >= 18)
                {
                    duration = 0.0f; // no more frightened
                }
                if (_currentLevel <= 3 && _currentPowerPhase <= 2)
                {
                    duration += 2.0f;  // add 2 seconds for levels 0 through 3 if the current power phase is the first or second
                }

            }
        }

#endregion
    }
}
