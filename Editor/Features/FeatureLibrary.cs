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
                            FeatureActionType.LinkTo,
                            "Link to Dynamic Object documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/dynamic-objects/");
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
                            FeatureActionType.LinkTo,
                            "Link to ExitPoll Survey documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/exitpoll/");
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
                            FeatureActionType.LinkTo,
                            "Link to Remote Controls documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/remote-controls/");
                            }
                        )
                    },
                    new RemoteControlsDetailGUI()
                ),
                new FeatureData(
                    "Oculus Social",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.CustomEventIcon,
                    () => { setFeatureIndex(3); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to Remote Controls documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/components/#oculus-social-data");
                            }
                        )
                    },
                    new MediaDetailGUI()
                ),
                new FeatureData(
                    "Custom Events",
                    "Visualize player journeys and scene engagement metrics in real time.",
                    EditorCore.CustomEventIcon,
                    () => { setFeatureIndex(4); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to Oculus Social Data documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/customevents/");
                            }
                        )
                    },
                    new CustomEventDetailGUI()
                ),
                new FeatureData(
                    "Media and 360 Video",
                    "Media related context",
                    EditorCore.DynamicsIcon,
                    () => { setFeatureIndex(5); },
                    new List<FeatureAction>
                    {
                        new FeatureAction(
                            FeatureActionType.LinkTo,
                            "Link to Media & 360 Video documentation",
                            () =>
                            {
                                Application.OpenURL("https://docs.cognitive3d.com/unity/media/");
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
