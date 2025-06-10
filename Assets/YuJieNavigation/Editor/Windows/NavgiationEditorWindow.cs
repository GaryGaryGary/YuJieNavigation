using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YuJie.Navigation.Editors
{
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

        private IntegerField m_gridWidthField;
        private MapRectField m_mapRectField;
        private static LayerMask m_layerMask = (1 << LayerMask.NameToLayer("UI")) | 1 << LayerMask.NameToLayer("Water");

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            m_gridWidthField = root.Q<IntegerField>("GridWidth");
            m_mapRectField = root.Q<MapRectField>("MapRect");
            m_mapRectField.value = new RectInt(-50, 50, 50, -50);
            m_mapRectField.OnValueChanged += OnMapRectChanged;
            root.Q<Button>("BtnGenerateMap").clicked += OnGenerateBtnClick;
        }

        #region 监听
        //生成
        private void OnGenerateBtnClick()
        {//TODO 进度条
            var bounds = FindObstacleRenderersInScene();
            return;
            RefreshMapGrid();
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

            ResetSceneView();
        }

        #region 地图网格
        private GridGizmoDrawer m_gridDrawer;
        private BorderGizmoDrawer m_borderDrawer;

        private void SetMapDrawer()
        {
            GameObject GridContainer = new GameObject("GridContainer");
            m_gridDrawer = GridContainer.AddComponent<GridGizmoDrawer>();
            m_gridDrawer.size = Vector2.zero;

            GameObject BorderContainer = new GameObject("BorderContainer");
            m_borderDrawer = BorderContainer.AddComponent<BorderGizmoDrawer>();
            m_borderDrawer.size = Vector2.zero;
        }

        private void RefreshMapGrid()
        {
            RectInt rect = m_mapRectField.value;
            float gridwidth = m_gridWidthField.value * 1.0f;
            //横竖网格数量
            int xDivisions = (int)Mathf.Ceil((rect.y - rect.x) / gridwidth);
            int yDivisions = (int)Mathf.Ceil((rect.width - rect.height) / gridwidth);

            m_gridDrawer.gridDivisions = new Vector2Int(xDivisions,yDivisions);
            m_gridDrawer.size = new Vector2(m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);

            m_gridDrawer.position = new Vector3((rect.x + rect.y) / 2.0f, 1, (rect.width + rect.height) / 2.0f);
            SceneView.lastActiveSceneView.Repaint();
        }

        private void RefreshMapBorder()
        {
            RectInt rect = m_mapRectField.value;
            float gridwidth = m_gridWidthField.value * 1.0f;
            //横竖网格数量
            int xDivisions = (int)Mathf.Ceil((rect.y - rect.x) / gridwidth);
            int yDivisions = (int)Mathf.Ceil((rect.width - rect.height) / gridwidth);
            m_borderDrawer.size = new Vector2(m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);
            m_borderDrawer.position = new Vector3((rect.x + rect.y) / 2.0f, 2, (rect.width + rect.height) / 2.0f);

            SceneView.lastActiveSceneView.Repaint();
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
        /// 获取所有Obstacle的BoundsCenter
        /// </summary>
        private List<Vector3> FindObstacleRenderersInScene()
        {
            // 初始化结果列表
            List<Vector3> obstacleBoundsCenter = new List<Vector3>();

            // 获取所有带有"Obstacle"标签的游戏对象
            GameObject[] obstacleObjects = GetAllGameObjectsInLayer(1);

            foreach (GameObject obj in obstacleObjects)
            {
                // 获取对象上所有Renderer组件
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

                if (renderers.Length == 0)
                {
                    continue;
                }

                foreach (Renderer renderer in renderers)
                {
                    // 仅处理启用的Renderer
                    if (renderer.enabled)
                    {
                        obstacleBoundsCenter.Add(renderer.bounds.center);
                    }
                }
            }
            return obstacleBoundsCenter;
        }


        private GameObject[] GetAllGameObjectsInLayer(int targetLayer)
        {
            if (targetLayer < 0 || targetLayer > 31)
            {
                Debug.LogError($"Invalid layer index: {targetLayer}. Must be between 0 and 31.");
                return new GameObject[0];
            }

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