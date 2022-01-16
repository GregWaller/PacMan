using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongRoadGames.PacMan
{
    public class BonusPointDisplay : MonoBehaviour
    {
        public bool Active { get; private set; }

        private const float _DEFAULT_DISPLAY_DURATION = 2.0f;

        private UIController _parent;
        private Gameboard _board;
        private RectTransform _rect;
        private Text _txtBonusPoints;
        private float _duration = 0.0f;

        public void Update()
        {
            if (Active && !_board.Paused)
            {
                _duration -= Time.deltaTime;
                if (_duration <= 0.0f)
                {
                    SetActive(false);
                    _duration = 0.0f;
                }
            }
        }

        public void Initialize(UIController parent, Gameboard gameboard)
        {
            _parent = parent;
            _board = gameboard;
            _rect = gameObject.GetComponent<RectTransform>();
            _txtBonusPoints = transform.Find("Text").gameObject.GetComponent<Text>();
            SetActive(false);
        }

        public void Display(int points, Vector3 pos)
        {
            _txtBonusPoints.text = points.ToString();
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(pos);

            // scale the screenPoint based on the current resolution of the player
            Vector2 anchor = new Vector2(screenPoint.x, screenPoint.y);
            float rw = (anchor.x * Screen.width) / 1920.0f;
            float rh = (anchor.y * Screen.height) / 1080.0f;
            Vector3 targetPos = new Vector3(rw, rh, 0.0f);

            _rect.position = targetPos;

            _duration = _DEFAULT_DISPLAY_DURATION;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            Active = active;
        }
    }
}
