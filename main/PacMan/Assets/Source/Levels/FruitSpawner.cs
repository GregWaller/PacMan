using UnityEngine;

namespace LongRoadGames.PacMan
{
    public class FruitSpawner : MonoBehaviour
    {
        public int Value => (int)_levelIcon;
        public bool Spawned { get; private set; }

        private Gameboard _board;
        private SpriteRenderer _spriteRenderer;
        private const float _DEFAULT_DURATION = 9.0f;
        private float _duration = 0.0f;
        private TileState _levelIcon;
        private GameTile _tile;

        public void Update()
        {
            if (Spawned && !_board.Paused)
            {
                _duration -= Time.deltaTime;
                if (_duration <= 0.0f)
                    Despawn();

                float distance = Vector3.Distance(transform.position, _board.PacMan.transform.position);
                if (distance <= PacMan.COLLISION_THRESHOLD)
                {
                    _board.AddPoints(Value);
                    _board.GUI.BonusPoints.Display(Value, _tile.Position);
                    AudioController.Instance.Play(AudioClipID.EatFruit);
                    Despawn();
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
            _levelIcon = _board.GUI.LevelState(level);
            _spriteRenderer.sprite = _board.GUI.GetLevelSprite(_levelIcon);

            gameObject.SetActive(true);

            float randomPad = Random.Range(0.0f, 1.0f);
            _duration = _DEFAULT_DURATION + randomPad;
            Spawned = true;
        }

        public void Despawn()
        {
            gameObject.SetActive(false);
            Spawned = false;
        }
    }
}