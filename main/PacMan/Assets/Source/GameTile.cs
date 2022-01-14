/*
 * Primary game-controller component for Pac-Man facsimile.
 * Provided to Nvizzio Creations for purposes of skill assessment 
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
        Empty,
        Wall,
        Dot,
        PDot,
    }

    public class GameTile
    {
        public TileState CurrentState { get; private set; }
        public Tile Tile { get; private set; }
        public Vector3Int Position { get; private set; }

        protected TileState _originalState;
        protected GameMaster _master;
        
        public GameTile(GameMaster master, Vector3Int position, TileState state, Tile tile)
        {
            _master = master;

            Position = position;
            _originalState = CurrentState = state;
            Tile = tile;
        }

        public void SetState(TileState state)
        {
            CurrentState = state;
            _master.SetTile(Position, state);
        }

        public void Reset()
        {
            SetState(_originalState);
        }
    }

    public class Wall_GameTile : GameTile
    {
        public Wall_GameTile(GameMaster master, Vector3Int position, Tile tile)
            : base(master, position, TileState.Wall, tile) { }
    }
}