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
        protected delegate Vector3Int _strategyMethod();
        protected abstract Vector3Int _SCATTER_TARGET { get; }
        protected abstract float _INITIAL_SPAWN_TIMER { get; }

        protected bool _active = false;
        protected bool _spawning = false;
        protected float _spawnTimer = 0.0f;
        
        protected Vector3 _ghostHomeCenter = new Vector3(14.0f, 16.5f, 0.0f);
        protected Vector3 _ghostHomeDoor = new Vector3(14.0f, 19.5f, 0.0f);
        protected Vector3 _spawnTransitionTarget = Vector3.zero;
        protected int _spawnStage = 0;

        private Dictionary<Strategy, _strategyMethod> _strategyMap;
        private Strategy _currentLevelStrategy;
        private bool _pathSelected = false;

        public override void Update()
        {
            if (_board.LevelInProgress)
            {
                if (_active)
                {
                    GameTile currentTile = CurrentTile;

                    bool atJunction = Vector3.Distance(transform.position, currentTile.Position) <= GameTile.CELL_COLLISION_THRESHOLD;

                    if (atJunction && !_pathSelected)
                    {
                        // FIXME: it appears as though ghosts are still making multiple decisions for a given tile.

                        _select_target();
                        GameTile neighbour = _board.GetTileNeighbour(currentTile.CellPosition, Facing);
                        //Debug.Log($"{name} is deciding a target for the tile at {currentTile.CellPosition} with the {Facing} neighbour is {neighbour.CurrentState}");
                        _pathSelected = true;
                    }

                    _check_warp(currentTile);

                    _update_gameplay_position();

                    if (_check_new_tile(currentTile))
                        _pathSelected = false;
                }
                else
                {
                    _update_spawn();
                }
            }
        }

        #region Initialization, Spawning, and Resets

        public override void Initialize(Gameboard board)
        {
            base.Initialize(board);

            _speed = 3.0f;
            _currentLevelStrategy = Strategy.Scatter; 

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
            _select_target();
            base.Begin();
        }

        public override void Reboot()
        {
            base.Reboot();
            _active = false;
            _spawnTimer = _INITIAL_SPAWN_TIMER;
        }

        private void _update_spawn()
        {
            if (_spawning)
            {
                float distanceToTarget = Vector3.Distance(transform.position, _spawnTransitionTarget);

                if (_spawnStage == 0 && distanceToTarget <= 0.01f)
                {
                    _spawnStage = 1;
                    _spawnTransitionTarget = _ghostHomeDoor;
                }
                else if (_spawnStage == 1 && distanceToTarget <= 0.01f)
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
                    _spawning = true;
                    _spawnTimer = 0.0f;
                    _spawnStage = 0;
                    _spawnTransitionTarget = _ghostHomeCenter;
                }
            }

        }

        #endregion

        #region Strategy AI

        protected void _select_target()
        {
            Strategy levelStrategy = _select_strategy();

            if (levelStrategy != _currentLevelStrategy)
            {
                _face(Facing.Flip());
                _currentLevelStrategy = levelStrategy;
            }

            Vector3Int targetCell = _strategyMap[_currentLevelStrategy]();
            _select_path(targetCell);
        }
        protected virtual Strategy _select_strategy()
        {
            return _board.LevelStrategy;
        }
        protected void _select_path(Vector3Int targetCell)
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

        protected abstract Vector3Int _chase();
        protected virtual Vector3Int _scatter()
        {
            return _SCATTER_TARGET;
        }
        protected virtual Vector3Int _frightened()
        {
            return Vector3Int.zero;
        }
        protected virtual Vector3Int _eaten()
        {
            return Vector3Int.zero;
        }

        #endregion
    }
}