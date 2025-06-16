#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    /// <summary>
    /// 平面绘制
    /// </summary>
    [ExecuteInEditMode]
    public class PlaneGizmoDrawer : MonoBehaviour
    {
        private Color m_objColor = new Color(1f, 0f, 0f, 0.5f);
        private bool m_alwaysVisible = true;
        private bool m_drawObstPlane;
        private List<Vector3> m_obstCenters;

        private Vector3 m_gridSize = Vector3.one;

        private void Awake()
        {
        }

        public void SetObstData(List<Vector3> vectors, float width)
        {
            if (vectors == null)
                return;
            m_gridSize = new Vector3(width, 0, width);
            m_obstCenters = vectors;
            m_drawObstPlane = true;
        }

        public void Clear()
        {
            m_obstCenters = null;
            m_drawObstPlane = false;
        }

        private void OnDrawGizmos()
        {
            if (m_alwaysVisible)
            {
                DrawObstPlane();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_alwaysVisible)
            {
                DrawObstPlane();
            }
        }

        private void DrawObstPlane()
        {
            if (!m_drawObstPlane)
                return;
            Gizmos.color = m_objColor;
            int count = m_obstCenters.Count;
            for (int i = 0; i < count; i++)
            {
                Gizmos.DrawCube(m_obstCenters[i], m_gridSize);
            }
        }

    }
}
#endif