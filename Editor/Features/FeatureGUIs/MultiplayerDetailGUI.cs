using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    public class MultiplayerDetailGUI : IFeatureDetailGUI
    {
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Multiplayer", EditorCore.styles.FeatureTitleStyle);

                float iconSize = EditorGUIUtility.singleLineHeight;
                Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize), GUILayout.Height(iconSize));

                if (GUI.Button(iconRect, EditorCore.InfoGrey, EditorCore.styles.InfoButton))
                {
                    Application.OpenURL("https://docs.cognitive3d.com/unity/multiplayer/");
                }

                GUILayout.FlexibleSpace(); // Push content to the left
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(
                "This feature adds extra analytics based on the multiplayer packages used in the project. It includes custom events for player connections and disconnections, as well as sensor data like Round-Trip Time (RTT/ping), and more.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space(10);

            GUILayout.Label("Add to Cognitive3D_Manager prefab", EditorCore.styles.FeatureTitleStyle);
            GUILayout.Label("Adds the necessary components to the Cognitive3D_Manager prefab.", EditorStyles.wordWrappedLabel);

#if PHOTON_UNITY_NETWORKING
            EditorGUILayout.HelpBox("Photon PUN 2 package was detected in your project.", MessageType.Info);
            var btnLabel = FeatureLibrary.TryGetComponent<Cognitive3D.Components.PhotonMultiplayer>() ? "Remove Photon PUN 2 Support" : "Add Photon PUN 2 Support";
            if (GUILayout.Button(btnLabel, GUILayout.Height(30)))
            {
                FeatureLibrary.AddOrRemoveComponent<Cognitive3D.Components.PhotonMultiplayer>();
                FeatureLibrary.AddOrRemoveComponent<Photon.Pun.PhotonView>();
            }
#elif COGNITIVE3D_INCLUDE_UNITY_NETCODE
            EditorGUILayout.HelpBox("Unity Netcode for Gameobjects package was detected in your project.", MessageType.Info);
            var btnLabel = FeatureLibrary.TryGetComponent<Cognitive3D.Components.NetcodeMultiplayer>() ? "Remove Unity Netcode for Gameobjects Support" : "Add Unity Netcode for Gameobjects Support";
            if (GUILayout.Button(btnLabel, GUILayout.Height(30)))
            {
                FeatureLibrary.AddOrRemoveComponent<Cognitive3D.Components.NetcodeMultiplayer>();
                FeatureLibrary.AddOrRemoveComponent<Unity.Netcode.NetworkObject>();
            }
#elif COGNITIVE3D_INCLUDE_NORMCORE
            EditorGUILayout.HelpBox("NormCore package was detected in your project.", MessageType.Info);
            var btnLabel = FeatureLibrary.TryGetComponent<Cognitive3D.Components.NormcoreMultiplayer>() ? "Remove NormCore Support" : "Add NormCore Support";
            if (GUILayout.Button(btnLabel, GUILayout.Height(30)))
            {
                FeatureLibrary.AddOrRemoveComponent<Cognitive3D.Components.NormcoreMultiplayer>();

                if (Cognitive3D_Manager.Instance.gameObject.GetComponent<Cognitive3D.Components.NormcoreMultiplayer>() != null)
                {
                    string localPath = "Assets/Resources/Cognitive3D_NormcoreSync.prefab";

                    string folderPath = System.IO.Path.GetDirectoryName(localPath);
                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    if (!Resources.Load<GameObject>("Cognitive3D_NormcoreSync"))
                    {
                        GameObject c3dNormcoreSyncObject = new GameObject("Cognitive3D_NormcoreSync");
                        PrefabUtility.SaveAsPrefabAsset(c3dNormcoreSyncObject, localPath);
                        Object.DestroyImmediate(c3dNormcoreSyncObject);
                    }

                    GameObject c3dNormcoreSyncPrefab = Resources.Load<GameObject>("Cognitive3D_NormcoreSync");
                    if (!c3dNormcoreSyncPrefab.GetComponent<Cognitive3D.NormcoreSync>())
                    {
                        c3dNormcoreSyncPrefab.AddComponent<Cognitive3D.NormcoreSync>();
                    }
                }
            }
#else
            EditorGUILayout.HelpBox("No multiplayer framework or package was detected in your project. Please make sure it's installed.", MessageType.Error);
            GUI.enabled = false;
            if (GUILayout.Button("Add Multiplayer Support", GUILayout.Height(30)))
            {
                // This won't be triggered while disabled
            }
            GUI.enabled = true;
#endif
        }
    }
}
