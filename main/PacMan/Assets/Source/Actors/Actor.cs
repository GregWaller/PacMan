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
    public abstract class Actor : MonoBehaviour
    {
        protected abstract float _speed { get; }

        protected Animator _animator;
        protected Gameboard _board;
        protected Vector3 _direction = Vector3.zero;

        public virtual void Initialize(Gameboard board)
        {
            _board = board;
            _animator = GetComponent<Animator>();
        }

        public void Warp(GameTile tile, Direction facing)
        {
            transform.SetPositionAndRotation(tile.Position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            _face(facing);
        }

        protected void _face(Direction facing)
        {
            _animator.SetTrigger(facing.ToString());
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
