using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation
{
    [CreateAssetMenu(fileName = "ObstacleMapData", menuName = "YuJie/创建地图障碍数据")]
    public class ObstacleMapData: ScriptableObject
    {
        public int xDivisions;
        public int yDivisions;
        public Vector2 Center;
        public float GridWidth;

        /// <summary>
        /// 一维化数据
        /// </summary>
        public bool[] SerializedData;

        /// <summary>
        /// 将二维数组序列化到一维数组
        /// </summary>
        public void SerializeMap(bool[,] map,Vector2 center,float width)
        {
            xDivisions = map.GetLength(0);
            yDivisions = map.GetLength(1);
            GridWidth = width;
            Center = center;
            SerializedData = new bool[xDivisions * yDivisions];

            for (int x = 0; x < xDivisions; x++)
            {
                for (int y = 0; y < yDivisions; y++)
                {
                    SerializedData[x * yDivisions + y] = map[x, y];
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
                    map[x, y] = SerializedData[x * yDivisions + y];
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

            RectInt rect = new RectInt(x,y,width,height);
            return rect;
        }
    }
}
