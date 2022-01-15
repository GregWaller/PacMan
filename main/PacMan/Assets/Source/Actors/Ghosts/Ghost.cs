using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace LongRoadGames.PacMan
{
    // reference for ghost AI: 
    // https://www.youtube.com/watch?v=ataGotQ7ir8

    public enum Strategy
    {
        Chase,
        Scatter,
        Frightened,
        Eaten,
    };

    public abstract class Ghost : Actor
    {
        public RuntimeAnimatorController FrightenedAnimatorController;
        public RuntimeAnimatorController EatenAnimatorController;

        // ----- Spawning and Respawning
        public static int POINT_VALUE = 200;
        protected abstract float _INITIAL_SPAWN_TIMER { get; }
        protected bool _active = false;
        protected bool _awaitingSpawn = false;
        protected bool _spawning = false;
        protected float _spawnTimer = 0.0f;
        protected Vector3 _ghostHomeCenter = new Vector3(14.0f, 16.5f, 0.0f);
        protected Vector3 _ghostHomeDoor = new Vector3(14.0f, 19.5f, 0.0f);
        protected Vector3 _spawnTransitionTarget = Vector3.zero;
        protected int _spawnStage = 0;
        protected bool _respawning = false;

        // ----- Pathing, AI, and Collisions
        
        protected abstract Vector3Int _SCATTER_TARGET { get; }
        protected Vector3Int _EATEN_TARGET = new Vector3Int(13, 19, 0);
        protected delegate Vector3Int _strategyMethod();
        protected Strategy _currentStrategy;
        private Dictionary<Strategy, _strategyMethod> _strategyMap;
        private bool _pathSelected = false;

        // ----- State Alternatives
        protected RuntimeAnimatorController _primaryAC;

        public override void Update()
        {
            if (_board.LevelInProgress)
            {
                if (_active)
                {
                    GameTile currentTile = CurrentTile;
                    bool atJunction = Vector3.Distance(transform.position, currentTile.Position) <= GameTile.CELL_COLLISION_THRESHOLD;
                    if (atJunction && !_pathSelected)
                        _execute_strategy();

                    _check_warp(currentTile);

                    _update_gameplay_position();

                    if (_check_pacman_collision())
                    {
                        if (_currentStrategy == Strategy.Frightened)
                        {
                            _currentStrategy = Strategy.Eaten;
                            _animator.runtimeAnimatorController = EatenAnimatorController;
                            _board.ConsumeGhost(this);
                        }
                        else
                        {
                            _board.PacMan.Capture();
                            return;
                        }
                    }

                    if (_check_new_tile(currentTile))
                    {
                        _pathSelected = false;

                        if (_currentStrategy == Strategy.Eaten && _board.GetTile(_EATEN_TARGET) == CurrentTile)
                        {
                            _respawning = true;
                            _active = false;
                            _spawnTransitionTarget = _ghostHomeCenter;
                            _spawnStage = 0;
                        }
                    }
                }
                else
                {
                    if (_awaitingSpawn || _spawning)
                        _update_spawn();
                    else if (_respawning)
                        _update_respawn();
                }
            }
        }

        #region Initialization, Spawning, and Resets

        public override void Initialize(Gameboard board)
        {
            base.Initialize(board);
            _primaryAC = _animator.runtimeAnimatorController;

            _speed = 3.0f;
            _currentStrategy = Strategy.Scatter; 

            _strategyMap = new Dictionary<Strategy, _strategyMethod>()
            {
                { Strategy.Chase, _chase },
                { Strategy.Scatter, _scatter },
                { Strategy.Frightened, _frightened },
                { Strategy.Eaten, _eaten },
            };
        }

        public override void Begin()
        {
            _active = true;
            _speed = 4.5f;
            _execute_strategy();
            base.Begin();
        }

        public override void ResetPosition()
        {
            base.ResetPosition();
            _active = false;
            _spawnTimer = _INITIAL_SPAWN_TIMER;
            _spawnStage = 0;
            _spawning = false;
            _awaitingSpawn = true;
            _respawning = false;

            if (_animator.runtimeAnimatorController != _primaryAC)
                _animator.runtimeAnimatorController = _primaryAC;
        }

        private void _update_spawn()
        {
            if (_spawning)
            {
                float distanceToTarget = Vector3.Distance(transform.position, _spawnTransitionTarget);

                if (_spawnStage == 0 && distanceToTarget <= GameTile.CELL_COLLISION_THRESHOLD)
                {
                    _spawnStage = 1;
                    _spawnTransitionTarget = _ghostHomeDoor;
                }
                else if (_spawnStage == 1 && distanceToTarget <= GameTile.CELL_COLLISION_THRESHOLD)
                {
                    _spawning = false;
                    Begin();
                }

                Vector3 direction = (_spawnTransitionTarget - transform.position).normalized;
                transform.Translate(direction * (Time.deltaTime * _speed));
            }
            else
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0.0f)
                {
                    _awaitingSpawn = false;
                    _spawning = true;
                    _spawnTimer = 0.0f;
                    _spawnStage = 0;
                    _spawnTransitionTarget = _ghostHomeCenter;
                }
            }
        }

        private void _update_respawn()
        {
            float distanceToTarget = Vector3.Distance(transform.position, _spawnTransitionTarget);

            if (_spawnStage == 0 && distanceToTarget <= GameTile.CELL_COLLISION_THRESHOLD)
            {
                _spawnStage = 1;
                _spawnTransitionTarget = _INITIAL_POSITION;
            }
            else if (_spawnStage == 1 && distanceToTarget <= GameTile.CELL_COLLISION_THRESHOLD)
            {
                _respawning = false;
                _spawning = true;
                _spawnStage = 0;
                _awaitingSpawn = false;
                _spawnTimer = 0.0f;
                _spawnTransitionTarget = _ghostHomeCenter;
                _currentStrategy = _board.LevelStrategy;
                _animator.runtimeAnimatorController = _primaryAC;
            }

            Vector3 direction = (_spawnTransitionTarget - transform.position).normalized;
            transform.Translate(direction * (Time.deltaTime * _speed));
        }

        #endregion

        #region Strategy AI

        public void Frighten()
        {
            if (_currentStrategy == Strategy.Frightened && _currentStrategy == Strategy.Eaten)
                return;

            _currentStrategy = Strategy.Frightened;
            _animator.runtimeAnimatorController = FrightenedAnimatorController;
        }
        public void Resume()
        {
            if (_currentStrategy == Strategy.Eaten)
                return;

            _currentStrategy = _board.LevelStrategy;
            _animator.runtimeAnimatorController = _primaryAC;
        }

        protected void _execute_strategy()
        {
            Strategy levelStrategy = _select_strategy();
            Vector3Int targetCell = _select_target(levelStrategy);
            _select_path(targetCell);
            _pathSelected = true;
        }
        protected virtual Strategy _select_strategy()
        {
            if (_currentStrategy == Strategy.Eaten || _currentStrategy == Strategy.Frightened)
                return _currentStrategy;

            return _board.LevelStrategy;
        }
        protected Vector3Int _select_target(Strategy levelStrategy)
        {
            if (levelStrategy != _currentStrategy)
            {
                _face(Facing.Flip());
                _currentStrategy = levelStrategy;
            }

            return _strategyMap[_currentStrategy]();
        }
        protected void _select_path(Vector3Int targetCell)
        {
            Dictionary<Direction, GameTile> viableOptions = _evaluate_options();

            float shortestDistance = float.MaxValue;
            Direction selectedDirection = Direction.None;
            foreach (KeyValuePair<Direction, GameTile> option in viableOptions)
            {
                GameTile tile = option.Value;
                float distance = Vector3Int.Distance(tile.CellPosition, targetCell);
                if (distance < shortestDistance)
                {
                    selectedDirection = option.Key;
                    shortestDistance = distance;
                }
            }

            _face(selectedDirection);
            _direction = _directionMap(selectedDirection);
        }
        protected Dictionary<Direction, GameTile> _evaluate_options()
        {
            Dictionary<Direction, GameTile> viableOptions = new Dictionary<Direction, GameTile>();

            Vector3Int currentPosition = CurrentTile.CellPosition;
            Direction forward = Facing;
            Direction left = Facing.Left();
            Direction right = Facing.Right();

            if (!_board.DirectionBlocked(currentPosition, forward))
                viableOptions.Add(forward, _board.GetTileNeighbour(currentPosition, forward));

            if (!_board.DirectionBlocked(currentPosition, left))
                viableOptions.Add(left, _board.GetTileNeighbour(currentPosition, left));

            if (!_board.DirectionBlocked(currentPosition, right))
                viableOptions.Add(right, _board.GetTileNeighbour(currentPosition, right));

            return viableOptions;
        }

        protected abstract Vector3Int _chase();

        protected virtual Vector3Int _scatter()
        {
            return _SCATTER_TARGET;
        }
        protected virtual Vector3Int _frightened()
        {
            Dictionary<Direction, GameTile> viableOptions = _evaluate_options();
            List<GameTile> options = viableOptions.Values.ToList();
            return options.Random().CellPosition;
        }
        protected virtual Vector3Int _eaten()
        {
            return _EATEN_TARGET;
        }

        protected override void _face(Direction facing)
        {
            if (_animator.runtimeAnimatorController != FrightenedAnimatorController)
                _animator.SetTrigger(facing.ToString());
            Facing = facing;
        }

        #endregion

        #region Actor Awareness

        private bool _check_pacman_collision()
        {
            if (_currentStrategy == Strategy.Eaten)
                return false;

            float distance = Vector3.Distance(transform.position, _board.PacMan.transform.position);
            return distance <= PacMan.COLLISION_THRESHOLD;
        }

        #endregion
    }
}