#if UNITY_EDITOR
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    /// <summary>
    /// 网格绘制
    /// </summary>
    [ExecuteInEditMode]
    public class GridGizmoDrawer : MonoBehaviour
    {
        private Vector2Int m_gridDivisions = new Vector2Int(5, 5);

        private Color m_gridColor = new Color(0f, 0f, 0f, 0.3f);
        private bool m_alwaysVisible = true;
        private Vector3 m_normal = Vector3.up;
        private bool m_draw;
        private Quaternion m_rot;
        private Vector3 m_center;
        private float m_halfWidth, m_halfHeight;

        private void Awake()
        {
            m_rot = GetPlaneRotation();
        }

        public void SetGrid(Vector3 position, float width, float height, Vector2Int gridDivision)
        {
            m_gridDivisions = gridDivision;
            m_center = position + transform.position;

            m_halfWidth = width / 2f;
            m_halfHeight = height / 2f;
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
                DrawGrid();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_alwaysVisible)
            {
                DrawGrid();
            }
        }

        private void DrawGrid()
        {
            if (!m_draw)
                return;

            Gizmos.matrix = Matrix4x4.TRS(m_center, m_rot, Vector3.one);

            if (m_gridDivisions.x > 0 && m_gridDivisions.y > 0)
            {
                Gizmos.color = m_gridColor;
                DrawGrid(m_gridDivisions.x, m_gridDivisions.y);
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

        private void DrawGrid(int divisionsX, int divisionsY)
        {
            // 横向网格线
            for (int i = 0; i <= divisionsX; i++)
            {
                float x = Mathf.Lerp(-m_halfWidth, m_halfWidth, (float)i / divisionsX);
                Vector3 start = new Vector3(x, 0, -m_halfHeight);
                Vector3 end = new Vector3(x, 0, m_halfHeight);
                Gizmos.DrawLine(start, end);
            }

            // 纵向网格线
            for (int i = 0; i <= divisionsY; i++)
            {
                float z = Mathf.Lerp(-m_halfHeight, m_halfHeight, (float)i / divisionsY);
                Vector3 start = new Vector3(-m_halfWidth, 0, z);
                Vector3 end = new Vector3(m_halfWidth, 0, z);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
#endif