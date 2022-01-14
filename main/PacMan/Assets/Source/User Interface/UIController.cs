/*
 * Primary game-controller component for a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
 * 
 * This class describes the game board and the play area and provides an interface for external objects
 * to discern and mutate the nature and contents of its composite tiles.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

#define _DEVMODE_COLLISION_DEBUG

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Tilemaps;

namespace LongRoadGames.PacMan
{
    public class UIController : MonoBehaviour
    {
        private Text _txtCurrentTile;
        private Text _txtNextTile;

        public void Initialize()
        {
            _initialize_tile_debug_controls();
        }

        public void SetLevel(int level)
        {

        }

        public void SetLives(int lives)
        {

        }

        public void SetScore(int score)
        {

        }

        public void SetHighScore(int highScore)
        {

        }

        private void _initialize_tile_debug_controls()
        {
            _txtCurrentTile = transform.Find("Canvas/pnlDev/pnlCurrentTile/Text").gameObject.GetComponent<Text>();
            _txtNextTile = transform.Find("Canvas/pnlDev/pnlNextTile/Text").gameObject.GetComponent<Text>();

#if !_DEVMODE_COLLISION_DEBUG
            _txtCurrentTile.gameObject.SetActive(false);
            _txtNextTile.gameObject.SetActive(false);
#endif
        }

#if _DEVMODE_COLLISION_DEBUG

        public void DebugTileStates(TileState currentTile, TileState nextTile)
        {
            _txtCurrentTile.text = $"CURRENT TILE: {currentTile}";
            _txtNextTile.text = $"NEXT TILE: {nextTile}";
        }

#endif
    }
}