using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class DynamicObjectDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.Label("Dynamic Objects", EditorCore.styles.FeatureTitleStyle);
            GUILayout.Label(
                "A Dynamic Object is a specific object in your experience which you wish to track.",
                EditorStyles.wordWrappedLabel
            );
        }
    }
}
