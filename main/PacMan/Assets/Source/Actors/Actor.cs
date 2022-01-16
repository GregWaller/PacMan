using System;
using UnityEngine;

namespace LongRoadGames.PacMan
{
    public abstract class Actor : MonoBehaviour
    {
        protected Animator _animator;
        protected Gameboard _board;

        protected abstract Vector3 _INITIAL_POSITION { get; }
        protected abstract Direction _INITIAL_FACING { get; }
        public GameTile CurrentTile => _board.GetTile(transform.position);
        public Direction Facing { get; protected set; } = Direction.Right;
        protected GameTile _previousTile = null;
        protected float _speed = 0.0f;
        protected Vector3 _direction = Vector3.zero;

        #region Initialization and Resets

        public virtual void Initialize(Gameboard board)
        {
            _board = board;
            _animator = GetComponent<Animator>();
        }

        public virtual void ResetPosition()
        {
            _direction = Vector3.zero;
            _previousTile = null;
            Facing = _INITIAL_FACING;
            Warp(_INITIAL_POSITION, Facing);
        }

        public virtual void Begin() 
        {
            _direction = _directionMap(Facing);
            _previousTile = CurrentTile;
        }

        #endregion

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
            }
        }

        #endregion

        #region Facing, Directionality, and Positional Awareness

        protected Vector3 _directionMap(Direction facing) => facing switch
        {
            Direction.Up => transform.up,
            Direction.Down => -transform.up,
            Direction.Left => -transform.right,
            Direction.Right => transform.right,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No direction matching the provided facing: {facing}."),
        };

        protected void _update_gameplay_position()
        {
            transform.Translate(_direction * (Time.deltaTime * _speed));

            Vector3 currentPos = transform.position;
            switch (Facing)
            {
                case Direction.Left:
                case Direction.Right:

                    int currentY = (int)currentPos.y;
                    transform.position = new Vector3(currentPos.x, currentY + 0.5f, currentPos.z);
                    break;

                case Direction.Up:
                case Direction.Down:

                    int currentX = (int)currentPos.x;
                    transform.position = new Vector3(currentX + 0.5f, currentPos.y, currentPos.z);
                    break;
            }
        }

        protected virtual void _face(Direction facing)
        {
            _animator.SetTrigger(facing.ToString());
            Facing = facing;
        }

        protected virtual bool _check_new_tile(GameTile currentTile)
        {
            GameTile newTile = _board.GetTile(transform.position);

            if (newTile != currentTile)
            {
                _previousTile = currentTile;
                return true;
            }

            return false;
        }

        #endregion
    }
}
