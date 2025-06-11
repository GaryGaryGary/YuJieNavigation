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
        [Header("网格设置")]
        public Vector3 position = Vector3.zero;
        [Tooltip("平面尺寸 (宽度, 高度)")]
        public Vector2 size = new Vector2(5, 5);
        [Tooltip("网格细分数量 (X, Y)")]
        public Vector2Int gridDivisions = new Vector2Int(5, 5);

        private Color gridColor = new Color(0f, 0f, 0f, 0.3f);
        private bool alwaysVisible = true;
        private Vector3 normal = Vector3.up;

        private void OnDrawGizmos()
        {
            if (alwaysVisible)
            {
                DrawGrid();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!alwaysVisible)
            {
                DrawGrid();
            }
        }

        private void DrawGrid()
        {
            // 计算坐标系
            Quaternion rotation = GetPlaneRotation();
            Vector3 center = transform.position + position;

            // 设置平面坐标系
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            // 绘制网格
            if (gridDivisions.x > 0 && gridDivisions.y > 0)
            {
                Gizmos.color = gridColor;
                DrawGrid(size.x, size.y, gridDivisions.x, gridDivisions.y);
            }
        }

        private Quaternion GetPlaneRotation()
        {
            // 确保法线向量有效
            if (normal.sqrMagnitude < 0.0001f)
            {
                normal = Vector3.up;
            }

            // 默认朝向上方
            Vector3 upDirection = Vector3.up;
            Vector3 planeNormal = normal.normalized;

            // 避免与默认方向相同导致计算出错
            if (Mathf.Abs(Vector3.Dot(planeNormal, upDirection)) > 0.9999f)
            {
                upDirection = Mathf.Abs(planeNormal.x) < 0.99f ?
                    new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
            }

            // 计算旋转 - 使平面的正面朝向法线方向
            return Quaternion.LookRotation(Vector3.Cross(planeNormal, upDirection).normalized, planeNormal);
        }

        private void DrawGrid(float width, float height, int divisionsX, int divisionsY)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            // 横向网格线
            for (int i = 0; i <= divisionsX; i++)
            {
                float x = Mathf.Lerp(-halfWidth, halfWidth, (float)i / divisionsX);
                Vector3 start = new Vector3(x, 0, -halfHeight);
                Vector3 end = new Vector3(x, 0, halfHeight);
                Gizmos.DrawLine(start, end);
            }

            // 纵向网格线
            for (int i = 0; i <= divisionsY; i++)
            {
                float z = Mathf.Lerp(-halfHeight, halfHeight, (float)i / divisionsY);
                Vector3 start = new Vector3(-halfWidth, 0, z);
                Vector3 end = new Vector3(halfWidth, 0, z);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
#endif