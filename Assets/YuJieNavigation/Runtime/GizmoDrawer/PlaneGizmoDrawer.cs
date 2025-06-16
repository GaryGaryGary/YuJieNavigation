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
        private bool m_drawObsPlane, m_drawWalkablesPlane;
        private List<Vector3> m_obsCenters;

        private Vector3 m_gridSize = Vector3.one;

        private void Awake()
        {
        }

        public void SetObsData(List<Vector3> vectors, float width)
        {
            if (vectors == null)
                return;
            m_gridSize = new Vector3(width, 0, width);
            m_obsCenters = vectors;
            m_drawObsPlane = true;
        }

        public void Clear()
        {
            m_obsCenters = null;
            m_drawObsPlane = m_drawWalkablesPlane = false;
        }

        private void OnDrawGizmos()
        {
            if (m_alwaysVisible)
            {
                DrawObsPlane();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_alwaysVisible)
            {
                DrawObsPlane();
            }
        }

        private void DrawObsPlane()
        {
            if (!m_drawObsPlane)
                return;
            Gizmos.color = m_objColor;
            int count = m_obsCenters.Count;
            for (int i = 0; i < count; i++)
            {
                Gizmos.DrawCube(m_obsCenters[i], m_gridSize);
            }
        }

    }
}
#endif