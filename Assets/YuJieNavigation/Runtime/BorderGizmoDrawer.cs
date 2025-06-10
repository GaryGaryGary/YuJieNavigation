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
        public Vector3 position = Vector3.zero;
        [Tooltip("平面尺寸 (宽度, 高度)")]
        public Vector2 size = new Vector2(5, 5);

        private bool alwaysVisible = true;

        private Vector3 normal = Vector3.up;
        private Color borderColor = new Color(0.1f, 0.5f, 0.2f, 1f);

        private void OnDrawGizmos()
        {
            if (alwaysVisible)
            {
                DrawPlane();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!alwaysVisible)
            {
                DrawPlane();
            }
        }

        private void DrawPlane()
        {
            // 计算坐标系
            Quaternion rotation = GetPlaneRotation();
            Vector3 center = transform.position + position;

            // 保存当前 Gizmos 状态
            Color originalColor = Gizmos.color;
            Matrix4x4 originalMatrix = Gizmos.matrix;

            // 设置平面坐标系
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            // 绘制平面边框
            Gizmos.color = borderColor;
            DrawPlaneBorder(size.x, size.y);

            // 恢复 Gizmos 设置
            Gizmos.color = originalColor;
            Gizmos.matrix = originalMatrix;
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

        private void DrawPlaneBorder(float width, float height)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            Vector3 topLeft = new Vector3(-halfWidth, 0, -halfHeight);
            Vector3 topRight = new Vector3(halfWidth, 0, -halfHeight);
            Vector3 bottomRight = new Vector3(halfWidth, 0, halfHeight);
            Vector3 bottomLeft = new Vector3(-halfWidth, 0, halfHeight);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

    }
}
#endif