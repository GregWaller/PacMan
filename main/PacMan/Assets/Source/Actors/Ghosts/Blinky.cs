/*
 * Enemy behaviour for Blinky in a Pac-Man facsimile.
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
    public class Blinky : Ghost
    {
        protected override Vector3 _INITIAL_POSITION => new Vector3(14.0f, 19.5f, 0.0f);
        protected override Direction _INITIAL_FACING => Direction.Left;
    }
}