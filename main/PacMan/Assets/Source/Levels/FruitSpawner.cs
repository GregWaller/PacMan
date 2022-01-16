using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class FruitSpawner : MonoBehaviour
    {
        public int Value => (int)_iconValue;

        private Gameboard _board;
        private SpriteRenderer _spriteRenderer;
        private const float _DEFAULT_DURATION = 9.0f;
        private float _duration = 0.0f;
        private bool _spawned = false;
        private TileState _iconValue;
        private GameTile _tile;

        public void Update()
        {
            if (_spawned && !_board.Paused)
            {
                _duration -= Time.deltaTime;
                if (_duration <= 0.0f)
                    _despawn();

                // determine collisions with pacman
                float distance = Vector3.Distance(transform.position, _board.PacMan.transform.position);
                if (distance <= PacMan.COLLISION_THRESHOLD)
                {
                    _board.AddPoints(Value);
                    _board.GUI.BonusPoints.Display(Value, _tile.Position);
                    _despawn();
                }
            }
        }

        public void Initialize(Gameboard gameboard)
        {
            _board = gameboard;
            _tile = _board.GetTile(transform.position);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.SetActive(false);
        }

        public void Spawn(int level)
        {
            _iconValue = _board.GUI.LevelState(level);
            _spriteRenderer.sprite = _board.GUI.GetLevelSprite(_iconValue);

            gameObject.SetActive(true);

            float randomPad = Random.Range(0.0f, 1.0f);
            _duration = _DEFAULT_DURATION + randomPad;
            _spawned = true;
        }

        private void _despawn()
        {
            gameObject.SetActive(false);
            _spawned = false;
        }
    }
}