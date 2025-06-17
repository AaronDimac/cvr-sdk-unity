using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class ExitpollDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.Label("Exit Poll", EditorCore.styles.FeatureTitleStyle);
        }
    }
}
