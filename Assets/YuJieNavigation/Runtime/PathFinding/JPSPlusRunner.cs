using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation
{
    /// <summary>
    /// 运行器
    /// </summary>
    public class JPSPlusRunner
    {
        private readonly JPSPlus m_jpsPlus = new JPSPlus();
        private readonly JPSPlusMapBaker m_baker = new JPSPlusMapBaker();
        private bool[,] m_obst = null;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Int2 StartP => m_startP.Value;
        public Int2 TargetP => m_targetP.Value;

        private bool m_isObstChanged = true;
        private Int2? m_startP = null;
        private Int2? m_targetP = null;

        public JPSPlusRunner(int width, int height)
        {
            Init(new bool[width, height]);
        }

        public JPSPlusRunner(bool[,] obst)
        {
            Init(obst);
        }

        public void Init(bool[,] obst)
        {
            m_obst = obst;
            Width = m_obst.GetLength(0);
            Height = m_obst.GetLength(1);
        }

        /// <summary>
        /// 设置起点
        /// </summary>
        public void SetStart(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                return;
            }
            m_startP = p;
        }

        /// <summary>
        /// 设置目标点
        /// </summary>
        public void SetTarget(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                return;
            }
            m_targetP = p;
        }

        /// <summary>
        /// 是否可通行
        /// </summary>
        public bool IsWalkable(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                return false;
            }
            return !m_obst[p.X, p.Y];
        }

        /// <summary>
        /// 执行步骤
        /// </summary>
        public bool StepAll(int stepCount = int.MaxValue)
        {
            if(m_isObstChanged)
            {
                m_baker.Init(m_obst);
                m_jpsPlus.Init(m_baker.Bake());
                m_isObstChanged = false;
            }
            _ = m_jpsPlus.SetStart(m_startP.Value);
            _ = m_jpsPlus.SetTarget(m_targetP.Value);
            return m_jpsPlus.StepAll(stepCount);
        }

        public void SetObst(in Int2 p, bool isObst)
        {
            if (!IsInMapBoundary(p))
            {
                return;
            }
            m_obst[p.X, p.Y] = isObst;
            m_isObstChanged = true;
        }

        public bool[,] GetObst()
        {
            return m_obst;
        }

        public IReadOnlyList<JPSPlusNode> GetPaths()
        {
            return m_jpsPlus.GetPaths();
        }
        private bool IsInMapBoundary(in Int2 p)
        {
            return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
        }


    }
}
