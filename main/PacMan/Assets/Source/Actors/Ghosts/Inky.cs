using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class Inky : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(12.0f, 16.5f, 0.0f);
        protected override Vector3Int _SCATTER_TARGET => new Vector3Int(27, -1, 0);
        protected override Direction _HOME_FACING => Direction.Up;
        protected override Direction _INITIAL_FACING => Direction.Right;
        protected override float _INITIAL_SPAWN_TIMER => 7.0f;

        protected override Vector3Int _chase()
        {
            Vector3 pacManPosition = _board.PacMan.CurrentTile.Position;
            Direction pacManFacing = _board.PacMan.Facing;
            Vector3 newPos;

            if (pacManFacing == Direction.Up)
            {
                // simulate the integer overflow from the original 8-bit version
                // see: https://youtu.be/ataGotQ7ir8?t=451 for more information

                newPos = pacManPosition + (_directionMap(Direction.Up) * 2);
                newPos += (_directionMap(Direction.Left) * 2);
            }
            else
            {
                newPos = pacManPosition + (_directionMap(pacManFacing) * 2);
            }

            Vector3 blinkyPosition = _board.Blinky.CurrentTile.Position;
            Vector3 targetToBlinky = blinkyPosition - newPos;
            newPos = Quaternion.AngleAxis(180.0f, new Vector3(0.0f, 0.0f, 1.0f)) * targetToBlinky;
            
            return _board.Tilemap.WorldToCell(newPos);
        }
    }
}