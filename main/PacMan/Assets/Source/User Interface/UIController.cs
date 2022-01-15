#define _DEV

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongRoadGames.PacMan
{
    public class UIController : MonoBehaviour
    {
        private Text _txtDevCurrentLevel;
        private Text _txtDevCurrentTile;
        private Text _txtDevNextTile;
        private Text _txtDevDotCounter;
        private Text _txtDevPowerPhase;
        private Text _txtDevCurrentPowerPhase;

        private Text _txtCurrentScore;
        private Text _txtHighScore;
        private Text _txtReady;

        private const int _ICON_COUNT = 7;
        private readonly List<TileState> _levelProgression = new List<TileState>
        {
            TileState.Cherry,
            TileState.Strawberry,
            TileState.Orange,
            TileState.Orange,
            TileState.Apple,
            TileState.Apple,
            TileState.Melon,
            TileState.Melon,
            TileState.Galaxian,
            TileState.Galaxian,
            TileState.Bell,
            TileState.Bell,
            TileState.Key,
            TileState.Key,
            TileState.Key,
            TileState.Key,
            TileState.Key,
            TileState.Key,
            TileState.Key,
        };
        private Dictionary<TileState, Sprite> _fruitMap;
        private List<Image> _levelIcons;

        public void Initialize()
        {
            _initialize_dev_controls();

            _txtCurrentScore = transform.Find("Canvas/pnlScore/pnlCurrentScore/Text").gameObject.GetComponent<Text>();
            _txtHighScore = transform.Find("Canvas/pnlScore/pnlHighScore/Text").gameObject.GetComponent<Text>();
            _txtReady = transform.Find("Canvas/pnlReady/Text").gameObject.GetComponent<Text>();
            _txtReady.gameObject.SetActive(false);

            Sprite[] fruitSprites = Resources.LoadAll<Sprite>("Sprites/level_icons");
            _fruitMap = new Dictionary<TileState, Sprite>
            {
                { TileState.Cherry, fruitSprites[0] },
                { TileState.Strawberry, fruitSprites[1] },
                { TileState.Orange, fruitSprites[2] },
                { TileState.Apple, fruitSprites[3] },
                { TileState.Melon, fruitSprites[4] },
                { TileState.Galaxian, fruitSprites[5] },
                { TileState.Bell, fruitSprites[6] },
                { TileState.Key, fruitSprites[7] },
            };

            _levelIcons = new List<Image>();
            for(int i = 0; i < _ICON_COUNT; i++)
                _levelIcons.Add(transform.Find("Canvas/pnlLevelIndicator/Icon_" + i).gameObject.GetComponent<Image>());
        }

        public void SetLevel(int level)
        {
            if (level < _ICON_COUNT)
            {
                int iconIDX = 0;
                for(; iconIDX <= level; iconIDX++)
                {
                    TileState icon = _levelProgression[iconIDX];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }

                for(; iconIDX < _ICON_COUNT; iconIDX++)
                    _levelIcons[iconIDX].gameObject.SetActive(false);
            }

            else if (level < _levelProgression.Count)
            {
                // case B: 7 < Level < 19
                // Range = [Level - 6, Level]
                for(int iconIDX = 0; iconIDX < _ICON_COUNT; iconIDX++)
                {
                    TileState icon = _levelProgression[iconIDX + (level - (_ICON_COUNT - 1))];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }
            }

            else
            {
                // case C: 18 < Level
                // Range = [12, 18]
                int levelIDX = 12;
                for (int iconIDX = 0; iconIDX < _ICON_COUNT; iconIDX++)
                {
                    TileState icon = _levelProgression[levelIDX++];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }
            }
        }

        public void SetLives(int lives)
        {

        }

        public void SetScore(int score)
        {
            _txtCurrentScore.text = score.ToString();
        }

        public void SetHighScore(int highScore)
        {
            _txtHighScore.text = highScore.ToString();
        }

        public void ShowReady(bool show)
        {
            _txtReady.gameObject.SetActive(show);
        }

        private void _initialize_dev_controls()
        {
            _txtDevCurrentLevel = transform.Find("Canvas/pnlDev/pnlCurrentLevel/Text").gameObject.GetComponent<Text>();
            _txtDevCurrentTile = transform.Find("Canvas/pnlDev/pnlCurrentTile/Text").gameObject.GetComponent<Text>();
            _txtDevNextTile = transform.Find("Canvas/pnlDev/pnlNextTile/Text").gameObject.GetComponent<Text>();
            _txtDevDotCounter = transform.Find("Canvas/pnlDev/pnlDotCounter/Text").gameObject.GetComponent<Text>();
            _txtDevPowerPhase = transform.Find("Canvas/pnlDev/pnlPowerPhase/Text").gameObject.GetComponent<Text>();
            _txtDevCurrentPowerPhase = transform.Find("Canvas/pnlDev/pnlCurrentPowerPhase/Text").gameObject.GetComponent<Text>();

#if !_DEV
            _txtDevCurrentLevel.gameObject.SetActive(false);
            _txtCurrentTile.gameObject.SetActive(false);
            _txtNextTile.gameObject.SetActive(false);
            _txtDotCounter.gameObject.SetActive(false);
            _txtDevPowerPhase.gameObject.SetActive(false);
            _txtDevCurrentPowerPhase.gameObject.SetActive(false);
#endif
        }

#if _DEV

        public void DebugLevel(int currentLevel)
        {
            _txtDevCurrentLevel.text = $"CURRENT LEVEL: {currentLevel}";
        }

        public void DebugTileStates(TileState currentTile, TileState nextTile)
        {
            _txtDevCurrentTile.text = $"CURRENT TILE: {currentTile}";
            _txtDevNextTile.text = $"NEXT TILE: {nextTile}";
        }

        public void DebugDotCounter(int dotCounter)
        {
            _txtDevDotCounter.text = $"DOT COUNTER: {dotCounter}";
        }

        public void DebugPowerPhase(bool powerPhase, int currentPhase = 0)
        {
            string yn = powerPhase ? "YES" : "NO";
            _txtDevPowerPhase.text = $"POWER PHASE: {yn}";

            if (powerPhase && currentPhase > 0)
                _txtDevCurrentPowerPhase.text = $"CURRENT PHASE: {currentPhase}";
            else
                _txtDevCurrentPowerPhase.text = $"CURRENT PHASE: -";
        }

#endif
    }
}