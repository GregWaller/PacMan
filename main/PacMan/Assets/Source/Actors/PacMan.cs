#define _DEV

using UnityEngine;
using UnityEngine.InputSystem;

namespace LongRoadGames.PacMan
{
    [RequireComponent(typeof(PlayerInput))]
    public class PacMan : Actor
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(14.0f, 7.5f, 0.0f);
        protected override Direction _INITIAL_FACING => Direction.Right;

        private PlayerInput _playerInput;

        public int Lives { get; private set; } = _DEFAULT_LIFE_COUNT;
        private const int _DEFAULT_LIFE_COUNT = 3;

        public override void Update()
        {
            if (_board.LevelInProgress)
            {
                Direction input = Facing;
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

                GameTile currentTile = CurrentTile;
                bool atJunction = Vector3.Distance(transform.position, currentTile.Position) <= GameTile.CELL_COLLISION_THRESHOLD;

#if _DEV
                GameTile nextTile = _board.GetTileNeighbour(transform.position, Facing);
                _board.GUI.DebugTileStates(currentTile.CurrentState, nextTile.CurrentState);
#endif

                if (input == Direction.None)
                    return;

                // TODO: make the junction threshold a little more forgiving for face reversal

                if (Facing != input)
                {
                    bool inputBlocked = _board.DirectionBlocked(transform.position, input);
                    if (!inputBlocked && atJunction)
                    {
                        Facing = input;
                        _face(input);
                        _direction = _directionMap(input);
                    }
                }

                bool pathBlocked = _board.DirectionBlocked(transform.position, Facing);
                if (pathBlocked && atJunction)
                    _direction = Vector3.zero;

                if (currentTile.CurrentState == TileState.Dot || currentTile.CurrentState == TileState.PDot)
                {
                    if (!_board.ConsumeDot(currentTile))
                        return;
                }

                _check_warp(currentTile);

                if (_direction != Vector3.zero)
                    _update_gameplay_position();

                _check_new_tile(currentTile);
            }
        }

        public override void Initialize(Gameboard gameboard)
        {
            base.Initialize(gameboard);

            // TODO: tweak PacMan's speed to make it feel a little smoother
            _speed = 5.0f;
            _playerInput = GetComponent<PlayerInput>();
        }

        public override void Begin()
        {
            Facing = Direction.Right;
            base.Begin();
        }

        #region Lives

        public void ResetLives()
        {
            Lives = _DEFAULT_LIFE_COUNT;
        }

        public void BonusLife()
        {
            Lives++;
            _board.GUI.SetLives(Lives);
        }

        public void Capture()
        {
            Lives--;

            if (Lives > 0)
                _board.ResetLevel(false);
            else
                _board.ResetGame();
        }

        #endregion
    }
}