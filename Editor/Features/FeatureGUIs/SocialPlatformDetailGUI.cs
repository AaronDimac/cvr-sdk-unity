using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    public class SocialPlatformDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Social Platform", EditorCore.styles.FeatureTitle);

                float iconSize = EditorGUIUtility.singleLineHeight;
                Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize), GUILayout.Height(iconSize));

                if (GUI.Button(iconRect, EditorCore.InfoGrey, EditorCore.styles.InfoButton))
                {
                    Application.OpenURL("https://docs.cognitive3d.com/unity/components/#oculus-social-data");
                }

                GUILayout.FlexibleSpace(); // Push content to the left
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(
                "Automatically record Oculus user and app identity data such as App ID, Oculus ID, and Display Name by adding a component that performs an entitlement check using the Oculus platform.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space(10);

            GUILayout.Label("1. Prepare Your Oculus App (Meta Developer Portal)", EditorCore.styles.FeatureTitle);

            GUILayout.Label(
                "Set up your Oculus App ID on the Developer Dashboard, enable user data permissions, publish the app, and add the App ID in Unity under Oculus > Platform > Edit Settings.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space(10);

            GUILayout.Label("2. Add to Cognitive3D_Manager prefab", EditorCore.styles.FeatureTitle);
            GUILayout.Label("Adds the Social Platform component to the Cognitive3D_Manager prefab to record Oculus user data.", EditorStyles.wordWrappedLabel);

            var btnLabel = FeatureLibrary.TryGetComponent<Cognitive3D.Components.SocialPlatform>() ? "Remove Social Platform" : "Add Social Platform";
            if (GUILayout.Button(btnLabel, GUILayout.Height(30)))
            {
                FeatureLibrary.AddOrRemoveComponent<Cognitive3D.Components.SocialPlatform>();
            }
        }
    }
}
