﻿/*
 * A set of utility methods and extensions for use in a Pac-Man facsimile.
 * 
 * This class describes the game board and the play area and provides an interface for external objects
 * to discern and mutate the nature and contents of its composite tiles.
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

namespace LongRoadGames.PacMan
{
    public static class UtilityExtensions
    {
        public static Direction Left(this Direction direction)
        {
            if (direction - 1 == Direction.None)
                return Direction.Left;  // cycle back to left from up

            return direction - 1;
        }

        public static Direction Right(this Direction direction)
        {
            if (direction + 1 > Direction.Left)
                return Direction.Up; // cycle to up from left

            return direction + 1;
        }

        public static Direction Flip(this Direction direction)
        {
            if (direction + 2 > Direction.Left)
                return direction - 2;

            return direction + 2;
        }
    }
}