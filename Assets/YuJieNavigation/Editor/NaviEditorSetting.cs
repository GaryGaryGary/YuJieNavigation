using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuJie.Navigation.Editors
{
    [CreateAssetMenu(fileName = "NaviEditorSetting", menuName = "YuJie/创建导航设置")]
    public class NaviEditorSetting : ScriptableObject
    {
        public string SaveOrLoadPath = "";
        public LayerMask ObstacleLayer;
    }
}
