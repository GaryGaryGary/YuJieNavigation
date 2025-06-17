using System;
using UnityEngine;

namespace YuJie.Navigation
{
    [CreateAssetMenu(fileName = "BakedMapData", menuName = "YuJie/创建地图寻路烘焙数据")]
    public class JPSPlusBakedMap : ScriptableObject
    {
        public JPSPlusBakedMapBlock[] Blocks => m_blocks;
        public bool[] SerializedBlockData => m_serializedBlockData;
        public int xDivisions => m_xDivisions;
        public int yDivisions => m_yDivisions;
        public Vector2 Center => m_center;
        public float GridWidth => m_gridWidth;
        public Vector2 OriginPos => m_originPos;

        [SerializeField,HideInInspector]
        private JPSPlusBakedMapBlock[] m_blocks;
        [SerializeField,HideInInspector]
        private bool[] m_serializedBlockData;
        [SerializeField]
        private int m_xDivisions, m_yDivisions;
        [SerializeField]
        private Vector2 m_center, m_originPos;
        [SerializeField]
        private float m_gridWidth;
        [SerializeField,HideInInspector]
        private int[] m_LUTs;


        /// <summary>
        /// 查找表
        /// </summary>
        private int[,] m_blockLUT;

        public void Bake(bool[,] map, Vector2 center, float width,Vector2 originPos)
        {
            SerializeMap(map,center,width);
            JPSPlusMapBaker jpspBaker = new JPSPlusMapBaker(map);
            var result = jpspBaker.Bake();
            this.m_blocks = result.blocks;
            this.m_originPos = originPos;
            this.m_blockLUT = result.blockLUT;

            m_LUTs = new int[m_xDivisions * m_yDivisions];
            for (int x = 0; x < m_xDivisions; x++)
            {
                for (int y = 0; y < m_yDivisions; y++)
                {
                    m_LUTs[x * m_yDivisions + y] = this.m_blockLUT[x, y];
                }
            }
        }


        public int[,] GetBlockLUT()
        {
            if(m_blockLUT == null)
            {
                m_blockLUT = new int[m_xDivisions, m_yDivisions];
                for (int x = 0; x < m_xDivisions; x++)
                {
                    for (int y = 0; y < m_yDivisions; y++)
                    {
                        m_blockLUT[x, y] = m_LUTs[x * m_yDivisions + y];
                    }
                }
            }
            return m_blockLUT;
        }

        [Serializable]
        public class JPSPlusBakedMapBlock
        {
            /// <summary>
            /// 八方向距离
            /// </summary>
            public int[] JumpDistances => m_jumpDistances;

            public Int2 Pos => m_pos;

            [SerializeField]
            private int[] m_jumpDistances;

            [SerializeField]
            private Int2 m_pos;

            public JPSPlusBakedMapBlock(in Int2 pos, int[] jumpDistances)
            {
                m_jumpDistances = jumpDistances;
                m_pos = pos;
            }
        }

        /// <summary>
        /// 将二维数组序列化到一维数组
        /// </summary>
        private void SerializeMap(bool[,] map, Vector2 center, float width)
        {
            m_xDivisions = map.GetLength(0);
            m_yDivisions = map.GetLength(1);
            m_gridWidth = width;
            m_center = center;
            m_serializedBlockData = new bool[xDivisions * yDivisions];

            for (int x = 0; x < xDivisions; x++)
            {
                for (int y = 0; y < yDivisions; y++)
                {
                    m_serializedBlockData[x * yDivisions + y] = map[x, y];
                }
            }
        }

        /// <summary>
        /// 从一维数组反序列化回二维数组
        /// </summary>
        public bool[,] DeserializeMap()
        {
            bool[,] map = new bool[xDivisions, yDivisions];
            for (int x = 0; x < xDivisions; x++)
            {
                for (int y = 0; y < yDivisions; y++)
                {
                    map[x, y] = m_serializedBlockData[x * yDivisions + y];
                }
            }
            return map;
        }

        public RectInt GetMapRect()
        {
            int x = (int)Math.Round(Center.x - xDivisions * GridWidth / 2.0f);
            int y = (int)Math.Round(Center.x + xDivisions * GridWidth / 2.0f);
            int width = (int)Math.Round(Center.y + yDivisions * GridWidth / 2.0f);
            int height = (int)Math.Round(Center.y - yDivisions * GridWidth / 2.0f);

            RectInt rect = new RectInt(x, y, width, height);
            return rect;
        }
    }
}
