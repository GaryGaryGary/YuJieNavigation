using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            window.LoadCurMapData();
        }

        private FloatField m_gridWidthField;
        private MapRectField m_mapRectField;
        private NaviEditorSetting m_setting;

        public void CreateGUI()
        {
            LoadSettings();
            VisualElement root = rootVisualElement;
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            m_gridWidthField = root.Q<FloatField>("GridWidth");
            m_mapRectField = root.Q<MapRectField>("MapRect");
            m_mapRectField.value = new RectInt(-50, 50, 50, -50);
            m_mapRectField.OnValueChanged += OnMapRectChanged;
            root.Q<Button>("BtnGenerateMap").clicked += OnGenerateBtnClick;
            root.Q<Button>("BtnSaveMap").clicked += OnSaveDataBtnClick;
        }

        private void LoadSettings()
        {
            m_setting = Resources.Load<NaviEditorSetting>("NaviEditorSetting");
            m_obstLayer = m_setting.ObstacleLayer.value;
        }

        #region 事件监听
        private void OnGenerateBtnClick()
        {
            bool succ = RefreshMapGrid(0.2f);
            if (succ)
                succ = RefreshGridData(0.5f);
            if (succ)
                succ = RefreshMapPlane(0.8f);
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("", succ ? "数据生成成功" : "数据生成失败", "确认");
        }

        private void OnMapRectChanged(RectInt rect)
        {
            RefreshMapBorder();
        }

        private void OnSaveDataBtnClick()
        {
            if (m_obstGrid == null || m_obstGrid.Length == 0)
            {
                Debug.LogWarning("障碍数据为空.");
                return;
            }
            if (m_setting == null || string.IsNullOrEmpty(m_setting.SaveOrLoadPath))
            {
                Debug.LogWarning("保存路径为空.");
                return;
            }
            string soName = $"{SceneManager.GetActiveScene().name}_BakedMap.asset";
            try
            {
                var mapSo = ScriptableObject.CreateInstance<JPSPlusBakedMap>();
                AssetDatabase.CreateAsset(mapSo, Path.Combine(m_setting.SaveOrLoadPath, soName));
                mapSo.Bake(m_obstGrid, m_centerPos, m_gridwidth);
                EditorUtility.SetDirty(mapSo);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("", $"{soName}保存成功", "确认");
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog("", $"{soName}保存失败", "确认");
                throw;
            }
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

            m_obstGrid = null;
            m_setting = null;
            m_obstRects = null;
            ResetSceneView();
        }

        #region 地图网格
        private GridGizmoDrawer m_gridDrawer;
        private BorderGizmoDrawer m_borderDrawer;
        private PlaneGizmoDrawer m_planeDrawer;

        private void SetMapDrawer()
        {
            GameObject GridContainer = new GameObject("GridContainer");
            m_gridDrawer = GridContainer.AddComponent<GridGizmoDrawer>();
            m_gridDrawer.Clear();

            GameObject BorderContainer = new GameObject("BorderContainer");
            m_borderDrawer = BorderContainer.AddComponent<BorderGizmoDrawer>();
            m_borderDrawer.Clear();

            GameObject PlaneContainer = new GameObject("PlaneContainer");
            m_planeDrawer = PlaneContainer.AddComponent<PlaneGizmoDrawer>();
            m_planeDrawer.Clear();
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

            var pos = new Vector3((rect.x + rect.y) / 2.0f, 5, (rect.width + rect.height) / 2.0f);
            m_borderDrawer.SetBorder(pos,m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);
            SceneView.lastActiveSceneView.Repaint();
        }


        /// <summary>
        /// 更新网格
        /// </summary>
        private bool RefreshMapGrid(float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("地图导航数据生成....", $"网格生成中...", progress))
            {//取消
                if (m_gridDrawer)
                    m_gridDrawer.Clear();
                return false;
            }
            try
            {
                RectInt rect = m_mapRectField.value;
                //横竖网格数量
                int xDivisions = Mathf.CeilToInt((rect.y - rect.x) / m_gridWidthField.value);
                int yDivisions = Mathf.CeilToInt((rect.width - rect.height) / m_gridWidthField.value);

                var div = new Vector2Int(xDivisions, yDivisions);
                var size = new Vector2(m_gridWidthField.value * xDivisions, m_gridWidthField.value * yDivisions);

                var pos = new Vector3((rect.x + rect.y) / 2.0f, 4, (rect.width + rect.height) / 2.0f);
                m_gridDrawer.SetGrid(pos,size.x, size.y, div);
                SceneView.lastActiveSceneView.Repaint();
            }
            catch (Exception)
            {
                if (m_gridDrawer)
                    m_gridDrawer.Clear();
                EditorUtility.ClearProgressBar();
                throw;
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

            sceneView.in2DMode = false;
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

        #region 网格数据处理
        private List<Rect> m_obstRects;

        /// <summary>
        /// 网格数据
        /// true为障碍物
        /// </summary>
        private bool[,] m_obstGrid;

        /// <summary>
        /// 障碍物网格
        /// </summary>
        private List<Vector3> m_obstNodes;

        /// <summary>
        /// 可通行网格
        /// </summary>
        private List<Vector3> m_walkableNodes;

        /// <summary>
        /// 障碍物形成的包围边界
        /// </summary>
        private float m_minX, m_maxX, m_minY, m_maxY;

        /// <summary>
        /// 刷新网格数据
        /// </summary>
        private bool RefreshGridData(float progress)
        {
            bool cancelled = EditorUtility.DisplayCancelableProgressBar("地图导航数据生成....", $"网格数据更新中...", progress);
            if (cancelled)
            {
                if (m_obstRects != null)
                    m_obstRects.Clear();
                m_obstGrid = null;
                m_walkableNodes = null;
                m_obstNodes = null;
                return false;
            }

            try
            {
                FindObstacleRenderersInScene();
                SetGridObstInfo();
            }
            catch (Exception)
            {
                if (m_obstRects != null)
                    m_obstRects.Clear();
                m_obstGrid = null;
                m_walkableNodes = null;
                m_obstNodes = null;
                EditorUtility.ClearProgressBar();
                throw;
            }
            return true;
        }

        private void SetGridObstInfo()
        {
            float gridW = m_gridWidthField.value;
            //横竖网格数量
            RectInt rect = m_mapRectField.value;
            int xDivisions = Mathf.CeilToInt((rect.y - rect.x) / gridW);
            int yDivisions = Mathf.CeilToInt((rect.width - rect.height) / gridW);
            m_obstNodes = new List<Vector3>(xDivisions * yDivisions / 2);
            m_walkableNodes = new List<Vector3>(xDivisions * yDivisions / 2);

            Vector2 center = new Vector2((rect.x + rect.y) / 2.0f, (rect.width + rect.height) / 2.0f);
            m_obstGrid = new bool[xDivisions, yDivisions];

            //左下角世界坐标
            Vector2 lbPos = new Vector2(center.x - xDivisions * gridW / 2, center.y - yDivisions * gridW / 2);
            Rect checkRect = new Rect(lbPos.x, lbPos.y, gridW, gridW);

            for (int x = 0; x < xDivisions; x++)
            {
                for (int y = 0; y < yDivisions; y++)
                {
                    checkRect.x = lbPos.x + gridW * x;
                    checkRect.y = lbPos.y + gridW * y;
                    bool isObst = CheckRectOverlap(checkRect);
                    m_obstGrid[x, y] = isObst;
                    if (isObst)
                        m_obstNodes.Add(new Vector3(checkRect.center.x, 6, checkRect.center.y));
                    else
                        m_walkableNodes.Add(new Vector3(checkRect.center.x, 0, checkRect.center.y));
                }
            }
        }

        private bool CheckRectOverlap(Rect testRect)
        {
            //先处理边界情况
            if (testRect.xMax <= m_minX)
                return false;
            if (testRect.xMin >= m_maxX)
                return false;
            if (testRect.yMax <= m_minY)
                return false;
            if (testRect.yMin >= m_maxY)
                return false;

            int count = m_obstRects.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_obstRects[i].Overlaps(testRect))
                    return true;
            }
            return false;
        }

        //获取所有的Obstacle
        private void FindObstacleRenderersInScene()
        {
            m_minX = m_minY = int.MaxValue;
            m_maxX = m_maxY = int.MinValue;
            GameObject[] obstacleObjects = GetAllGameObjectsInLayer();
            m_obstRects = new List<Rect>(obstacleObjects.Length);
            Renderer[] renderers;
            foreach (GameObject obj in obstacleObjects)
            {
                renderers = obj.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    continue;
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled)
                    {
                        var newRect = new Rect(renderer.bounds.min.x, renderer.bounds.min.z, renderer.bounds.size.x, renderer.bounds.size.z);
                        m_obstRects.Add(newRect);
                        m_minX = Mathf.Min(m_minX, newRect.xMin);
                        m_minY = Mathf.Min(m_minY, newRect.yMin);
                        m_maxX = Mathf.Max(m_maxX, newRect.xMax);
                        m_maxY = Mathf.Max(m_maxY, newRect.yMax);
                    }
                }
            }

            if (m_obstRects.Count == 0)
            {
                Debug.LogWarning("障碍物检测为空");
            }
        }

        private int m_obstLayer;
        private GameObject[] GetAllGameObjectsInLayer()
        {
            // 查找场景中的所有游戏对象
            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
            return allGameObjects
                .Where(go => go.activeInHierarchy) // 只考虑活跃对象
                .Where(go => (m_obstLayer & (1 << go.layer)) != 0) // 指定层的对象
                .Where(go => go.TryGetComponent<Renderer>(out _))//是渲染物
                .ToArray();
        }
        #endregion 网格数据处理


        #region 网格障碍显示
        private Vector2 m_centerPos;
        private float m_gridwidth;

        /// <summary>
        /// 更新地图网格面
        /// </summary>
        private bool RefreshMapPlane(float progress)
        {
            bool cancelled = EditorUtility.DisplayCancelableProgressBar("地图导航数据生成....", $"网格表面生成中...", progress);
            if (cancelled)
            {
                if (m_planeDrawer)
                    m_planeDrawer.Clear();
                return false;
            }

            try
            {
                RectInt rect = m_mapRectField.value;
                m_gridwidth = m_gridWidthField.value;
                m_centerPos = new Vector2((rect.x + rect.y) / 2.0f, (rect.width + rect.height) / 2.0f);
                m_planeDrawer.SetObstData(m_obstNodes, m_gridWidthField.value);
                SceneView.lastActiveSceneView.Repaint();
            }
            catch (Exception)
            {
                if (m_planeDrawer)
                    m_planeDrawer.Clear();
                EditorUtility.ClearProgressBar();
                throw;
            }

            return true;
        }

        #endregion 网格障碍显示

        private void LoadCurMapData()
        {
            if (m_setting == null)
                return;
            string soName = $"{SceneManager.GetActiveScene().name}_BakedMap.asset";
            var mapdata = AssetDatabase.LoadAssetAtPath<JPSPlusBakedMap>(Path.Combine(m_setting.SaveOrLoadPath, soName));
            if (mapdata == null)
                return;
            m_gridWidthField.value = mapdata.GridWidth;
            m_mapRectField.SetValueWithoutNotify(mapdata.GetMapRect());
            if (m_gridWidthField.value <= 0)
                return;
            m_centerPos = mapdata.Center;
            m_gridwidth = mapdata.GridWidth;
            Preview(mapdata);
        }

        private void Preview(JPSPlusBakedMap data )
        {
            int xDivisions = data.xDivisions;
            int yDivisions = data.yDivisions;
            m_obstNodes = new List<Vector3>(xDivisions * yDivisions / 2);
            var center = data.Center;

            //左下角世界坐标
            Vector2 lbPos = new Vector2(center.x - xDivisions * m_gridwidth / 2, center.y - yDivisions * m_gridwidth / 2);
            Rect checkRect = new Rect(lbPos.x, lbPos.y, m_gridwidth, m_gridwidth);

            var list = data.DeserializeMap();
            int rows = list.GetLength(0);
            int cols = list.GetLength(1);
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    if (list[x,y] == true)
                    {//障碍
                        checkRect.x = lbPos.x + m_gridwidth * x;
                        checkRect.y = lbPos.y + m_gridwidth * y;
                        m_obstNodes.Add(new Vector3(checkRect.center.x, 6, checkRect.center.y));
                    }
                }
            }

            //边框
            m_borderDrawer.SetBorder(new Vector3(center.x, 5, center.y), m_gridwidth * xDivisions, m_gridwidth * yDivisions);
            //网格
            m_gridDrawer.SetGrid(new Vector3(center.x, 4, center.y),
                m_gridWidthField.value * xDivisions,
                m_gridWidthField.value * yDivisions,
                new Vector2Int(xDivisions, yDivisions));
            //障碍物
            m_planeDrawer.SetObstData(m_obstNodes, m_gridwidth);
            SceneView.lastActiveSceneView.Repaint();
        }
    }
}