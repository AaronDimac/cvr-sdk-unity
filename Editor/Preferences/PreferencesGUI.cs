using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class PreferencesGUI
    {
        internal void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorCore.styles.DetailContainer))
            {
                // Load current preferences
                var currentPrefs = EditorCore.GetPreferences();

                if (currentPrefs == null)
                {
                    EditorGUILayout.HelpBox("Cognitive3D Preferences asset not found!", MessageType.Error);
                    return;
                }

                // Draw and allow replacing the preferences asset
                EditorGUI.BeginChangeCheck();
                var newPrefs = (Cognitive3D_Preferences)EditorGUILayout.ObjectField(
                    "Preferences Asset", currentPrefs, typeof(Cognitive3D_Preferences), false);
                if (EditorGUI.EndChangeCheck() && newPrefs != null && newPrefs != currentPrefs)
                {
                    EditorCore.SetPreferences(newPrefs);
                    currentPrefs = newPrefs;
                }

                EditorGUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create New"))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Create New Cognitive3D Preferences",
                        "Cognitive3D_Preferences",
                        "asset",
                        "Choose a location for the new preferences asset");

                    newPrefs = EditorCore.CreatePreferences(path);
                    EditorCore.SetPreferences(newPrefs);
                    currentPrefs = newPrefs;
                }
                if (GUILayout.Button("Copy Current"))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Copy Preferences",
                        "Cognitive3D_Preferences_Copy",
                        "asset",
                        "Choose a location to save the copied preferences asset");

                    newPrefs = EditorCore.CopyPreferences(path, currentPrefs);
                    EditorCore.SetPreferences(newPrefs);
                    currentPrefs = newPrefs;
                }
                if (GUILayout.Button("Save"))
                {
                    if (currentPrefs == null)
                    {
                        Debug.LogError("No current preferences selected.");
                        return;
                    }

                    // Copy serialized data
                    EditorCore.SaveToPreference(currentPrefs);
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.LabelField("Current Preferences Asset", EditorCore.styles.IssuesTitleBoldLabel);
                EditorGUILayout.Space(5);

                // Use existing preferences inspector UI
                var editor = Editor.CreateEditor(currentPrefs, typeof(PreferencesInspector));
                if (editor != null)
                {
                    editor.OnInspectorGUI();
                }
            }
        }
    }
}
