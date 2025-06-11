using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation
{
    /// <summary>
    /// 网格节点数据
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 可行走
        /// </summary>
        public bool Walkable { private set; get; }

        /// <summary>
        /// 网格横坐标
        /// </summary>
        public uint GridX { private set; get; }

        /// <summary>
        /// 网格纵坐标
        /// </summary>
        public uint GridY { private set; get; }

        /// <summary>
        /// 网格中心世界坐标
        /// </summary>
        public Vector2 CenterPos { private set; get; }

        public Node(uint x,uint y,Vector2 center,bool walkable = true)
        {
            GridX = x;
            GridY = y;
            CenterPos = center;
            Walkable = walkable;
        }

        public void RefreshWalkable(bool walkable)
        {
            Walkable = walkable;
        }
    }
}
