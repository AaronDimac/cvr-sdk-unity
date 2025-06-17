using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class CustomEventDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.Label("Custom Events", EditorCore.styles.FeatureTitleStyle);
        }
    }
}
