#if UNITY_EDITOR
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    /// <summary>
    /// 平面绘制
    /// </summary>
    [ExecuteInEditMode]
    public class PlaneGizmoDrawer : MonoBehaviour
    {
        public Vector3 position = Vector3.zero;
        [Tooltip("平面尺寸 (宽度, 高度)")]
        public Vector2 size = new Vector2(5, 5);
        [Tooltip("网格细分数量 (X, Y)")]
        public Vector2Int gridDivisions = new Vector2Int(5, 5);

        [Header("可视化选项")]
        [Tooltip("平面表面颜色")]
        public Color faceColor = new Color(0.3f, 0.8f, 0.5f, 0.25f);

        [Tooltip("始终绘制 (在非选中状态下也可见)")]
        public bool alwaysVisible = true;

        private Vector3 normal = Vector3.up;

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
            if (Vector2.Equals(size,Vector2.zero))
                return;
            // 计算坐标系
            Quaternion rotation = GetPlaneRotation();
            Vector3 center = transform.position + position;

            // 设置平面坐标系
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            // 绘制平面表面
            Gizmos.color = faceColor;
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, 0, size.y));
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

    }
}
#endif