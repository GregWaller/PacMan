using System;
using System.Collections.Generic;

namespace LongRoadGames.PacMan
{
    public static class UtilityExtensions
    {
        private readonly static Random _rng = new Random();

        #region Direction Enumeration Manipulation

        public static Direction Left(this Direction direction)
        {
            if (direction - 1 == Direction.None)
                return Direction.Left;  // cycle to left from up

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

        #endregion

        #region Generic Collections

        public static T Random<T>(this List<T> list)
        {
            return list[_rng.Next(list.Count)];
        }

        #endregion
    }
}