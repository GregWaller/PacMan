/*
 * Enemy behaviour generalization in a Pac-Man facsimile.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

#define _DEV

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

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

        private Dictionary<Strategy, _strategyMethod> _strategyMap;

        public override void Update()
        {
            GameTile currentTile = _board.GetTile(transform.position);
            bool atJunction = Vector3.Distance(transform.position, currentTile.Position) <= GameTile.CELL_CENTER_THRESHOLD;

            if (atJunction)
            {
                Strategy levelStrategy = _select_strategy();
                Vector3Int targetCell = _strategyMap[levelStrategy]();
                _select_path(targetCell);
            }

            if (_direction != Vector3.zero)
                transform.Translate(_direction * (Time.deltaTime * _speed));
        }

        public override void Initialize(Gameboard board)
        {
            base.Initialize(board);
            _strategyMap = new Dictionary<Strategy, _strategyMethod>()
            {
                { Strategy.Chase, _chase },
                { Strategy.Scatter, _scatter },
                { Strategy.Frightened, _frightened },
                { Strategy.Eaten, _eaten },
            };
        }

        protected virtual Strategy _select_strategy()
        {
            return _board.LevelStrategy;
        }

        protected void _select_path(Vector3Int targetCell)
        {
            Dictionary<Direction,GameTile> viableOptions = new Dictionary<Direction, GameTile>();
            Vector3Int currentPosition = CurrentTile.CellPosition;

            Direction forward = _facing;
            Direction left = _facing.Left();
            Direction right = _facing.Right();

            if (!_board.DirectionBlocked(currentPosition, _facing))
                viableOptions.Add(forward, _board.GetTileNeighbour(currentPosition, forward));
            if (!_board.DirectionBlocked(currentPosition, _facing.Left()))
                viableOptions.Add(left, _board.GetTileNeighbour(currentPosition, left));
            if (!_board.DirectionBlocked(currentPosition, _facing.Right()))
                viableOptions.Add(right, _board.GetTileNeighbour(currentPosition, right));

            float shortestDistance = float.MaxValue;
            Direction selectedDirection = Direction.None;
            foreach(KeyValuePair<Direction,GameTile> option in viableOptions)
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

        protected abstract Vector3Int _scatter();
        protected abstract Vector3Int _chase();
        protected virtual Vector3Int _frightened()
        {
            return Vector3Int.zero;
        }
        protected virtual Vector3Int _eaten()
        {
            return Vector3Int.zero;
        }
    }
}