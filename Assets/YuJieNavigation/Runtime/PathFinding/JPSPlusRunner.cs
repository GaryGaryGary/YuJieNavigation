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

        private JPSPlusBakedMap m_bakedMap;
        private bool[,] m_obst = null;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Int2 StartP => m_startP.Value;
        public Int2 TargetP => m_targetP.Value;

        private Int2? m_startP = null;
        private Int2? m_targetP = null;

        public JPSPlusRunner(JPSPlusBakedMap bakedMap)
        {
            m_bakedMap = bakedMap;
            m_jpsPlus.Init(bakedMap);
            m_obst = bakedMap.DeserializeMap();
            Width = m_obst.GetLength(0);
            Height = m_obst.GetLength(1);
        }

        /// <summary>
        /// 设置起点
        /// </summary>  
        public bool SetStart(Vector3 pos)
        {
            return SetStart(ConvertV3ToInt2(pos));
        }

        /// <summary>
        /// 设置起点
        /// </summary>
        public bool SetStart(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                Debug.LogWarning("起点设置失败:"+p.ToString());
                return false;
            }
            m_startP = p;
            return true;
        }

        /// <summary>
        /// 设置目标点
        /// </summary>
        public bool SetTarget(Vector3 pos)
        {
            return SetTarget(ConvertV3ToInt2(pos));
        }

        /// <summary>
        /// 设置目标点
        /// </summary>
        public bool SetTarget(in Int2 p)
        {
            if (!IsInMapBoundary(p))
            {
                Debug.LogWarning("终点设置失败:" + p.ToString());
                return false;
            }
            m_targetP = p;
            return true;
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
            _ = m_jpsPlus.SetStart(m_startP.Value);
            _ = m_jpsPlus.SetTarget(m_targetP.Value);
            return m_jpsPlus.StepAll(stepCount);
        }

        public bool[,] GetObst()
        {
            return m_obst;
        }

        public IReadOnlyList<JPSPlusNode> GetPaths()
        {
            return m_jpsPlus.GetPaths();
        }

        /// <summary>
        /// 转换网格数据到世界坐标
        /// </summary>
        private Int2 ConvertV3ToInt2(Vector3 pos)
        {
            Int2 i = new Int2(0, 0);
            float dx = pos.x - m_bakedMap.OriginPos.x;
            float dy = pos.z - m_bakedMap.OriginPos.y;
            i.X = Mathf.FloorToInt(dx / m_bakedMap.GridWidth);
            i.Y = Mathf.FloorToInt(dy / m_bakedMap.GridWidth);
            return i;
        }

        private bool IsInMapBoundary(in Int2 p)
        {
            return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
        }


    }
}
