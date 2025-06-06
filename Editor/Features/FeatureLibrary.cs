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
            Upload,
            LinkTo,
            Settings
        }

        private static int projectID;
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
                                GameObject[] selectedObjects = Selection.gameObjects;

                                foreach (GameObject obj in selectedObjects)
                                {
                                    if (!obj.GetComponent<DynamicObject>())
                                    {
                                        obj.AddComponent<DynamicObject>();
                                    }
                                }
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
                                var packageName = "com.cognitive3d.c3d-sdk";
                                var sampleName = "Exitpoll Customization";

                                var samples = UnityEditor.PackageManager.UI.Sample.FindByPackage(packageName, null);

                                foreach (var sample in samples)
                                {
                                    if (sample.displayName == sampleName)
                                    {
                                        if (!sample.isImported)
                                        {
                                            sample.Import();
                                            Debug.Log($"Imported sample: {sample.displayName}");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"{sample.displayName} sample already imported. Path: {sample.importPath}");
                                        }

                                        if (GameObject.FindAnyObjectByType<ExitPollHolder>() == null)
                                        {
                                            string fullPath = sample.importPath + "/ExitPollHolderPrefab.prefab";

                                            // Normalize slashes
                                            fullPath = fullPath.Replace("\\", "/");

                                            // Find where "Assets" starts in the full path
                                            int assetsIndex = fullPath.IndexOf("Assets/");
                                            if (assetsIndex == -1)
                                            {
                                                Debug.LogError("Could not find 'Assets/' in path: " + fullPath);
                                                return;
                                            }

                                            string assetRelativePath = fullPath.Substring(assetsIndex);

                                            // Load and instantiate
                                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetRelativePath);
                                            if (prefab == null)
                                            {
                                                Debug.LogError("Could not find prefab at: " + assetRelativePath);
                                                return;
                                            }

                                            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                                            Undo.RegisterCreatedObjectUndo(instance, "Add ExitPoll Prefab");
                                            Selection.activeObject = instance;              
                                        }

                                        return;
                                    }
                                }

                                Debug.LogError("Exitpoll Customization sample not found!");
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
                    new MediaDetailGUI()
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
                            "Adds Remote Controls component to Cognitive3D_Manager prefab",
                            () =>
                            {

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
                    new MediaDetailGUI()
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

                            }
                        )
                    },
                    new MediaDetailGUI()
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

                            }
                        )
                    },
                    new MediaDetailGUI()
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
                    new MediaDetailGUI()
                ),
                new FeatureData(
                    "Media (360 Video)",
                    "Media related context",
                    EditorCore.DynamicsIcon,
                    () => { setFeatureIndex(5); },
                    new List<FeatureAction>
                    {
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
    }

    internal class FeatureData
    {
        internal string Title;
        internal string Description;
        internal Texture2D Icon;
        internal System.Action OnClick;

        internal List<FeatureAction> Actions;

        internal IFeatureDetailGUI DetailGUI;

        internal FeatureData(string title, string description, Texture2D icon, System.Action onClick, List<FeatureAction> actions, IFeatureDetailGUI detailGUI = null)
        {
            Title = title;
            Description = description;
            Icon = icon;
            OnClick = onClick;
            Actions = actions ?? new List<FeatureAction>();
            DetailGUI = detailGUI;
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
