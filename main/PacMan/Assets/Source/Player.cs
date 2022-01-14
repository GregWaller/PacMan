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
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    };

    [RequireComponent(typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private Animator _animator;

        private GameMaster _master;
        private const float _speed = 5.0f;
        private Vector3 _direction = Vector3.zero;

        public void Update()
        {
            if (_direction != Vector3.zero)
            {
                transform.Translate(_direction * (Time.deltaTime * _speed));
            }
        }

        public void Initialize(GameMaster master)
        {
            _master = master;
            _playerInput = GetComponent<PlayerInput>();
            _animator = GetComponent<Animator>();

            _playerInput.actions["Up"].performed += _up;
            _playerInput.actions["Down"].performed += _down;
            _playerInput.actions["Left"].performed += _left;
            _playerInput.actions["Right"].performed += _right;
        }

        public void Warp(GameTile tile, Direction facing)
        {
            transform.SetPositionAndRotation(tile.Position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            _face(facing);
        }

        private Vector3 _directionMap(Direction facing) => facing switch
        {
            Direction.Up => transform.up,
            Direction.Down => -transform.up,
            Direction.Left => -transform.right,
            Direction.Right => transform.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No direction matching the provided facing: {facing}."),
        };

        private void _face(Direction facing)
        {
            _animator.SetTrigger(facing.ToString());
        }

        private void _up(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Up;
            
            bool blocked = _master.DirectionBlocked(transform.position, facing); 
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _down(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Down;

            bool blocked = _master.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _left(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Left;

            bool blocked = _master.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }

        private void _right(InputAction.CallbackContext context)
        {
            Direction facing = Direction.Right;

            bool blocked = _master.DirectionBlocked(transform.position, facing);
            if (!blocked)
            {
                _face(facing);
                _direction = _directionMap(facing);
            }
        }
    }
}