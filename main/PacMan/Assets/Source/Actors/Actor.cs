using System;
using UnityEngine;

namespace LongRoadGames.PacMan
{
    public abstract class Actor : MonoBehaviour
    {
        protected abstract Vector3 _INITIAL_POSITION { get; }
        protected abstract Direction _INITIAL_FACING { get; }

        protected Animator _animator;
        protected Gameboard _board;

        public GameTile CurrentTile => _board.GetTile(transform.position);
        public Direction Facing { get; protected set; } = Direction.Right;
        protected GameTile _previousTile = null;
        protected float _speed = 0.0f;
        protected Vector3 _direction = Vector3.zero;

        public virtual void Update() { }

        public virtual void Initialize(Gameboard board)
        {
            _board = board;
            _animator = GetComponent<Animator>();
            
            Reboot();
        }

        public virtual void Reboot()
        {
            Facing = _INITIAL_FACING;
            _direction = Vector3.zero;
            Warp(_INITIAL_POSITION, _INITIAL_FACING);
        }

        public virtual void Begin() 
        {
            _direction = _directionMap(Facing);
            _previousTile = CurrentTile;
        }

        #region Warping

        public void Warp(Vector3 position, Direction facing)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            _face(facing);
        }

        protected void _check_warp(GameTile currentTile)
        {
            // don't warp if we just warped
            if (_previousTile.CurrentState == TileState.LeftWarp ||
                _previousTile.CurrentState == TileState.RightWarp)
                return;

            // don't warp if we're not on a warp tile
            if (currentTile.CurrentState != TileState.LeftWarp && 
                currentTile.CurrentState != TileState.RightWarp)
                return;
            
            Vector3 offset = new Vector3(0.5f, 0.0f, 0.0f);
            if (currentTile.CurrentState == TileState.LeftWarp)
                offset = -offset;

            float distance = Vector3.Distance(transform.position, currentTile.Position + offset);
            bool edgeCollision = distance <= GameTile.CELL_COLLISION_THRESHOLD;

            if (edgeCollision)
            {
                offset = -offset;  
                GameTile targetTile = currentTile.CurrentState == TileState.LeftWarp ? _board.RightWarp : _board.LeftWarp;
                Vector3 targetPosition = targetTile.Position + offset;
                Warp(targetPosition, Facing);

                // only warp if i am entering a warp tile (don't warp on exit)
            }
        }

        protected void _check_new_tile(GameTile currentTile)
        {
            GameTile newTile = _board.GetTile(transform.position);
            if (newTile != currentTile)
                _previousTile = currentTile;
        }

        #endregion

        #region Facing and Directionality

        protected Vector3 _directionMap(Direction facing) => facing switch
        {
            Direction.Up => transform.up,
            Direction.Down => -transform.up,
            Direction.Left => -transform.right,
            Direction.Right => transform.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No direction matching the provided facing: {facing}."),
        };

        protected void _face(Direction facing)
        {
            _animator.SetTrigger(facing.ToString());
            Facing = facing;
        }

        #endregion
    }
}
