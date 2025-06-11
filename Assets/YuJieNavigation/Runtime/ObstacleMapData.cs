using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation
{
    [CreateAssetMenu(fileName = "ObstacleMapData", menuName = "YuJie/创建地图障碍数据")]
    public class ObstacleMapData: ScriptableObject
    {
        public int Width;
        public int Height;

        /// <summary>
        /// 一维化数据
        /// </summary>
        public bool[] SerializedData;

        /// <summary>
        /// 将二维数组序列化到一维数组
        /// </summary>
        public void SerializeMap(bool[,] map)
        {
            Width = map.GetLength(0);
            Height = map.GetLength(1);
            SerializedData = new bool[Width * Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SerializedData[x * Height + y] = map[x, y];
                }
            }
        }

        /// <summary>
        /// 从一维数组反序列化回二维数组
        /// </summary>
        public bool[,] DeserializeMap()
        {
            bool[,] map = new bool[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    map[x, y] = SerializedData[x * Height + y];
                }
            }
            return map;
        }

    }
}
