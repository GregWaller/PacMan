/*
 * Player behaviour for a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
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
    [RequireComponent(typeof(PlayerInput))]
    public class PacMan : Actor
    {
        private const float _CELL_CENTER_THRESHOLD = 0.01f;
        private const float _CELL_AREA_THRESHOLD = 0.35f;

        private PlayerInput _playerInput;
        private Direction _currentDirection = Direction.None;

        public override void Update()
        {
            Direction input = _currentDirection;
            if (_playerInput.actions["Up"].IsPressed())
            {
                input = Direction.Up;
            }
            else if (_playerInput.actions["Down"].IsPressed())
            {
                input = Direction.Down;
            }
            else if (_playerInput.actions["Right"].IsPressed())
            {
                input = Direction.Right;
            }
            else if (_playerInput.actions["Left"].IsPressed())
            {
                input = Direction.Left;
            }

            GameTile currentTile = _board.GetTile(transform.position);
            bool atJunction = Vector3.Distance(transform.position, currentTile.Position) <= _CELL_CENTER_THRESHOLD;

            // no input has been received from the player.  
            if (input == Direction.None)
                return;

            if (_currentDirection != input)
            {
                bool inputBlocked = _board.DirectionBlocked(transform.position, input);
                if (!inputBlocked && atJunction)
                {
                    _currentDirection = input;
                    _face(input);
                    _direction = _directionMap(input);
                }
            }
            else
            {
                bool pathBlocked = _board.DirectionBlocked(transform.position, _currentDirection);
                if (pathBlocked && atJunction)
                    _direction = Vector3.zero;
            }

            if (_direction != Vector3.zero)
                transform.Translate(_direction * (Time.deltaTime * _speed));

#if _DEV
            GameTile nextTile = _board.GetTileNeighbour(transform.position, _currentDirection);
            _board.GUI.DebugTileStates(currentTile.CurrentState, nextTile.CurrentState);
#endif

            if (currentTile.CurrentState == TileState.Dot || currentTile.CurrentState == TileState.PDot)
                _board.ConsumeDot(currentTile);
        }

        public override void Initialize(Gameboard gameboard)
        {
            base.Initialize(gameboard);

            _speed = 5.0f;

            // Since I was on the first level, Pac-Man was at 80 % of his full speed.
            // With some math, it turns out Pac - Man moves exactly 80 pixels per second, or 10 tiles per second.

            _playerInput = GetComponent<PlayerInput>();
        }
    }
}