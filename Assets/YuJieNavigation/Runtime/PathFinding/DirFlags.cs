using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation
{
    public static class DirFlags
    {
        public static int ToArrayIndex(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.UpLeft:
                    return 0;
                case EDirFlags.Up:
                    return 1;
                case EDirFlags.UpRight:
                    return 2;
                case EDirFlags.Right:
                    return 3;
                case EDirFlags.DownRight:
                    return 4;
                case EDirFlags.Down:
                    return 5;
                case EDirFlags.DownLeft:
                    return 6;
                case EDirFlags.Left:
                    return 7;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// 直线
        /// </summary>

        public static bool IsStraight(EDirFlags dir)
        {
            return (dir & (EDirFlags.Up | EDirFlags.Down | EDirFlags.Left | EDirFlags.Right)) != EDirFlags.NONE;
        }

        /// <summary>
        /// 斜线
        /// </summary>
        public static bool IsDiagonal(EDirFlags dir)
        {
            return (dir & (EDirFlags.UpLeft | EDirFlags.UpRight | EDirFlags.DownLeft | EDirFlags.DownRight)) != EDirFlags.NONE;
        }

        public static Int2 ToPos(EDirFlags dir)
        {
            return DirToPos[dir];
        }

        private static readonly Dictionary<EDirFlags,Int2> DirToPos = new Dictionary<EDirFlags, Int2>() 
        {
            { EDirFlags.Up,new Int2(0, 1) },
            { EDirFlags.Down,new Int2(0, -1) },
            { EDirFlags.Right,new Int2(1, 0) },
            { EDirFlags.Left,new Int2(-1, 0) },
            { EDirFlags.UpLeft,new Int2(-1, 1) },
            { EDirFlags.UpRight,new Int2(1, 1) },
            { EDirFlags.DownLeft,new Int2(-1, -1) },
            { EDirFlags.DownRight,new Int2(1, -1) },
        };
    }


    public enum EDirFlags : int
    {
        NONE = 0,
        /// <summary>
        /// 上
        /// </summary>
        Up = 1 << 0,
        /// <summary>
        /// 下
        /// </summary>
        Down = 1 << 1,
        /// <summary>
        /// 右
        /// </summary>
        Right = 1 << 2,
        /// <summary>
        /// 左
        /// </summary>
        Left = 1 << 3,
        /// <summary>
        /// 右上
        /// </summary>
        UpRight = 1 << 4,
        /// <summary>
        /// 左上
        /// </summary>
        UpLeft = 1 << 5,
        /// <summary>
        /// 右下
        /// </summary>
        DownRight = 1 << 6,
        /// <summary>
        /// 左下
        /// </summary>
        DownLeft = 1 << 7,
        ALL = Up | Down | Right | Left | UpRight | UpLeft | DownRight | DownLeft,
    }
}
