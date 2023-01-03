#define _DEV

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongRoadGames.PacMan
{
    public class UIController : MonoBehaviour
    {
        private Gameboard _board;

        // ----- Developer Controls
        private Text _txtDevCurrentLevel;
        private Text _txtDevCurrentTile;
        private Text _txtDevNextTile;
        private Text _txtDevDotCounter;
        private Text _txtDevPowerPhase;
        private Text _txtDevCurrentPowerPhase;

        // ----- Gameplay Readouts
        private Text _txtCurrentScore;          
        private Text _txtHighScore;
        private Text _txtReady;
        private Text _txtGameOver;

        // ----- Level Indicators
        private const int _LEVEL_ICON_COUNT = 7;
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

        // ----- Life Indicators
        private const int _LIFE_ICON_COUNT = 5;
        private List<Image> _lifeIcons;

        // ----- Bonus Point Display
        public BonusPointDisplay BonusPoints { get; private set; }

        public void Initialize(Gameboard gameboard)
        {
            _board = gameboard;

            _initialize_dev_controls();

            // ----- Gameplay Readouts
            _txtCurrentScore = transform.Find("Canvas/pnlScore/pnlCurrentScore/Text").gameObject.GetComponent<Text>();
            _txtHighScore = transform.Find("Canvas/pnlScore/pnlHighScore/Text").gameObject.GetComponent<Text>();
            _txtReady = transform.Find("Canvas/pnlReady/Text").gameObject.GetComponent<Text>();
            _txtReady.gameObject.SetActive(false);
            _txtGameOver = transform.Find("Canvas/pnlGameOver/Text").gameObject.GetComponent<Text>();
            _txtGameOver.gameObject.SetActive(false);

            // ----- Level Indicators
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
            for(int i = 0; i < _LEVEL_ICON_COUNT; i++)
                _levelIcons.Add(transform.Find("Canvas/pnlLevelIndicator/Icon_" + i).gameObject.GetComponent<Image>());

            // ----- Life Indicators
            _lifeIcons = new List<Image>();
            for(int i = 0; i < _LIFE_ICON_COUNT; i++)
                _lifeIcons.Add(transform.Find("Canvas/pnlLivesIndicator/Icon_" + i).gameObject.GetComponent<Image>());

            // ----- Bonus Point Display
            BonusPoints = transform.Find("Canvas/pnlBonusPoints").gameObject.AddComponent<BonusPointDisplay>();
            BonusPoints.Initialize(this, _board);
        }

        public void SetLevel(int level)
        {
            if (level < _LEVEL_ICON_COUNT)
            {
                int iconIDX = 0;
                for(; iconIDX <= level; iconIDX++)
                {
                    TileState icon = _levelProgression[iconIDX];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }

                for(; iconIDX < _LEVEL_ICON_COUNT; iconIDX++)
                    _levelIcons[iconIDX].gameObject.SetActive(false);
            }

            else if (level < _levelProgression.Count)
            {
                for(int iconIDX = 0; iconIDX < _LEVEL_ICON_COUNT; iconIDX++)
                {
                    TileState icon = _levelProgression[iconIDX + (level - (_LEVEL_ICON_COUNT - 1))];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }
            }

            else
            {
                int levelIDX = 12;
                for (int iconIDX = 0; iconIDX < _LEVEL_ICON_COUNT; iconIDX++)
                {
                    TileState icon = _levelProgression[levelIDX++];
                    _levelIcons[iconIDX].gameObject.SetActive(true);
                    _levelIcons[iconIDX].sprite = _fruitMap[icon];
                }
            }
        }

        public TileState LevelState(int level) => level switch
        {
            0 => TileState.Cherry,
            1 => TileState.Strawberry,
            2 => TileState.Orange,
            3 => TileState.Orange,
            4 => TileState.Apple,
            5 => TileState.Apple,
            6 => TileState.Melon,
            7 => TileState.Melon,
            8 => TileState.Galaxian,
            9 => TileState.Galaxian,
            10 => TileState.Bell,
            11 => TileState.Bell,
            _ => TileState.Key,
        };

        public Sprite GetLevelSprite(int level) 
        {
            TileState levelState = LevelState(level);
            return GetLevelSprite(levelState);
        }

        public Sprite GetLevelSprite(TileState state)
        {
            return _fruitMap[state];
        }

        public void SetLives(int lives)
        {
            int iconIDX = 0;
            for(; iconIDX < lives && iconIDX < _LIFE_ICON_COUNT; iconIDX++)
                _lifeIcons[iconIDX].gameObject.SetActive(true);

            for (; iconIDX < _LIFE_ICON_COUNT; iconIDX++)
                _lifeIcons[iconIDX].gameObject.SetActive(false);
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

        public void ShowGameOver(bool show)
        {
            _txtGameOver.gameObject.SetActive(show);
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

        public void DebugDotCounter(int dotCounter, int maxDots)
        {
            _txtDevDotCounter.text = $"DOTS: {dotCounter}/{maxDots}";
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