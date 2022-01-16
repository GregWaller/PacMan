using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class Pinky : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(14.0f, 16.5f, 0.0f);
        protected override Vector3Int _SCATTER_TARGET => new Vector3Int(2, 34, 0);
        protected override Direction _HOME_FACING => Direction.Down;
        protected override Direction _INITIAL_FACING => Direction.Left;
        protected override float _INITIAL_SPAWN_TIMER => 1.0f;

        protected override Vector3Int _chase()
        {
            Vector3 pacManPosition = _board.PacMan.CurrentTile.Position;
            Direction pacManFacing = _board.PacMan.Facing;
            Vector3 targetPosition;

            if (pacManFacing == Direction.Up)
            {
                // simulate the integer overflow from the original 8-bit version
                // see: https://youtu.be/ataGotQ7ir8?t=451 for more information

                targetPosition = pacManPosition + (_directionMap(Direction.Up) * 4);
                targetPosition += (_directionMap(Direction.Left) * 4);
            }
            else
            {
                targetPosition = pacManPosition + (_directionMap(pacManFacing) * 4);
            }

            return _board.Tilemap.WorldToCell(targetPosition);
        }
    }
}