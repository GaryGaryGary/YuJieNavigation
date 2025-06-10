using UnityEditor;
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    internal class NavigationTools
    {
        [MenuItem("Tools/Navi/打开地图导航设置", true)]
        private static bool ValidateOpenMapNavigationEditor() => !EditorApplication.isPlaying;


        [MenuItem("Tools/Navi/打开地图导航设置", false, priority = 2000)]
        private static void OpenMapNavigationEditor()
        {
            NavgiationEditorWindow.ShowWindow();
        }



    }
}
