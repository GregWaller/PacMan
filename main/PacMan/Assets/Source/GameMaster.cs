/*
 * Primary game-controller component for Pac-Man facsimile.
 * Provided to Nvizzio Creations for purposes of skill assessment 
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LongRoadGames.PacMan
{
    public class GameMaster : MonoBehaviour
    {
        private const int _BOARD_WIDTH = 28;
        private const int _BOARD_HEIGHT = 31;

        public Tilemap Gameboard;

        private Dictionary<Vector3Int, GameTile> _playArea;

        public Tile DotTile { get; private set; }
        public Tile PDotTile { get; private set; }
        public Tile EmptyTile { get; private set; }

        public void Start()
        {
            Debug.Assert(Gameboard != null, "CRITICAL ERROR: A tilemap must be provided.");

            _tile_init();
            _board_init();
            _board_setup();
            //_spawn_player();
        }

        private void _tile_init()
        {
            DotTile = Instantiate(Resources.Load<Tile>("Sprites/Dot"));
            PDotTile = Instantiate(Resources.Load<Tile>("Sprites/PDot"));
            EmptyTile = Instantiate(Resources.Load<Tile>("Sprites/Empty"));
        }

        private void _board_init()
        {
            _playArea = new Dictionary<Vector3Int, GameTile>();

            for(int y = 0; y < _BOARD_HEIGHT; y++)
            {
                for(int x = 0; x < _BOARD_WIDTH; x++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    Tile tile = Gameboard.GetTile<Tile>(pos);

                    if (tile == null)
                    {
                        Gameboard.SetTile(pos, EmptyTile);
                    }
                    else if (tile.name.Contains("wall"))
                    {
                        _playArea.Add(pos, new Wall_GameTile(this, pos, tile));
                    }
                    else
                    {
                        TileState initialState = (TileState)Enum.Parse(typeof(TileState), tile.name);
                        GameTile gameTile = new GameTile(this, pos, initialState, tile);
                        _playArea.Add(pos, gameTile);
                    }
                }
            }
        }

        private void _board_setup()
        {
            foreach(GameTile gameTile in _playArea.Values.Where(t => t.CurrentState != TileState.Wall))
                gameTile.Reset();
        }

        private Tile _stateMap(TileState state) => state switch
        {
            TileState.Empty => EmptyTile,
            TileState.Dot => DotTile,
            TileState.PDot => PDotTile,
            _ => throw new ArgumentOutOfRangeException($"CRITICAL ERROR: No tile was found for the given state: {state}."),
        };

        public void SetTile(Vector3Int position, TileState state)
        {
            Gameboard.SetTile(position, _stateMap(state));
        }

        private void _spawn_player()
        {

        }
    }
}
