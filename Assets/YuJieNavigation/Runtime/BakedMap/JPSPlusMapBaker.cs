using Codice.Client.BaseCommands.Merge.IncomingChanges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YuJie.Navigation
{
    public class JPSPlusMapBaker
    {
        public int Width { private set; get; }
        public int Height { private set; get; }

        public int[,] BlockLUT;
        public JPSPlusMapBakerBlock[] Blocks;

        public JPSPlusMapBaker() { }

        public JPSPlusMapBaker(bool[,] obs)
        {
            Init(obs);
        }

        public void Init(bool[,] obs)
        {
            Width = obs.GetLength(0);
            Height = obs.GetLength(1);
            BlockLUT = new int[Width, Height];

            List<JPSPlusMapBakerBlock> blocks = new List<JPSPlusMapBakerBlock>();
            int index = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (obs[x, y])
                    {//障碍
                        BlockLUT[x, y] = -1;
                    }
                    else
                    {
                        BlockLUT[x, y] = index;
                        blocks.Add(new JPSPlusMapBakerBlock(new Int2(x, y)));
                        index++;
                    }
                }
            }
            Blocks = blocks.ToArray();
        }

        public JPSPlusBakedMap Bake()
        {
            MarkJumpDirFlag();
            MarkStraight();
            MarkDiagonal();
            return new JPSPlusBakedMap(
                BlockLUT,
                Blocks.Select(b =>
                {
                    return new JPSPlusBakedMap.JPSPlusBakedMapBlock(b.Pos, b.JumpDistances);
                }).ToArray());
        }

        //标记跳点
        private void MarkJumpDirFlag()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Int2 p = new Int2(x, y);

                    if (IsWalkable(p))
                        continue;

                    for (int d = 0b10000000; d > 0b00001111; d >>= 1)
                    {
                        EDirFlags dir = (EDirFlags)d;
                        Int2 primaryP = p.Foward(dir);
                        JPSPlusMapBakerBlock primaryB = GetBlockOrNull(primaryP);
                        if (primaryB == null)
                            continue;

                        switch (dir)
                        {
                            case EDirFlags.UpRight:
                                {
                                    Int2 p1 = p.Foward(EDirFlags.Up);
                                    Int2 p2 = p.Foward(EDirFlags.Right);
                                    if (IsWalkable(p1) && IsWalkable(p2))
                                    {
                                        primaryB.JumpDirFlags |= EDirFlags.Down | EDirFlags.Left;
                                    }
                                    break;
                                }
                            case EDirFlags.DownLeft:
                                {
                                    Int2 p1 = p.Foward(EDirFlags.Down);
                                    Int2 p2 = p.Foward(EDirFlags.Left);
                                    if (IsWalkable(p1) && IsWalkable(p2))
                                    {
                                        primaryB.JumpDirFlags |= EDirFlags.Up | EDirFlags.Right;
                                    }
                                    break;
                                }
                            case EDirFlags.DownRight:
                                {
                                    Int2 p1 = p.Foward(EDirFlags.Down);
                                    Int2 p2 = p.Foward(EDirFlags.Right);
                                    if (IsWalkable(p1) && IsWalkable(p2))
                                    {
                                        primaryB.JumpDirFlags |= EDirFlags.Up | EDirFlags.Left;
                                    }
                                    break;
                                }
                            case EDirFlags.UpLeft:
                                {
                                    Int2 p1 = p.Foward(EDirFlags.Up);
                                    Int2 p2 = p.Foward(EDirFlags.Left);
                                    if (IsWalkable(p1) && IsWalkable(p2))
                                    {
                                        primaryB.JumpDirFlags |= EDirFlags.Down | EDirFlags.Right;
                                    }
                                    break;
                                }
                            default:
                                throw new ArgumentException();
                        }
                    }
                }
            }
        }

        //标记直线
        private void MarkStraight()
        {
            // . . .
            // L . .
            // . . .
            for (int y = 0; y < Height; y++)
            {//Left
                bool isJumpPointLastSeen = false;
                int distance = -1;
                for (int x = 0; x < Width; x++)
                {//从左往右遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                    {//障碍物或超出地图边界
                        distance = -1;
                        isJumpPointLastSeen = false;
                        continue;
                    }

                    distance++;
                    if (isJumpPointLastSeen)
                    {//直线到跳点距离
                        block.SetDistance(EDirFlags.Left, distance);
                    }
                    else
                    {//直线到障碍距离
                        block.SetDistance(EDirFlags.Left, -distance);
                    }

                    if (block.IsJumpable(EDirFlags.Right))
                    {//是当前方向上的跳点
                        distance = 0;
                        isJumpPointLastSeen = true;
                    }
                }
            }

            // . . .
            // . . R
            // . . .
            for (int y = 0; y < Height; y++)
            {//Right
                bool isJumpPointLastSeen = false;
                int distance = -1;
                for (int x = Width - 1; x >= 0; x--)
                {//从右往左遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                    {//障碍物或超出地图边界
                        distance = -1;
                        isJumpPointLastSeen = false;
                        continue;
                    }

                    distance++;
                    if (isJumpPointLastSeen)
                    {//直线到跳点距离
                        block.SetDistance(EDirFlags.Right, distance);
                    }
                    else
                    {//直线到障碍距离
                        block.SetDistance(EDirFlags.Right, -distance);
                    }
                    if (block.IsJumpable(EDirFlags.Left))
                    {
                        distance = 0;
                        isJumpPointLastSeen = true;
                    }
                }
            }

            // . . .
            // . . .
            // . D .
            for (int x = 0; x < Width; x++)
            {//Down
                bool isJumpPointLastSeen = false;
                int distance = -1;
                for (int y = 0; y < Height; y++)
                {//从下往上遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                    {//障碍物或超出地图边界
                        distance = -1;
                        isJumpPointLastSeen = false;
                        continue;
                    }

                    distance++;
                    if (isJumpPointLastSeen)
                    {//直线到跳点距离
                        block.SetDistance(EDirFlags.Down, distance);
                    }
                    else
                    {//直线到障碍距离
                        block.SetDistance(EDirFlags.Down, -distance);
                    }
                    if (block.IsJumpable(EDirFlags.Up))
                    {
                        distance = 0;
                        isJumpPointLastSeen = true;
                    }
                }
            }

            // . U .
            // . . .
            // . . .
            for (int x = 0; x < Width; x++)
            {//Up
                bool isJumpPointLastSeen = false;
                int distance = -1;
                for (int y = Height - 1; y >= 0; y--)
                {//从上往下遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                    {//障碍物或超出地图边界
                        distance = -1;
                        isJumpPointLastSeen = false;
                        continue;
                    }

                    distance++;
                    if (isJumpPointLastSeen)
                    {//直线到跳点距离
                        block.SetDistance(EDirFlags.Up, distance);
                    }
                    else
                    {//直线到障碍距离
                        block.SetDistance(EDirFlags.Up, -distance);
                    }
                    if (block.IsJumpable(EDirFlags.Down))
                    {
                        distance = 0;
                        isJumpPointLastSeen = true;
                    }
                }
            }
        }

        //标记斜线
        private void MarkDiagonal()
        {
            // * U .
            // L . .
            // . . .
            for (int y = Height - 1; y >= 0; y--)
            {//Up & Left
                for (int x = 0; x < Width; x++)
                {//从左上往右下遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                        continue;

                    if(x == 0 || y == Height - 1)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.UpLeft, 0);
                        continue;
                    }

                    Int2 p1 = p.Foward(EDirFlags.Up);
                    Int2 p2 = p.Foward(EDirFlags.UpLeft);
                    Int2 p3 = p.Foward(EDirFlags.Left);
                    bool p1Walkable = IsWalkable(p1);
                    bool p2Walkable = IsWalkable(p2);
                    bool p3Walkable = IsWalkable(p3);

                    if (!p1Walkable || !p2Walkable || !p3Walkable)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.UpLeft, 0); 
                        continue;
                    }

                    //上一个block
                    JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                    if(prevBlock.GetDistance(EDirFlags.Up) > 0 || prevBlock.GetDistance(EDirFlags.Left) > 0)
                    {//初始斜方向距离
                        block.SetDistance(EDirFlags.UpLeft, 1);
                        continue;
                    }

                    int distanceFromPrev = prevBlock.GetDistance(EDirFlags.UpLeft);
                    if(distanceFromPrev > 0)
                    {//斜方向到跳点距离
                        block.SetDistance(EDirFlags.UpLeft, distanceFromPrev + 1);
                    }
                    else
                    {//斜方向到障碍距离
                        block.SetDistance(EDirFlags.UpLeft, distanceFromPrev - 1);
                    }
                }
            }

            // . U *
            // . . R
            // . . .
            for (int y = Height - 1; y >= 0; y--)
            {//Up & Right
                for (int x = Width - 1; x >= 0; x--)
                {//从右上往左下遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                        continue;

                    if (x == Width - 1 || y == Height - 1)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.UpRight, 0);
                        continue;
                    }

                    Int2 p1 = p.Foward(EDirFlags.Up);
                    Int2 p2 = p.Foward(EDirFlags.UpRight);
                    Int2 p3 = p.Foward(EDirFlags.Right);
                    bool p1Walkable = IsWalkable(p1);
                    bool p2Walkable = IsWalkable(p2);
                    bool p3Walkable = IsWalkable(p3);

                    if (!p1Walkable || !p2Walkable || !p3Walkable)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.UpRight, 0);
                        continue;
                    }

                    //上一个block
                    JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                    if (prevBlock.GetDistance(EDirFlags.Up) > 0 || prevBlock.GetDistance(EDirFlags.Right) > 0)
                    {//初始斜方向距离
                        block.SetDistance(EDirFlags.UpRight, 1);
                        continue;
                    }

                    int distanceFromPrev = prevBlock.GetDistance(EDirFlags.UpRight);
                    if (distanceFromPrev > 0)
                    {//斜方向到跳点距离
                        block.SetDistance(EDirFlags.UpRight, distanceFromPrev + 1);
                    }
                    else
                    {//斜方向到障碍距离
                        block.SetDistance(EDirFlags.UpRight, distanceFromPrev - 1);
                    }
                }
            }

            // . . .
            // L . .
            // * D .
            for (int y = 0; y < Height; y++)
            {//Left & Down
                for (int x = 0; x < Width; x++)
                {//从左下往右上遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                        continue;

                    if (x == 0 || y == 0)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.DownLeft, 0);
                        continue;
                    }

                    Int2 p1 = p.Foward(EDirFlags.Down);
                    Int2 p2 = p.Foward(EDirFlags.DownLeft);
                    Int2 p3 = p.Foward(EDirFlags.Left);
                    bool p1Walkable = IsWalkable(p1);
                    bool p2Walkable = IsWalkable(p2);
                    bool p3Walkable = IsWalkable(p3);

                    if (!p1Walkable || !p2Walkable || !p3Walkable)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.DownLeft, 0);
                        continue;
                    }

                    //上一个block
                    JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                    if (prevBlock.GetDistance(EDirFlags.Down) > 0 || prevBlock.GetDistance(EDirFlags.Left) > 0)
                    {//初始斜方向距离
                        block.SetDistance(EDirFlags.DownLeft, 1);
                        continue;
                    }

                    int distanceFromPrev = prevBlock.GetDistance(EDirFlags.DownLeft);
                    if (distanceFromPrev > 0)
                    {//斜方向到跳点距离
                        block.SetDistance(EDirFlags.DownLeft, distanceFromPrev + 1);
                    }
                    else
                    {//斜方向到障碍距离
                        block.SetDistance(EDirFlags.DownLeft, distanceFromPrev - 1);
                    }
                }
            }

            // . . .
            // . . R
            // . D *
            for (int y = 0; y < Height; y++)
            {//Right & Down
                for (int x = Width - 1; x >= 0; x--)
                {//从右下往左上遍历
                    Int2 p = new Int2(x, y);
                    JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                    if (block == null)
                        continue;

                    if (x == Width - 1 || y == 0)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.DownRight, 0);
                        continue;
                    }

                    Int2 p1 = p.Foward(EDirFlags.Down);
                    Int2 p2 = p.Foward(EDirFlags.DownRight);
                    Int2 p3 = p.Foward(EDirFlags.Right);
                    bool p1Walkable = IsWalkable(p1);
                    bool p2Walkable = IsWalkable(p2);
                    bool p3Walkable = IsWalkable(p3);

                    if (!p1Walkable || !p2Walkable || !p3Walkable)
                    {//斜方向到障碍的距离
                        block.SetDistance(EDirFlags.DownRight, 0);
                        continue;
                    }

                    //上一个block
                    JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                    if (prevBlock.GetDistance(EDirFlags.Down) > 0 || prevBlock.GetDistance(EDirFlags.Right) > 0)
                    {//初始斜方向距离
                        block.SetDistance(EDirFlags.DownRight, 1);
                        continue;
                    }

                    int distanceFromPrev = prevBlock.GetDistance(EDirFlags.DownRight);
                    if (distanceFromPrev > 0)
                    {//斜方向到跳点距离
                        block.SetDistance(EDirFlags.DownRight, distanceFromPrev + 1);
                    }
                    else
                    {//斜方向到障碍距离
                        block.SetDistance(EDirFlags.DownRight, distanceFromPrev - 1);
                    }
                }
            }

        }

        //可行走
        private bool IsWalkable(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                return false;
            }
            return BlockLUT[p.X, p.Y] >= 0;
        }

        //在地图边界里
        private bool IsInMapBoundary(in Int2 p)
        {
            return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
        }

        private JPSPlusMapBakerBlock GetBlockOrNull(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                return null;
            }

            int index = BlockLUT[p.X, p.Y];
            if (index < 0)
            {//是障碍物
                return null;
            }
            return Blocks[index];
        }
    }
}
