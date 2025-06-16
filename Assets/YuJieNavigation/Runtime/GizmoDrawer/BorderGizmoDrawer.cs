#if UNITY_EDITOR
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    /// <summary>
    /// 边框绘制
    /// </summary>
    [ExecuteInEditMode]
    public class BorderGizmoDrawer : MonoBehaviour
    {
        private Color m_borderColor = new Color(0.1f, 0.5f, 0.2f, 1f);
        private bool m_alwaysVisible = true;
        private Vector3 m_normal = Vector3.up;
        private Quaternion m_rot;

        private Vector3 m_topLeft, m_topRight, m_bottomRight, m_bottomLeft;

        private bool m_draw;

        private Vector3 m_center;

        private void Awake()
        {
            m_rot = GetPlaneRotation();
        }

        public void SetBorder(Vector3 position, float width,float height)
        {
            m_center = position + transform.position;
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            m_topLeft = new Vector3(-halfWidth, 0, -halfHeight);
            m_topRight = new Vector3(halfWidth, 0, -halfHeight);
            m_bottomRight = new Vector3(halfWidth, 0, halfHeight);
            m_bottomLeft = new Vector3(-halfWidth, 0, halfHeight);

            m_draw = true;
        }

        public void Clear()
        {
            m_draw = false;
        }

        private void OnDrawGizmos()
        {
            if (m_alwaysVisible)
            {
                DrawPlane();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_alwaysVisible)
            {
                DrawPlane();
            }
        }

        private void DrawPlane()
        {
            if (!m_draw)
                return;
            Gizmos.matrix = Matrix4x4.TRS(m_center, m_rot, Vector3.one);
            Gizmos.color = m_borderColor;
            DrawPlaneBorder();
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

        private void DrawPlaneBorder()
        {
            Gizmos.DrawLine(m_topLeft, m_topRight);
            Gizmos.DrawLine(m_topRight, m_bottomRight);
            Gizmos.DrawLine(m_bottomRight, m_bottomLeft);
            Gizmos.DrawLine(m_bottomLeft, m_topLeft);
        }

    }
}
#endif