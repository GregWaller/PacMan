using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class Clyde : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(16.0f, 16.5f, 0.0f);
        protected override Vector3Int _SCATTER_TARGET => new Vector3Int(0, -1, 0);
        protected override Direction _HOME_FACING => Direction.Up;
        protected override Direction _INITIAL_FACING => Direction.Left;
        protected override float _INITIAL_SPAWN_TIMER => 14.0f;

        protected override Vector3Int _chase()
        {
            Vector3Int pacManCell = _board.PacMan.CurrentTile.CellPosition;
            float cellDistanceToPacMan = Vector3Int.Distance(CurrentTile.CellPosition, pacManCell);

            if (cellDistanceToPacMan > 8)
                return pacManCell;
            else
                return _SCATTER_TARGET;
        }
    }
}