using System;
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
            GetWindow<NavgiationEditorWindow>("地图导航数据编辑").minSize = new Vector2(300,350);
        }

        private IntegerField m_gridWidthField;
        private MapRectField m_mapRectField;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            m_gridWidthField = root.Q<IntegerField>("GridWidth");
            m_mapRectField = root.Q<MapRectField>("MapRect");
            m_mapRectField.value = new RectInt(-10,-10,100,100);
            m_mapRectField.OnValueChanged += OnMapRectChanged;
            root.Q<Button>("BtnGenerateMap").clicked += OnGenerateBtnClick;
        }

        #region 监听
        private void OnGenerateBtnClick()
        {
            Debug.LogWarning("生成");
        }

        private void OnMapRectChanged(RectInt rect)
        {
            Debug.LogWarning(rect.ToString());
        }

        #endregion 监听

        public void OnDestroy()
        {
            ResetSceneView();
        }

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
    }
}