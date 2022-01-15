/*
 * Enemy behaviour for Pinky in a Pac-Man facsimile.
 * 
 * Author: Greg Waller
 * Date: 01.13.2022
 */

#define _DEV

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

namespace LongRoadGames.PacMan
{
    public class Pinky : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(14.0f, 16.5f, 0.0f);
        protected override Direction _INITIAL_FACING => Direction.Down;

        protected override Vector3Int _chase()
        {
            return Vector3Int.zero;
        }

        protected override Vector3Int _scatter()
        {
            return Vector3Int.zero;
        }
    }
}