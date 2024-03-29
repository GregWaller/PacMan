﻿#define _DEV
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
        private const int _TARGET_FPS = 60;

        // ----- Audio
        public AudioController AudioController => AudioController.Instance;

        // ----- Actors
        public PacMan PacMan;
        public Blinky Blinky;
        public Inky Inky;
        public Pinky Pinky;
        public Clyde Clyde;

        // ----- Play Area
        public FruitSpawner FruitSpawner;
        public GameTile LeftWarp { get; private set; }
        public GameTile RightWarp { get; private set; }
        private const int _BOARD_WIDTH = 28;
        private const int _BOARD_HEIGHT = 31;
        private Dictionary<Vector3Int, GameTile> _playArea;
        private const float _POWER_DOT_FLASH_FREQUENCY = 0.25f;
        private List<Vector3Int> _powerDotCells;
        private float _powerDotFlashTimer = 1.0f;
        private bool _powerDotsOn = true;

        // ----- Power Phases
        public bool PowerPhase { get; private set; } = false;
        private readonly List<float> _powerPhaseDurations = new List<float> 
        { 
            6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 5.0f, 2.0f, 2.0f, 1.0f, 5.0f, 2.0f, 1.0f, 1.0f, 3.0f, 1.0f, 1.0f, 0.0f, 1.0f 
        };
        private int _currentPowerPhase = 0;
        private float _powerPhaseDuration = 0.0f;
        private int _ghostsEaten = 0;

        // ----- Level Phase and Strategy
        public bool LevelInProgress { get; private set; } = false;
        public int CurrentLevel { get; private set; } = 0;
        public Strategy LevelStrategy { get; private set; }
        protected int _levelPhase;
        protected float _phaseTimer = 0.0f;

        // ----- Scoring
        public int DotsRemaining { get; private set; } = 0;
        private const string _HIGHSCORE_KEY = "HighScore";
        private const int _BONUS_LIFE = 10000;
        private int _nextBonusLife = 0;
        private int _maxDots = 0;
        private int _score = 0;
        private int _highScore = 0;
        private float _readyCountdown = 5.0f;
        private int _fruitSpawn = 0;

        // ----- Pausing
        public delegate void PauseComplete();
        public bool Paused { get; private set; }
        private const float _DEFAULT_SCREEN_PAUSE = 0.75f; 
        private float _pauseDuration = 0.0f;
        private PauseComplete _pauseCompleteCB;

        public void Start()
        {
            Application.targetFrameRate = _TARGET_FPS;
            Screen.SetResolution(1920, 1080, false); // force to 1080p resolution

#if DEBUG
            Debug.Assert(Tilemap != null, "CRITICAL ERROR: The gameboard cannot be null.");
            Debug.Assert(PacMan != null, "CRITICAL ERROR: PacMan cannot be null.");
#endif

            GUI = gameObject.AddComponent<UIController>();
            GUI.Initialize(this);

            AudioSource loopSource = transform.Find("Audio/Loop").gameObject.GetComponent<AudioSource>();
            AudioSource sfxSource = transform.Find("Audio/SFX").gameObject.GetComponent<AudioSource>();
            AudioController.Initialize(sfxSource, loopSource);

            DotTile = Instantiate(Resources.Load<Tile>("Sprites/Dot"));
            PDotTile = Instantiate(Resources.Load<Tile>("Sprites/PDot"));
            EmptyTile = Instantiate(Resources.Load<Tile>("Sprites/Empty"));

            _initialize_board();
            _initialize_actors();

            ResetGame();
        }

        public void Update()
        {
#if _DEV
            GUI.DebugPowerPhase(PowerPhase, _currentPowerPhase);
            GUI.DebugDotCounter(DotsRemaining, _maxDots);
#endif

            _powerDotFlashTimer -= Time.deltaTime;
            if (_powerDotFlashTimer < 0.0f)
            {
                _powerDotsOn = !_powerDotsOn;
                foreach(Vector3Int cell in _powerDotCells)
                    Tilemap.SetColor(cell, _powerDotsOn ? Color.white : Color.clear);
                _powerDotFlashTimer = _POWER_DOT_FLASH_FREQUENCY;
            }

            if (Paused)
            {
                _pauseDuration -= Time.deltaTime;
                if (_pauseDuration <= 0.0f)
                {
                    _pauseDuration = 0.0f;
                    _pauseCompleteCB?.Invoke();
                    Paused = false;
                }
            }
            else
            {
                if (!LevelInProgress)
                {
                    _readyCountdown -= Time.deltaTime;
                    if (_readyCountdown <= 0.0f)
                    {
                        LevelInProgress = true;
                        _readyCountdown = 0.0f;

                        PacMan.Begin();
                        Blinky.Begin();
                        GUI.ShowReady(false);
                    }
                }
                else
                {
                    if (PowerPhase)
                    {
                        _powerPhaseDuration -= Time.deltaTime;
                        if (_powerPhaseDuration <= 0.0f)
                            _end_power_phase();
                    }
                    else
                    {
                        _update_level_phase();
                    }
                }
            }
        }

        #region Board Initialization and Setup

        public void ResetLevel(bool resetDots)
        {
            if (resetDots)
            {
                _reset_dots();
                _currentPowerPhase = 0;
                _fruitSpawn = 0;

                _powerDotsOn = true;
                _powerDotFlashTimer = _POWER_DOT_FLASH_FREQUENCY;
            }

            _reset_actors();
            _reset_level_phase();

            if (FruitSpawner.Spawned)
                FruitSpawner.Despawn();

            if (PowerPhase)
                _end_power_phase();

            _prep_level();
        }

        public void ResetGame()
        {
            _score = 0;
            _nextBonusLife = _BONUS_LIFE;
            GUI.SetScore(0);

            _highScore = PlayerPrefs.GetInt(_HIGHSCORE_KEY, 0);
            GUI.SetHighScore(_highScore);

            CurrentLevel = 0;
            GUI.SetLevel(CurrentLevel);

            PacMan.ResetLives();
            ResetLevel(true);
        }

        private void _initialize_board()
        {
            _playArea = new Dictionary<Vector3Int, GameTile>();
            for (int y = 0; y < _BOARD_HEIGHT; y++)
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

            FruitSpawner.Initialize(this);
            _powerDotCells = new List<Vector3Int>();

            LeftWarp = _playArea[new Vector3Int(0, 16, 0)];
            LeftWarp.OverwriteState(TileState.LeftWarp);
            RightWarp = _playArea[new Vector3Int(27, 16, 0)];
            RightWarp.OverwriteState(TileState.RightWarp);
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
            _clear_dots();
            GameTile tile = GetTile(new Vector3Int(20, 7, 0));
            tile.Reset();
            DotsRemaining++;
#else

            _powerDotCells.Clear();

            foreach(GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
            {
                gameTile.Reset();
                if (gameTile.CurrentState == TileState.Dot || gameTile.CurrentState == TileState.PDot)
                    DotsRemaining++;

                if (gameTile.CurrentState == TileState.PDot)
                {
                    _powerDotCells.Add(gameTile.CellPosition);
                    Tilemap.SetTileFlags(gameTile.CellPosition, TileFlags.None);
                }
            }

            _maxDots = DotsRemaining;
#endif
        }

#if _DEV_LEVELPROGRESSION
        private void _clear_dots()
        {
            foreach (GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
            {
                if (gameTile.CurrentState == TileState.Dot || gameTile.CurrentState == TileState.PDot)
                {
                    gameTile.SetState(TileState.Empty);
                }
            }
        }
#endif

        private void _reset_actors()
        {
            PacMan.ResetPosition();
            Blinky.ResetPosition();
            Inky.ResetPosition();
            Pinky.ResetPosition();
            Clyde.ResetPosition();
        }

        private void _prep_level()
        {
            LevelInProgress = false;
            _readyCountdown = AudioController.Duration(AudioClipID.GameStart);
            AudioController.Play(AudioClipID.GameStart);
            GUI.ShowGameOver(false);
            GUI.ShowReady(true);
            GUI.SetLives(PacMan.ExtraLives);
        }

        #endregion

        #region Tile Access and Mutation

        private Tile _stateMap(TileState state) => state switch
        {
            TileState.Empty => EmptyTile,
            TileState.Dot => DotTile,
            TileState.PDot => PDotTile,
            TileState.LeftWarp => EmptyTile,
            TileState.RightWarp => EmptyTile,
            TileState.WarpTunnel => EmptyTile,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No base tile was found for the state: {state}."),
        };
        private Vector3Int _neighbourMap(Vector3Int cell, Direction direction) => direction switch
        {
            Direction.Up => cell + Vector3Int.up,
            Direction.Down => cell + Vector3Int.down,
            Direction.Left => cell + Vector3Int.left,
            Direction.Right => cell + Vector3Int.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: A valid direction must be supplied in order to calculate the coordinates of a tile's neighbours."),
        };

        public void SetTile(Vector3Int position, TileState state)
        {
            Tilemap.SetTile(position, _stateMap(state));
        }

        public GameTile GetTile(Vector3Int cell)
        {
#if DEBUG
            Debug.Assert(_playArea.ContainsKey(cell), $"CRITICAL ERROR: The coordinate ({cell.x},{cell.y}) is not within the bounds of the gameboard.");
#endif
            return _playArea[cell];
        }
        public GameTile GetTile(Vector3 point)
        {
            return GetTile(Tilemap.WorldToCell(point));
        }

        public GameTile GetTileNeighbour(Vector3Int cell, Direction direction)
        {
            Vector3Int neighbourCell = cell;
            GameTile currentTile = _playArea[cell];

            if (currentTile == LeftWarp)
                neighbourCell = RightWarp.CellPosition;
            else if (currentTile == RightWarp)
                neighbourCell = LeftWarp.CellPosition;
            else
                neighbourCell = _neighbourMap(cell, direction);

#if DEBUG
            Debug.Assert(_playArea.ContainsKey(neighbourCell), $"CRITICAL ERROR: Could not determine the {direction} neighbour of ({currentTile.CurrentState} @ [{cell.x},{cell.y}]) as the neighbour coordinate is not within the bounds of the gameboard.");
#endif
            return _playArea[neighbourCell];
        }
        public GameTile GetTileNeighbour(Vector3 point, Direction direction)
        {
            Vector3Int cellPos = Tilemap.WorldToCell(point);
            return GetTileNeighbour(cellPos, direction);
        }

        public bool DirectionBlocked(Vector3Int cellPos, Direction direction)
        {
            GameTile gameTile = GetTileNeighbour(cellPos, direction);
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
            _score += points;
            GUI.SetScore(_score);

            if (_score > _highScore)
            {
                _highScore = _score;
                PlayerPrefs.SetInt(_HIGHSCORE_KEY, _highScore);
                GUI.SetHighScore(_highScore);
            }

            if (_score >= _nextBonusLife)
            {
                _nextBonusLife += _BONUS_LIFE;
                PacMan.BonusLife();
                AudioController.Play(AudioClipID.Extend);
            }
        }

        public bool ConsumeDot(GameTile tile)
        {
            bool isPowerDot = tile.CurrentState == TileState.PDot;
            bool isDot = tile.CurrentState == TileState.Dot;

            AddPoints((int)tile.CurrentState);
            tile.SetState(TileState.Empty);

            if (isDot || isPowerDot)
            {
                DotsRemaining--;

                AudioController.PlayChomp();

                if (_fruitSpawn == 0 && DotsRemaining == _maxDots - 70 ||
                    _fruitSpawn == 1 && DotsRemaining == _maxDots - 170)
                    _spawn_fruit();
            }

            if (DotsRemaining == 0)
            {
                ResetLevel(true);

                CurrentLevel++;
                GUI.SetLevel(CurrentLevel);
                GUI.DebugLevel(CurrentLevel);

                _prep_level();

                return false;
            }

            if (isPowerDot) _begin_power_phase();

            return true;
        }

        public void ConsumeGhost(Ghost ghost)
        {
            _ghostsEaten++;
            int points = Ghost.POINT_VALUE * _ghostsEaten;
            AddPoints(points);
            GUI.BonusPoints.Display(points, ghost.CurrentTile.Position);
            AudioController.Play(AudioClipID.EatGhost);

            Pause();
        }

        private void _spawn_fruit()
        {
            FruitSpawner.Spawn(CurrentLevel);
            _fruitSpawn++;
        }

        #endregion

        #region Power Phases

        private void _begin_power_phase()
        {
            _powerPhaseDuration = _fright_time();
            if (_powerPhaseDuration > 0.0f)
            {
                PowerPhase = true;
                _currentPowerPhase++;

                _ghostsEaten = 0;
                AudioController.StartLoop(AudioClipID.PowerPellet);

                Pause();

                Blinky.Frighten();
                Pinky.Frighten();
                Inky.Frighten();
                Clyde.Frighten();
            }
        }

        private void _end_power_phase()
        {
            PowerPhase = false;
            _powerPhaseDuration = 0.0f;
            _ghostsEaten = 0;
            AudioController.StopLoop();

            Blinky.Resume();
            Pinky.Resume();
            Inky.Resume();
            Clyde.Resume();
        }

        private float _fright_time()
        {
            if (CurrentLevel < _powerPhaseDurations.Count)
                return _powerPhaseDurations[CurrentLevel];

            return 0.0f;
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

        protected virtual void _update_level_phase()
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

                    _phaseTimer = _phase_duration(_levelPhase, LevelStrategy);
                }
            }
        }

        private void _reset_level_phase()
        {
            _levelPhase = 0;
            _phaseTimer = CurrentLevel < 5 ? 7.0f : 5.0f;
            LevelStrategy = Strategy.Scatter;
        }

        private float _phase_duration(int levelPhase, Strategy strategy)
        {
            if (levelPhase < 2)
            {
                if (strategy == Strategy.Chase)
                    return 20.0f;
                else if (CurrentLevel < 5)
                    return 7.0f;
                else
                    return 5.0f;
            }
            else if (levelPhase == 2)
            {
                if (strategy == Strategy.Scatter)
                    return 5.0f;
                else if (CurrentLevel == 0)
                    return 20.0f;
                else
                    return 17.0f;
            }
            else
            {
                if (strategy == Strategy.Chase)
                    return 0.0f;
                else if (CurrentLevel == 0)
                    return 5.0f;
                else
                    return 0.01f;
            }
        }

        #endregion

        #region Animation Pauses (Dramatic Effect)

        public void Pause(float pauseDuration = _DEFAULT_SCREEN_PAUSE, PauseComplete pauseCompleteCallback = null)
        {
            _pauseCompleteCB = pauseCompleteCallback;
            _pauseDuration = pauseDuration;
            Paused = true;
        }

        #endregion
    }
}

