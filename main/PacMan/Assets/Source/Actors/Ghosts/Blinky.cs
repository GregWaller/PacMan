using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class Blinky : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(14.0f, 19.5f, 0.0f);
        protected override Vector3Int _SCATTER_TARGET => new Vector3Int(25, 34, 0);
        protected override Direction _HOME_FACING => Direction.Left;
        protected override Direction _INITIAL_FACING => Direction.Right;
        protected override float _INITIAL_SPAWN_TIMER => 0.0f;

        // Blinky can override the level strategy if there are only a few dots left on the map 
        // Level            0       1       2-4     5-7     8-10    11-13   14-17   18+
        // Dots Remaining   20      30      40      50      60      80      100     120

        public override void Begin()
        {
            _spawnTimer = _INITIAL_SPAWN_TIMER;
            _spawnStage = 0;
            _spawning = false;
            _awaitingSpawn = false;
            _respawning = false;

            base.Begin();
        }

        protected override Strategy _select_strategy()
        {
            if (_currentStrategy == Strategy.Eaten || _currentStrategy == Strategy.Frightened)
                return _currentStrategy;

            int level = _board.CurrentLevel;
            int dotsRemaining = _board.DotsRemaining;

            if ( (level == 0 && dotsRemaining <= 20) ||
                 (level == 1 && dotsRemaining <= 30) ||
                 (level < 5 && dotsRemaining <= 40) ||
                 (level < 8 && dotsRemaining <= 50) ||
                 (level < 11 && dotsRemaining <= 60) ||
                 (level < 14 && dotsRemaining <= 80) ||
                 (level < 18 && dotsRemaining <= 100) ||
                 (level >= 18 && dotsRemaining <= 120))
                return Strategy.Chase;
            else
                return base._select_strategy();
        }

        protected override Vector3Int _chase()
        {
            return _board.PacMan.CurrentTile.CellPosition;
        }
    }
}