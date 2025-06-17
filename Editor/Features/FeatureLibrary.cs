using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    [InitializeOnLoad]
    internal static class FeatureLibrary
    {
        internal enum FeatureActionType
        {
            Apply,
            Remove,
            Upload,
            LinkTo,
            Settings
        }

        internal static int projectID;
        private static int userID;

        internal static List<FeatureData> CreateFeatures(System.Action<int> setFeatureIndex)
        {
            if (!string.IsNullOrEmpty(EditorCore.DeveloperKey))
            {
                EditorCore.GetUserData(EditorCore.DeveloperKey, GetUserResponse);
            }

            return new List<FeatureData>
            {
                new FeatureData(
                    "Dynamic Objects",
                    "Learn how to use reusable components to construct immersive XR experiences.",
                    EditorCore.DynamicsIcon,
                    () => { setFeatureIndex(0); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Add Dynamic Object component to selected game objects",
                            () =>
                            {
                                DynamicObjectDetailGUI.AttachDynamicObjectToSelected();
                            }
                        ),
                        new FeatureAction(
                            FeatureActionType.Upload,
                            "Upload all Dynamic Objects in the scene",
                            () =>
                            {
                                UploadTools.ExportAndUploadAllDynamicsInScene();
                            }
                        )
                    },
                    new DynamicObjectDetailGUI()
                ),
                new FeatureData(
                    "Exit Poll",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.ExitpollIcon,
                    () => { setFeatureIndex(1); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Adds ExitPoll folder to Assets folder",
                            () =>
                            {
                                ExitpollDetailGUI.ImportExitPollSampleWithOptionalPrefab(true);
                            }
                        ),
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to create/modify hooks on dashboard",
                            () =>
                            {
                                if (projectID != 0)
                                {
                                    Application.OpenURL(CognitiveStatics.GetExitPollSettingsUrl(projectID));
                                }
                            }
                        )
                    },
                    new ExitpollDetailGUI()
                ),
                new FeatureData(
                    "Remote Controls",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.RemoteControlsIcon,
                    () => { setFeatureIndex(2); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Adds/Removes Remote Controls component to Cognitive3D_Manager prefab",
                            () =>
                            {
                                AddOrRemoveComponent<Cognitive3D.Components.RemoteControls>();
                            }
                        ),
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to create/modify remote controls on dashboard",
                            () =>
                            {
                                if (projectID != 0)
                                {
                                    Application.OpenURL(CognitiveStatics.GetRemoteControlsSettingsUrl(projectID));
                                }
                            }
                        )
                    },
                    new RemoteControlsDetailGUI(),
                    () => TryGetComponent<Cognitive3D.Components.RemoteControls>()
                ),
                new FeatureData(
                    "Eye Tracking",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.EyeTrackingIcon,
                    () => { setFeatureIndex(3); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Adds Eye Tracking (Fixation) component to Cognitive3D_Manager prefab",
                            () =>
                            {
                                AddOrRemoveComponent<FixationRecorder>();
                            }
                        )
                    },
                    new EyeTrackingDetailGUI(),
                    () => TryGetComponent<FixationRecorder>()
                ),
                new FeatureData(
                    "Oculus Social",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.CustomEventIcon,
                    () => { setFeatureIndex(4); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Adds Oculus Social to Cognitive3D_Manager prefab",
                            () =>
                            {
                                AddOrRemoveComponent<Cognitive3D.Components.OculusSocial>();
                            }
                        )
                    },
                    new MediaDetailGUI(),
                    () => TryGetComponent<Cognitive3D.Components.OculusSocial>()
                ),
                new FeatureData(
                    "Custom Events",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.CustomEventIcon,
                    () => { setFeatureIndex(5); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to Custom Events docs",
                            () =>
                            {

                            }
                        )
                    },
                    new CustomEventDetailGUI()
                ),
                new FeatureData(
                    "Media (360 Video)",
                    "Media related context",
                    EditorCore.DynamicsIcon,
                    () => { setFeatureIndex(6); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.Apply,
                            "Add Media components (Dynamic Object and Mesh Collider) to the selected GameObjects",
                            () =>
                            {
                                GameObject[] selectedObjects = Selection.gameObjects;

                                foreach (GameObject obj in selectedObjects)
                                {
                                    if (!obj.GetComponent<MediaComponent>())
                                    {
                                        obj.AddComponent<MediaComponent>();
                                    }
                                }
                            }
                        ),
                        new FeatureAction(
                            FeatureActionType.Settings,
                            "Media settings",
                            () =>
                            {
                                setFeatureIndex(5);
                            }
                        ),
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to Media on dashboard",
                            () =>
                            {
                                if (projectID != 0)
                                {
                                    Application.OpenURL(CognitiveStatics.GetMediaSettingsUrl(projectID));
                                }
                            }
                        )
                    },
                    new MediaDetailGUI()
                )
            };
        }

        private static void GetUserResponse(int responseCode, string error, string text)
        {
            var userdata = JsonUtility.FromJson<EditorCore.UserData>(text);
            if (responseCode != 200)
            {
                Util.logDevelopment("Failed to retrieve user data" + responseCode + "  " + error);
            }

            if (responseCode == 200 && userdata != null)
            {
                userID = userdata.userId;
                projectID = userdata.projectId;
            }
        }

        internal static void AddOrRemoveComponent<T>() where T : Component
        {
            GameObject c3dPrefab = Resources.Load<GameObject>("Cognitive3D_Manager");

            if (c3dPrefab == null)
            {
                Debug.LogError("Cognitive3D Manager prefab not found in Resources folder!");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(c3dPrefab);

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(assetPath);

            if (prefabContents.GetComponent<T>() != null)
            {
                Object.DestroyImmediate(prefabContents.GetComponent<T>());
            }
            else
            {
                prefabContents.AddComponent<T>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            AssetDatabase.Refresh();
        }

        internal static bool TryGetComponent<T>() where T : Component
        {
            GameObject c3dPrefab = Resources.Load<GameObject>("Cognitive3D_Manager");

            if (c3dPrefab == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(c3dPrefab);

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(assetPath);

            bool hasComponent = prefabContents.GetComponent<T>() != null;

            PrefabUtility.UnloadPrefabContents(prefabContents);

            return hasComponent;
        }
    }

    internal class FeatureData
    {
        internal string Title;
        internal string Description;
        internal List<string> Tags;
        internal Texture2D Icon;
        internal System.Action OnClick;
        internal System.Func<bool> IsApplied;

        internal List<FeatureAction> Actions;

        internal IFeatureDetailGUI DetailGUI;

        internal FeatureData(string title, string description, Texture2D icon, System.Action onClick, List<FeatureAction> actions, IFeatureDetailGUI detailGUI = null, System.Func<bool> isApplied = null)
        {
            Title = title;
            Description = description;
            Icon = icon;
            OnClick = onClick;
            Actions = actions ?? new List<FeatureAction>();
            DetailGUI = detailGUI;
            IsApplied = isApplied;
        }
    }

    internal class FeatureAction
    {
        internal FeatureLibrary.FeatureActionType Type;
        internal string Tooltip;
        internal System.Action OnClick;

        internal FeatureAction(FeatureLibrary.FeatureActionType type, string tooltip, System.Action onClick)
        {
            Type = type;
            Tooltip = tooltip;
            OnClick = onClick;
        }
    }
}
