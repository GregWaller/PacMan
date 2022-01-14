/*
 * Tile abstraction for a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
 * 
 * This class describes tiles on the gameboard, along with their initial and current states.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

using UnityEngine;
using UnityEngine.Tilemaps;

namespace LongRoadGames.PacMan
{
    public enum TileState
    {
        Empty = 0,
        Wall = 1,
        Dot = 10,
        PDot = 50,
        Cherry = 100,
        Strawberry = 300,
        Orange = 500,
        Apple = 700,
        Melon = 1000,
        Galaxian = 2000,
        Bell = 3000,
        Key = 5000,
    }

    public class GameTile
    {
        public TileState CurrentState { get; private set; }
        public Tile Tile { get; private set; }
        public Vector3Int CellPosition { get; private set; }
        public Vector3 Position => _board.Tilemap.CellToWorld(CellPosition) + _board.Tilemap.tileAnchor; // offset to a center-point anchor on the tile.

        protected TileState _originalState;
        protected Gameboard _board;
        
        public GameTile(Gameboard board, Vector3Int position, TileState state, Tile tile)
        {
            _board = board;
            
            CellPosition = position;
            _originalState = CurrentState = state;
            Tile = tile;
        }

        public void SetState(TileState state)
        {
            CurrentState = state;
            _board.SetTile(CellPosition, state);
        }

        public void Reset()
        {
            SetState(_originalState);
        }
    }
}