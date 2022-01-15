/*
 * Generalization of an actor.
 * Provided to Nvizzio Creations for skill assessment.
 * 
 * This class represents the abstraction of an actor during the course of play and provides
 * a generalized interface for those objects.
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
    // reference for ghost AI: 
    // https://www.youtube.com/watch?v=ataGotQ7ir8

    public abstract class Actor : MonoBehaviour
    {
        protected abstract Vector3 _INITIAL_POSITION { get; }
        protected abstract Direction _INITIAL_FACING { get; }

        protected float _speed = 0.0f;

        protected Animator _animator;
        protected Gameboard _board;
        protected Vector3 _direction = Vector3.zero;
        protected Direction _facing = Direction.Right;

        public virtual void Update() { }

        public virtual void Initialize(Gameboard board)
        {
            _board = board;
            _animator = GetComponent<Animator>();
            _speed = 3.0f;
            ResetPosition();
        }

        public void ResetPosition()
        {
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
