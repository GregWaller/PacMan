/*
 * Enemy behaviour for Clyde in a Pac-Man facsimile.
 * Provided to Nvizzio Creations for skill assessment.
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
    public class Clyde : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(16.0f, 16.5f, 0.0f);
        protected override Direction _INITIAL_FACING => Direction.Up;
    }
}