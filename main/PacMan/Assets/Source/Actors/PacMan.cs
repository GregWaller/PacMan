/*
 * Player behaviour for a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

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
        protected override float _speed => 5.0f;

        private PlayerInput _playerInput;

        public void Update()
        {
            if (_direction != Vector3.zero)
            {
                transform.Translate(_direction * (Time.deltaTime * _speed));
            }
        }

        public override void Initialize(Gameboard gameboard)
        {
            base.Initialize(gameboard);

            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions["Up"].performed += _up;
            _playerInput.actions["Down"].performed += _down;
            _playerInput.actions["Left"].performed += _left;
            _playerInput.actions["Right"].performed += _right;
        }

        private void _up(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Up;
            
            bool blocked = _board.DirectionBlocked(transform.position, facing); 
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _down(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Down;

            bool blocked = _board.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _left(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Left;

            bool blocked = _board.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _right(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Right;

            bool blocked = _board.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }
    }
}