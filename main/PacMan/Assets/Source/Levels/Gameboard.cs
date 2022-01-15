#define _DEV

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
        Right,
        Down,
        Left,
    };

    public class Gameboard : MonoBehaviour
    {
        // ----- GUI
        public Tilemap Tilemap;
        public Tile DotTile { get; private set; }
        public Tile PDotTile { get; private set; }
        public Tile EmptyTile { get; private set; }
        public UIController GUI { get; private set; }

        // ----- ACTORS
        public PacMan PacMan;
        public Blinky Blinky;
        public Inky Inky;
        public Pinky Pinky;
        public Clyde Clyde;

        // ----- PLAY AREA
        private const int _BOARD_WIDTH = 28;
        private const int _BOARD_HEIGHT = 31;
        private Dictionary<Vector3Int, GameTile> _playArea;

        // ----- POWER PHASES
        public bool PowerPhase { get; private set; } = false;
        private int _currentPowerPhase = 0;
        private float _readyCountdown = 5.0f;

        // ----- LEVEL PHASE AND STRATEGY
        public bool LevelInProgress { get; private set; } = false;
        public int CurrentLevel { get; private set; } = 0;
        public Strategy LevelStrategy { get; private set; }
        protected int _levelPhase;
        protected float _phaseTimer = 0.0f;

        // ---- SCORING
        private int _points = 0;
        public int DotsRemaining { get; private set; } = 0;

        public void Start()
        {
            Application.targetFrameRate = 60;

            Debug.Assert(Tilemap != null, "CRITICAL ERROR: The gameboard cannot be null.");
            Debug.Assert(PacMan != null, "CRITICAL ERROR: PacMan cannot be null.");

            GUI = gameObject.AddComponent<UIController>();
            GUI.Initialize();

            DotTile = Instantiate(Resources.Load<Tile>("Sprites/Dot"));
            PDotTile = Instantiate(Resources.Load<Tile>("Sprites/PDot"));
            EmptyTile = Instantiate(Resources.Load<Tile>("Sprites/Empty"));

            _initialize_board();
            _initialize_actors();
            _reset_dots();
            _reset_phase();

            CurrentLevel = 0;
            GUI.SetLevel(CurrentLevel);

            _readyCountdown = 3.0f;
            GUI.ShowReady(true);
        }

        public void Update()
        {
#if _DEV
            GUI.DebugPowerPhase(PowerPhase, _currentPowerPhase);
            GUI.DebugDotCounter(DotsRemaining);
#endif

            if (!LevelInProgress)
            {
                _readyCountdown -= Time.deltaTime;
                if (_readyCountdown <= 0.0f)
                {
                    LevelInProgress = true;
                    _readyCountdown = 0.0f;

                    // set pac-man in motion 
                    PacMan.Begin();
                    Blinky.Begin();
                    GUI.ShowReady(false);
                }
            }
            else
            {
                _update_phase();

                if (PowerPhase)
                {
                    // we'll count down the power phase duration and terminate it.



                }
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

        private void _initialize_actors()
        {
            PacMan.Initialize(this);
            Blinky.Initialize(this);
            Inky.Initialize(this);
            Pinky.Initialize(this);
            Clyde.Initialize(this);
        }

        private void _reset_dots()
        {
            DotsRemaining = 0;
#if _DEV_LEVELPROGRESSION

            GameTile tile = GetTile(new Vector3Int(20, 7, 0));
            tile.Reset();
            _dotCounter++;
#else
            foreach(GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
            {
                gameTile.Reset();
                if (gameTile.CurrentState == TileState.Dot || gameTile.CurrentState == TileState.PDot)
                    DotsRemaining++;
            }
#endif

        }

        private void _reset_actors()
        {
            PacMan.Reboot();
            Blinky.Reboot();
            Inky.Reboot();
            Pinky.Reboot();
            Clyde.Reboot();
        }

        private void _reset_phase()
        {
            _levelPhase = 0;
            _phaseTimer = CurrentLevel < 5 ? 7.0f : 5.0f;
            LevelStrategy = Strategy.Scatter;
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
            Vector3Int neighbourCell = _neighbourMap(cell, direction);
            if (_playArea.ContainsKey(neighbourCell))
                return _playArea[neighbourCell];

            return null;
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

            DotsRemaining--;

            if (DotsRemaining == 0)
            {
                _reset_actors();
                _reset_dots();
                _reset_phase();

                CurrentLevel++;
                GUI.SetLevel(CurrentLevel);
                GUI.DebugLevel(CurrentLevel);

                LevelInProgress = false;
                _readyCountdown = 5.0f;
                GUI.ShowReady(true);

                return;
            }

            if (isPowerDot && CurrentLevel < 16 || CurrentLevel == 17)
            {
                PowerPhase = true;
            }
        }

        #endregion

        #region Level Strategy and Phases

        /* 
            Phases start with scatter and alternate for the duration of the level as follows (values in seconds)

                             phase 0                 phase 1                 phase 2                 phase 3
                             scatter     chase       scatter     chase       scatter     chase       scatter     chase       
            level 1          7           20          7           20          5           20          5           -
            level 2-4        7           20          7           20          5           17          .01         -
            level 5+         5           20          5           20          5           17          .01         -
        */

        protected virtual void _update_phase()
        {
            if (_levelPhase < 3)
            {
                _phaseTimer -= Time.deltaTime;
                if (_phaseTimer <= 0.0f)
                {
                    // alternate between scatter and chase
                    LevelStrategy = LevelStrategy == Strategy.Scatter ? Strategy.Chase : Strategy.Scatter;

                    // increment the phase counter every time we come back to scatter
                    if (LevelStrategy == Strategy.Scatter)
                        _levelPhase++;

                    if (_levelPhase < 2)
                    {
                        if (LevelStrategy == Strategy.Chase)
                        {
                            _phaseTimer = 20.0f;
                        }
                        else if (CurrentLevel < 5)
                        {
                            _phaseTimer = 7.0f;
                        }
                        else
                        {
                            _phaseTimer = 5.0f;
                        }
                    }
                    else if (_levelPhase == 2)
                    {
                        if (LevelStrategy == Strategy.Scatter)
                        {
                            _phaseTimer = 5.0f;
                        }
                        else if (CurrentLevel == 0)
                        {
                            _phaseTimer = 20.0f;
                        }
                        else
                        {
                            _phaseTimer = 17.0f;
                        }
                    }
                    else
                    {
                        if (LevelStrategy == Strategy.Chase)
                        {
                            _phaseTimer = 0.0f;
                        }
                        else if (CurrentLevel == 0)
                        {
                            _phaseTimer = 5.0f;
                        }
                        else
                        {
                            _phaseTimer = 0.01f;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
