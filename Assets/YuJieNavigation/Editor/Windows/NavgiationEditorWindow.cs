using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YuJie.Navigation.Editors
{
    /// <summary>
    /// TODO 
    /// layermask
    /// </summary>
    public class NavgiationEditorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public static void ShowWindow()
        {
            SetSceneView();
            var window = GetWindow<NavgiationEditorWindow>("地图导航数据编辑");
            window.minSize = new Vector2(300, 350);
            window.SetMapDrawer();
        }

        private FloatField m_gridWidthField;
        private MapRectField m_mapRectField;
        private static LayerMask m_layerMask = (1 << LayerMask.NameToLayer("UI")) | 1 << LayerMask.NameToLayer("Water");

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            m_gridWidthField = root.Q<FloatField>("GridWidth");
            m_mapRectField = root.Q<MapRectField>("MapRect");
            m_mapRectField.value = new RectInt(-50, 50, 50, -50);
            m_mapRectField.OnValueChanged += OnMapRectChanged;
            root.Q<Button>("BtnGenerateMap").clicked += OnGenerateBtnClick;
        }

        #region 监听

        //生成
        private void OnGenerateBtnClick()
        {
            bool succ = RefreshMapGrid();
            if (succ)
                succ = RefreshMapPlane();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("", succ ? "数据生成成功" : "数据生成失败", "确认");
        }

        private void OnMapRectChanged(RectInt rect)
        {
            RefreshMapBorder();
        }

        #endregion 监听

        public void OnDestroy()
        {
            if (m_gridDrawer != null)
                GameObject.DestroyImmediate(m_gridDrawer.gameObject);
            if (m_borderDrawer != null)
                GameObject.DestroyImmediate(m_borderDrawer.gameObject);
            if (m_planeDrawer != null)
                GameObject.DestroyImmediate(m_planeDrawer.gameObject);

            m_obsRects = null;
            ResetSceneView();
        }

        #region 地图网格
        private GridGizmoDrawer m_gridDrawer;
        private BorderGizmoDrawer m_borderDrawer;
        private PlaneGizmoDrawer m_planeDrawer;
        private List<Rect> m_obsRects;

        private void SetMapDrawer()
        {
            GameObject GridContainer = new GameObject("GridContainer");
            m_gridDrawer = GridContainer.AddComponent<GridGizmoDrawer>();
            m_gridDrawer.size = Vector2.zero;

            GameObject BorderContainer = new GameObject("BorderContainer");
            m_borderDrawer = BorderContainer.AddComponent<BorderGizmoDrawer>();
            m_borderDrawer.size = Vector2.zero;

            GameObject PlaneContainer = new GameObject("PlaneContainer");
            m_planeDrawer = PlaneContainer.AddComponent<PlaneGizmoDrawer>();
            m_planeDrawer.size = Vector2.zero;
        }

        /// <summary>
        /// 更新地图边框
        /// </summary>
        private void RefreshMapBorder()
        {
            RectInt rect = m_mapRectField.value;
            //横竖网格数量
            int xDivisions = Mathf.CeilToInt((rect.y - rect.x) / m_gridWidthField.value);
            int yDivisions = Mathf.CeilToInt((rect.width - rect.height) / m_gridWidthField.value);
            m_borderDrawer.size = new Vector2(m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);
            m_borderDrawer.position = new Vector3((rect.x + rect.y) / 2.0f, 2, (rect.width + rect.height) / 2.0f);

            SceneView.lastActiveSceneView.Repaint();
        }


        /// <summary>
        /// 更新网格
        /// </summary>
        private bool RefreshMapGrid()
        {
            if (EditorUtility.DisplayCancelableProgressBar("地图导航数据生成....", $"网格生成中...", 0.2f))
            {//取消
                if (m_gridDrawer)
                    m_gridDrawer.size = Vector2.zero;
                return false;
            }

            RectInt rect = m_mapRectField.value;
            //横竖网格数量
            int xDivisions = Mathf.CeilToInt((rect.y - rect.x) / m_gridWidthField.value);
            int yDivisions = Mathf.CeilToInt((rect.width - rect.height) / m_gridWidthField.value);

            m_gridDrawer.gridDivisions = new Vector2Int(xDivisions, yDivisions);
            m_gridDrawer.size = new Vector2(m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);

            m_gridDrawer.position = new Vector3((rect.x + rect.y) / 2.0f, 1, (rect.width + rect.height) / 2.0f);
            SceneView.lastActiveSceneView.Repaint();
            return true;
        }

        /// <summary>
        /// 更新地图网格面
        /// </summary>
        private bool RefreshMapPlane()
        {
            FindObstacleRenderersInScene();
            int obsCount = m_obsRects.Count;
            int gridCount = 0;
            int checkCount = 0;
            for (int i = 0; i < gridCount; i++)
            {
                bool cancelled = EditorUtility.DisplayCancelableProgressBar("地图导航数据生成....",
                    $"数据生成中...({checkCount}/{gridCount})",
                    (float)checkCount / gridCount);
                if (cancelled)
                {
                    if (m_planeDrawer)
                        m_planeDrawer.size = Vector2.zero;
                    return false;
                }
            }
            return true;
        }
        #endregion 地图网格

        #region Scene视图设置
        private static bool m_originalProj;

        private static void SetSceneView()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.GetWindow<SceneView>();
            }
            sceneView.Focus();

            sceneView.rotation = Quaternion.Euler(90f, 0f, 0f);
            sceneView.LookAt(Vector3.zero);
            m_originalProj = sceneView.orthographic;
            sceneView.orthographic = true;
            sceneView.isRotationLocked = true;
        }

        private void ResetSceneView()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.GetWindow<SceneView>();
            }
            sceneView.orthographic = m_originalProj;
            sceneView.isRotationLocked = false;
        }
        #endregion Scene视图设置

        /// <summary>
        /// 获取所有的Obstacle
        /// </summary>
        private void FindObstacleRenderersInScene()
        {
            GameObject[] obstacleObjects = GetAllGameObjectsInLayer();
            m_obsRects = new List<Rect>(obstacleObjects.Length);

            foreach (GameObject obj in obstacleObjects)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    continue;

                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled)
                    {
                        m_obsRects.Add(new Rect(renderer.bounds.min.x,renderer.bounds.min.z,renderer.bounds.size.x, renderer.bounds.size.z));
                    }
                }
            }
        }

        private GameObject[] GetAllGameObjectsInLayer()
        {
            // 查找场景中的所有游戏对象
            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);

            // 使用 LINQ 过滤指定层的对象
            return allGameObjects
                .Where(go => go.activeInHierarchy) // 只考虑活跃对象
                .Where(go => (m_layerMask.value & (1 << go.layer)) != 0) // 指定层的对象
                .Where(go => go.TryGetComponent<Renderer>(out _))//是渲染物
                .ToArray();
        }
    }
}