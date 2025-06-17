using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class EyeTrackingDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.Label("Eye Tracking", EditorCore.styles.FeatureTitleStyle);
        }
    }
}
