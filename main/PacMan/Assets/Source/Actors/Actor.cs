/*
 * Generalization of an actor.
 * 
 * This class represents the abstraction of an actor during the course of play and provides
 * a generalized interface for those objects.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

using System;
using UnityEngine;

namespace LongRoadGames.PacMan
{
    public abstract class Actor : MonoBehaviour
    {
        public GameTile CurrentTile => _board.GetTile(transform.position);

        protected abstract Vector3 _INITIAL_POSITION { get; }
        protected abstract Direction _INITIAL_FACING { get; }

        protected Animator _animator;
        protected Gameboard _board;

        protected float _speed = 0.0f;
        protected Vector3 _direction = Vector3.zero;
        protected Direction _facing = Direction.Right;

        public virtual void Update() { }

        public virtual void Initialize(Gameboard board)
        {
            _board = board;
            _animator = GetComponent<Animator>();
            _speed = 3.0f;
            Reboot();
        }

        public virtual void Reboot()
        {
            _facing = Direction.Right;
            _direction = Vector3.zero;
            Warp(_INITIAL_POSITION, _INITIAL_FACING);
        }

        public virtual void Begin() { }

        public void Warp(GameTile tile, Direction facing)
        {
            Warp(tile.Position, facing);
        }
        public void Warp(Vector3 position, Direction facing)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            _face(facing);
        }

        protected void _face(Direction facing)
        {
            _animator.SetTrigger(facing.ToString());
            _facing = facing;
        }

        protected Vector3 _directionMap(Direction facing) => facing switch
        {
            Direction.Up => transform.up,
            Direction.Down => -transform.up,
            Direction.Left => -transform.right,
            Direction.Right => transform.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No direction matching the provided facing: {facing}."),
        };
    }
}
