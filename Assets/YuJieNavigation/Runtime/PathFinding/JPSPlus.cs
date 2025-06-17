using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace YuJie.Navigation
{
    internal class JPSPlus
    {
        private JPSPlusNode m_start = null;
        private JPSPlusNode m_target = null;

        private readonly Dictionary<Int2,JPSPlusNode> m_createdNodes = new Dictionary<Int2, JPSPlusNode>();
        private readonly PriorityQueue<(JPSPlusNode Node, EDirFlags Dir)> m_openList = new PriorityQueue<(JPSPlusNode Node, EDirFlags Dir)>();
        private readonly HashSet<JPSPlusNode> m_closeList = new HashSet<JPSPlusNode>();
        private JPSPlusBakedMap m_bakedMap;

        public int Width => m_bakedMap.xDivisions;
        public int Height => m_bakedMap.yDivisions;

        public JPSPlus() { }

        public void Init(JPSPlusBakedMap bakedMap)
        {
            m_bakedMap = bakedMap;
        }

        public bool StepAll(int stepCount = int.MaxValue)
        {
            m_openList.Clear();
            m_closeList.Clear();

            var lut = m_bakedMap.GetBlockLUT();
            foreach (JPSPlusNode node in m_createdNodes.Values.ToList())
            {
                int index = lut[node.Position.X, node.Position.Y];
                if(index < 0)
                {//障碍
                    _ = m_createdNodes.Remove(node.Position);
                }
                else
                {
                    node.Refresh(m_bakedMap.Blocks[index].JumpDistances);
                }
            }
            m_openList.Enqueue((m_start, EDirFlags.ALL), m_start.F);
            return Step(stepCount);
        }

        public bool Step(int stepCount)
        {
            int step = stepCount;
            while (true)
            {
                if(step <= 0)
                    return false;
                if (m_openList.Count == 0)
                    return false;

                (JPSPlusNode node,EDirFlags fromDir) cur = m_openList.Dequeue();
                JPSPlusNode curNode = cur.node;
                EDirFlags fromDir = cur.fromDir;
                _ = m_closeList.Add(curNode);

                Int2 curPos = curNode.Position;
                Int2 targetPos = m_target.Position;

                if (curPos == targetPos)
                {//到达目标点
                    return true;
                }

                EDirFlags validDirs = ValidLookUpTable(fromDir);
                for (int i = 0b10000000; i > 0; i >>= 1)
                {
                    EDirFlags processDir = (EDirFlags)i;
                    if ((processDir & validDirs) == EDirFlags.NONE)
                        continue;

                    bool isDiagonalDir = DirFlags.IsDiagonal(processDir);
                    int dirDistance = curNode.GetDistance(processDir);
                    int lengthX = RowDiff(curNode, m_target);
                    int lengthY = ColDiff(curNode, m_target);

                    //当前节点到目标节点的最大行列距
                    int maxDiff = Math.Max(lengthX, lengthY);
                    //当前节点到目标节点的最小行列距
                    int minDiff = Math.Min(lengthX, lengthY);

                    JPSPlusNode nextNode;
                    int nextG;
                    if(!isDiagonalDir
                        && IsTargetInExactDirection(curPos,processDir,targetPos)
                        && maxDiff <= Math.Abs(dirDistance))
                    {//直线方向,与目标方向相同,且与目标的最大行列距小于可直接前进的距离,则找到目标
                        nextNode = m_target;
                        nextG = curNode.G + (maxDiff * 10);//直线消耗代价
                    }else if(isDiagonalDir
                        && IsTargetInGeneralDirection(curPos, processDir, targetPos)
                        && minDiff <= Math.Abs(dirDistance))
                    {//目标跳点,斜线方向
                        nextNode = GetNode(curNode, minDiff, processDir);
                        nextG = curNode.G + (maxDiff * 14);//斜线消耗代价
                    }else if(dirDistance > 0)
                    {//跳点
                        nextNode = GetNode(curNode, processDir);
                        if (isDiagonalDir)
                        {
                            nextG = curNode.G + maxDiff * 14;
                        }
                        else
                        {
                            nextG = curNode.G + maxDiff * 10;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    (JPSPlusNode, EDirFlags) openJump = (nextNode, processDir);
                    if(!m_openList.Contains(openJump) && !m_closeList.Contains(nextNode))
                    {//第一次遍历到该节点
                        nextNode.Parent = curNode;
                        nextNode.G = nextG;
                        nextNode.H = H(nextNode,m_target);
                        m_openList.Enqueue(openJump, nextNode.F);
                    }else if(nextG < nextNode.G)
                    {//已遍历过,且当前计算消耗小于下一节点缓存消耗,则刷新数据
                        nextNode.Parent = curNode;
                        nextNode.G = nextG;
                        nextNode.H = H(nextNode, m_target);
                        m_openList.UpdatePriority(openJump, nextNode.F);
                    }
                }
                step--;
            }
        }

        /// <summary>
        /// 设置寻路起点
        /// </summary>
        public bool SetStart(in Int2 p)
        {
            if (m_bakedMap == null)
                return false;

            if (!IsInMapBoundary(p))
                return false;

            m_start = GetOrCreatedNode(p);
            return true;
        }

        /// <summary>
        /// 设置寻路终点
        /// </summary>
        public bool SetTarget(in Int2 p)
        {
            if(m_bakedMap == null)
                return false;

            if (!IsInMapBoundary(p))
                return false;

            m_target = GetOrCreatedNode(p);
            return true;
        }

        /// <summary>
        /// 获取路径
        /// </summary>
        public IReadOnlyList<JPSPlusNode> GetPaths()
        {
            List<JPSPlusNode> ret = new List<JPSPlusNode>();
            JPSPlusNode n = m_target;
            while(n != null)
            {
                ret.Add(n);
                n = n.Parent;
            }
            ret.Reverse();
            return ret;
        }

        public JPSPlusNode GetJPSPlusNode(in Int2 p)
        {
            var lut = m_bakedMap.GetBlockLUT();
            return new JPSPlusNode(p, m_bakedMap.Blocks[lut[p.X,p.Y]].JumpDistances);
        }

        private JPSPlusNode GetOrCreatedNode(in Int2 p)
        {
            if (m_createdNodes.TryGetValue(p,out JPSPlusNode createdNode))
            {
                return createdNode;
            }
            JPSPlusNode newNode = GetJPSPlusNode(p);
            m_createdNodes.Add(p, newNode);
            return newNode;
        }

        private bool IsInMapBoundary(in Int2 p)
        {
            return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
        }

        /// <summary>
        /// 获取可能的有效方向
        /// </summary>
        private EDirFlags ValidLookUpTable(EDirFlags dir)
        {
            switch (dir)
            {
                case EDirFlags.Up:
                    return EDirFlags.Right | EDirFlags.UpRight | EDirFlags.Up | EDirFlags.UpLeft | EDirFlags.Left;
                case EDirFlags.Left:
                    return EDirFlags.Up | EDirFlags.UpLeft | EDirFlags.Left | EDirFlags.DownLeft | EDirFlags.Down;
                case EDirFlags.Right:
                    return EDirFlags.Down | EDirFlags.DownRight | EDirFlags.Right | EDirFlags.UpRight | EDirFlags.Up;
                case EDirFlags.Down:
                    return EDirFlags.Left | EDirFlags.DownLeft | EDirFlags.Down | EDirFlags.DownRight | EDirFlags.Right;
                case EDirFlags.UpLeft:
                    return EDirFlags.Up | EDirFlags.UpLeft | EDirFlags.Left;
                case EDirFlags.UpRight:
                    return EDirFlags.Up | EDirFlags.UpRight | EDirFlags.Right;
                case EDirFlags.DownLeft:
                    return EDirFlags.Down | EDirFlags.UpLeft | EDirFlags.Left;
                case EDirFlags.DownRight:
                    return EDirFlags.Down | EDirFlags.UpRight | EDirFlags.Right;
                default:
                    return dir;
            }
        }

        /// <summary>
        /// 精确方向
        /// </summary>
        private bool IsTargetInExactDirection(in Int2 cur,EDirFlags processDir,in Int2 target)
        {
            int dx = target.X - cur.X;
            int dy = target.Y - cur.Y;

            switch (processDir)
            {
                case EDirFlags.Up:
                    return dx == 0 && dy > 0;
                case EDirFlags.Down:
                    return dx == 0 && dy < 0;
                case EDirFlags.Left:
                    return dx < 0 && dy == 0;
                case EDirFlags.Right:
                    return dx > 0 && dy == 0;
                case EDirFlags.UpLeft:
                    return dx < 0 && dy > 0 && (Math.Abs(dx) == Math.Abs(dy));
                case EDirFlags.UpRight:
                    return dx > 0 && dy > 0 && (Math.Abs(dx) == Math.Abs(dy));
                case EDirFlags.DownLeft:
                    return dx < 0 && dy < 0 && (Math.Abs(dx) == Math.Abs(dy));
                case EDirFlags.DownRight:
                    return dx > 0 && dy < 0 && (Math.Abs(dx) == Math.Abs(dy));
                default:
                    return false;
            }
        }

        /// <summary>
        /// 大概方向
        /// </summary>
        private bool IsTargetInGeneralDirection(in Int2 cur,EDirFlags processDir, in Int2 target)
        {
            int dx = target.X - cur.X;
            int dy = target.Y - cur.Y;

            switch (processDir)
            {
                case EDirFlags.Up:
                    return dx == 0 && dy > 0;
                case EDirFlags.Down:
                    return dx == 0 && dy < 0;
                case EDirFlags.Left:
                    return dx < 0 && dy == 0;
                case EDirFlags.Right:
                    return dx > 0 && dy == 0;
                case EDirFlags.UpLeft:
                    return dx < 0 && dy > 0;
                case EDirFlags.UpRight:
                    return dx > 0 && dy > 0;
                case EDirFlags.DownLeft:
                    return dx < 0 && dy < 0;
                case EDirFlags.DownRight:
                    return dx > 0 && dy < 0;
                default:
                    return false;
            }
        }

        private JPSPlusNode GetNode(JPSPlusNode node,EDirFlags dir)
        {
            return GetOrCreatedNode(node.Position + (DirFlags.ToPos(dir) * node.GetDistance(dir)));
        }

        private JPSPlusNode GetNode(JPSPlusNode node, int dist, EDirFlags dir)
        {
            return GetOrCreatedNode(node.Position + (DirFlags.ToPos(dir) * dist));
        }

        /// <summary>
        /// 列距
        /// </summary>
        private int ColDiff(JPSPlusNode curNode, JPSPlusNode targetNode)
        {
            return Math.Abs(targetNode.Position.Y - curNode.Position.Y);
        }

        /// <summary>
        /// 行距
        /// </summary>
        private int RowDiff(JPSPlusNode curNode, JPSPlusNode targetNode)
        {
            return Math.Abs(targetNode.Position.X - curNode.Position.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int H(JPSPlusNode n,JPSPlusNode target)
        {
            return (Math.Abs(target.Position.X - n.Position.X) + Math.Abs(target.Position.Y - n.Position.Y)) * 10;
        }
    }
}
