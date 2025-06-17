using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class RemoteControlsDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.Label("Remote Controls", EditorCore.styles.FeatureTitleStyle);
        }
    }
}
