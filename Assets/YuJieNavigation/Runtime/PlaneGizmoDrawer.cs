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
        private Vector3 m_normal = Vector3.up;
        private bool m_drawObsPlane, m_drawWalkablesPlane;
        private Quaternion m_rot;
        private List<Vector3> m_obsCenters;

        private Vector3 m_gridSize = Vector3.one;
        private Vector3 m_center;

        private void Awake()
        {
            m_rot = GetPlaneRotation();
        }

        public void SetObsData(List<Vector3> vectors,Vector3 position, float width)
        {
            if (vectors == null)
                return;
            m_gridSize = new Vector3(width, 0, width);
            m_obsCenters = vectors;
            m_center = transform.position + position;
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
            Gizmos.matrix = Matrix4x4.TRS(m_center, m_rot, Vector3.one);
            Gizmos.color = m_objColor;
            int count = m_obsCenters.Count;
            for (int i = 0; i < count; i++)
            {
                Gizmos.DrawCube(m_obsCenters[i], m_gridSize);
            }
        }

        private Quaternion GetPlaneRotation()
        {
            // 确保法线向量有效
            if (m_normal.sqrMagnitude < 0.0001f)
            {
                m_normal = Vector3.up;
            }

            // 默认朝向上方
            Vector3 upDirection = Vector3.up;
            Vector3 planeNormal = m_normal.normalized;

            // 避免与默认方向相同导致计算出错
            if (Mathf.Abs(Vector3.Dot(planeNormal, upDirection)) > 0.9999f)
            {
                upDirection = Mathf.Abs(planeNormal.x) < 0.99f ?
                    new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
            }

            // 计算旋转 - 使平面的正面朝向法线方向
            return Quaternion.LookRotation(Vector3.Cross(planeNormal, upDirection).normalized, planeNormal);
        }

    }
}
#endif