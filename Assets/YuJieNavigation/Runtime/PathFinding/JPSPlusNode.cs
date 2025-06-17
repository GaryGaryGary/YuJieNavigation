
namespace YuJie.Navigation
{
    /// <summary>
    /// 网格节点
    /// </summary>
    public class JPSPlusNode
    {
        public Int2 Position { get; private set; }

        public UnityEngine.Vector3 WorldPos { get; private set; }

        /// <summary>
        /// 代价
        /// </summary>
        public int G { get; internal set; } = 0;

        /// <summary>
        /// 启发函数值
        /// </summary>
        public int H { get; internal set; } = 0;

        public long F => G + H;

        /// <summary>
        /// 父节点
        /// </summary>
        public JPSPlusNode Parent { get; internal set; }

        /// <summary>
        /// 八方像距离
        /// </summary>
        private int[] m_jumpDistances;

        public JPSPlusNode(in Int2 p,in UnityEngine.Vector3 worldpos, int[] jumpDis)
        {
            G = 0;
            H = 0;
            Position = p;
            WorldPos = worldpos;
            m_jumpDistances = jumpDis;
        }

        public int GetDistance(EDirFlags dir)
        {
            return m_jumpDistances[DirFlags.ToArrayIndex(dir)];
        }

        internal void Refresh(int[] jumpDistances)
        {
            m_jumpDistances = jumpDistances;
            G = 0;
            H = 0;
            Parent = null;
        }
    }
}
