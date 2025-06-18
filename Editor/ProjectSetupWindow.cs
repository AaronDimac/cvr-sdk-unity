using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cognitive3D
{
    [InitializeOnLoad]
    public class ProjectSetupWindow : EditorWindow
    {
        bool keysSet = false;
        private string developerKey;
        private string apiKey;

        #region Project Setup Window
        public static void Init()
        {
            ProjectSetupWindow setup = GetWindow<ProjectSetupWindow>("Project Setup");
            setup.minSize = new Vector2(600, 800);
            setup.maxSize = new Vector2(600, 800);
            setup.LoadKeys();
        }

        private void OnEnable()
        {
            if (autoSelectXR)
            {
                AutoSelectXRSDK();
            }

            CacheCurrentScenes();
            UploadTools.OnUploadScenesComplete += CacheCurrentScenes;
            EditorApplication.update += CheckForChanges;
        }

        private void OnDisable()
        {
            CacheCurrentScenes();
            UploadTools.OnUploadScenesComplete -= CacheCurrentScenes;
            EditorApplication.update -= CheckForChanges;
        }
        #endregion

        private Vector2 mainScroll;

        int lastDevKeyResponseCode;
        private bool forceUpdateApiKey = false;
        private string apiKeyFromDashboard = "";

        bool autoSelectXR = true;
        bool previousAutoSelectXR = false;
        int selectedSDKIndex = 0;
        string[] availableXrSdks = new string[] { "MetaXR", "PicoXR", "ViveWave", "SteamVR/OpenVR", "Omnicept", "Default" };
        string selectedSDKName = "C3D_DEFAULT";
        
        bool autoPlayerSetup = true;
        GameObject hmd;
        GameObject trackingSpace;
        GameObject rightController;
        GameObject leftController;

        private Vector2 scrollPos;

        private bool selectAll;
        private List<SceneEntry> sceneEntries = new List<SceneEntry>();

        private void OnGUI()
        {
            // Header background and logo
            if (EditorCore.LogoTexture != null)
            {
                float bgHeight = 100f;
                Rect bgRect = GUILayoutUtility.GetRect(position.width, bgHeight);
                GUI.DrawTexture(bgRect, EditorCore.BackgroundTexture, ScaleMode.ScaleAndCrop);

                float logoWidth = EditorCore.LogoTexture.width / 3f;
                float logoHeight = EditorCore.LogoTexture.height / 3f;
                float logoX = bgRect.x + (bgRect.width - logoWidth) / 2f;
                float logoY = bgRect.y + (bgRect.height - logoHeight) / 2f;
                GUI.DrawTexture(new Rect(logoX, logoY, logoWidth, logoHeight), EditorCore.LogoTexture, ScaleMode.ScaleToFit);
            }

            // Calculate heights
            float footerHeight = 60f;
            float contentHeight = position.height - footerHeight;

            // Scrollable content area
            Rect contentRect = new Rect(0, 100, position.width, contentHeight - 100); // below logo
            GUILayout.BeginArea(contentRect);
            mainScroll = GUILayout.BeginScrollView(mainScroll);

            bool completenessStatus;
            Texture2D statusIcon;

            using (new EditorGUILayout.VerticalScope(EditorCore.styles.ContextPadding))
            {
                GUILayout.Space(5);
                GUILayout.Label(
                    "Welcome to the " + EditorCore.DisplayValue(DisplayKey.FullName) + " SDK Project Setup! This window will guide you through setting up our SDK in your project and ensuring the features available from packages in your project are automatically recorded.\n\nAt the end of this setup process, you will have production ready analytics and a method to replay individual sessions",
                    EditorCore.styles.ItemDescription);

                #region Dev and App keys
                completenessStatus = !string.IsNullOrEmpty(developerKey) && !string.IsNullOrEmpty(apiKey);
                statusIcon = GetStatusIcon(completenessStatus);

                DrawFoldout("Developer and App Keys", statusIcon, () =>
                {
                    GUILayout.Label("Enter your developer key:", EditorCore.styles.DescriptionPadding);
                    developerKey = EditorGUILayout.TextField("Developer Key", developerKey);
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();

                    Rect apiKeyRect = EditorGUILayout.GetControlRect();

                    // Draw with EditorGUI to retain full control
                    apiKey = EditorGUI.TextField(apiKeyRect, "Application Key", forceUpdateApiKey ? apiKeyFromDashboard : apiKey);

                    if (forceUpdateApiKey)
                    {
                        forceUpdateApiKey = false;
                        GUI.FocusControl(null); // Optionally clear focus
                    }

                    if (GUILayout.Button("Get from Dashboard", GUILayout.Width(130)))
                    {
                        EditorCore.CheckForExpiredDeveloperKey(GetDevKeyResponse);
                        EditorCore.CheckForApplicationKey(developerKey, GetApplicationKeyResponse);
                        EditorCore.CheckSubscription(developerKey, GetSubscriptionResponse);

                        forceUpdateApiKey = true;
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);
                });
                #endregion

                EditorGUI.BeginDisabledGroup(!keysSet);
                #region XR SDK
                completenessStatus = autoSelectXR;
                statusIcon = GetStatusIcon(completenessStatus);

                DrawFoldout("XR SDK Setup", statusIcon, () =>
                {
                    bool newAutoSelectXR = EditorGUILayout.Toggle("Auto-select XR SDK", autoSelectXR);

                    if (newAutoSelectXR && newAutoSelectXR != previousAutoSelectXR)
                    {
                        AutoSelectXRSDK();
                    }

                    autoSelectXR = newAutoSelectXR;
                    previousAutoSelectXR = newAutoSelectXR;

                    using (new EditorGUI.DisabledScope(autoSelectXR))
                    {
                        selectedSDKIndex = EditorGUILayout.Popup("Select XR SDK", selectedSDKIndex, availableXrSdks);
                    }

                    EditorGUILayout.HelpBox($"Current SDK: {availableXrSdks[selectedSDKIndex]}", MessageType.Info);
                });
                #endregion

                #region Player Setup
                completenessStatus = autoPlayerSetup;
                statusIcon = GetStatusIcon(completenessStatus);

                DrawFoldout("Player Setup", statusIcon, () =>
                {
                    GUILayout.Label(
                        "Use your existing Player Prefab to assign tracked objects. Enable auto-setup for automatic detection, or disable it to assign manually.",
                        EditorCore.styles.DescriptionPadding);

                    autoPlayerSetup = EditorGUILayout.Toggle("Auto Player Setup", autoPlayerSetup);

                    if (!autoPlayerSetup)
                    {
                        hmd = (GameObject)EditorGUILayout.ObjectField("HMD", hmd, typeof(GameObject), true);
                        trackingSpace = (GameObject)EditorGUILayout.ObjectField("Tracking Space", trackingSpace, typeof(GameObject), true);
                        rightController = (GameObject)EditorGUILayout.ObjectField("Right Controller", rightController, typeof(GameObject), true);
                        leftController = (GameObject)EditorGUILayout.ObjectField("Left Controller", leftController, typeof(GameObject), true);

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Set Player", GUILayout.Width(100)))
                        {
                            EditorCore.SetMainCamera(hmd);
                            EditorCore.SetTrackingSpace(trackingSpace);
                            EditorCore.SetController(true, rightController);
                            EditorCore.SetController(false, leftController);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }
                });
                #endregion

                #region Scene Upload
                completenessStatus = Cognitive3D_Preferences.Instance.sceneSettings.Count > 0;
                statusIcon = GetStatusIcon(completenessStatus);
                
                DrawFoldout("Scene Upload", statusIcon, () =>
                {
                    GUILayout.Label("Configure which scenes should be prepared and uploaded.", EditorCore.styles.DescriptionPadding);
                    EditorGUILayout.BeginVertical(EditorCore.styles.ListBoxPadding);

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    // Select All Toggle
                    bool newSelectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(40));
                    if (newSelectAll != selectAll)
                    {
                        selectAll = newSelectAll;
                        foreach (var scene in sceneEntries)
                        {
                            scene.selected = selectAll;
                        }
                    }
                    DrawColumnSeparator();

                    // Column Headers
                    GUILayout.Label("Scene Name", EditorCore.styles.leftPaddingBoldLabel, GUILayout.Width(185));
                    DrawColumnSeparator();
                    GUILayout.Label("Version Number", EditorCore.styles.leftPaddingBoldLabel);

                    // Flexible space to push icon to the right
                    GUILayout.FlexibleSpace();

                    // Gear Icon Button
                    if (GUILayout.Button(new GUIContent(EditorCore.SettingsIcon2, "Additional Settings"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                    {
                        GenericMenu gm = new GenericMenu();
                        gm.AddItem(new GUIContent("Full Texture Resolution"), Cognitive3D_Preferences.Instance.TextureResize == 1, OnSelectFullResolution);
                        gm.AddItem(new GUIContent("Half Texture Resolution"), Cognitive3D_Preferences.Instance.TextureResize == 2, OnSelectHalfResolution);
                        gm.AddItem(new GUIContent("Quarter Texture Resolution"), Cognitive3D_Preferences.Instance.TextureResize == 4, OnSelectQuarterResolution);
                        gm.AddSeparator("");
                        gm.AddItem(new GUIContent("Export lowest LOD meshes"), Cognitive3D_Preferences.Instance.ExportSceneLODLowest, OnToggleLODMeshes);

#if UNITY_2020_1_OR_NEWER
                        gm.AddItem(new GUIContent("Include Disabled Dynamic Objects"), Cognitive3D_Preferences.Instance.IncludeDisabledDynamicObjects, OnToggleIncludeDisabledDynamics);
#endif
                        gm.ShowAsContext();
                    }

                    EditorGUILayout.EndHorizontal();

                    // Scrollable list of scenes
                    scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));

                    for (int i = 0; i < sceneEntries.Count; i++)
                    {
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneEntries[i].path);

                        EditorGUILayout.BeginHorizontal();
                        sceneEntries[i].selected = EditorGUILayout.Toggle(sceneEntries[i].selected, GUILayout.Width(40));

                        GUILayout.Space(5);
                        GUILayout.Label(sceneName, EditorCore.styles.leftPaddingLabel, GUILayout.Width(185));

                        GUILayout.Label(sceneEntries[i].versionNumber.ToString(), EditorCore.styles.leftPaddingLabel);
                        EditorGUILayout.EndHorizontal();
                    }

                    bool allSelected = true;
                    foreach (var scene in sceneEntries)
                    {
                        if (!scene.selected)
                        {
                            allSelected = false;
                            break;
                        }
                    }

                    if (selectAll != allSelected)
                    {
                        selectAll = allSelected;
                    }

                    GUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                });
                #endregion

                EditorGUI.EndDisabledGroup();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Sticky footer button
            Rect footerRect = new Rect(0, position.height - footerHeight, position.width, footerHeight);
            GUILayout.BeginArea(footerRect, EditorStyles.helpBox);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Upload and Finish", GUILayout.Width(120), GUILayout.Height(30)))
            {
                // Upload scenes
                var selectedScenes = UploadTools.GetSelectedScenes(sceneEntries);
                UploadTools.UploadScenes(selectedScenes);

                // Set the XR SDK scripting define
                SetXRSDK();

                // Start tracking compile progress after setting the define
                compileStartTime = -1; // reset timer for new compile
                EditorApplication.update += WaitForCompileAndUpload;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndArea();
        }

        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        private void DrawFoldout(string title, Texture2D icon, Action drawContent)
        {
            if (!foldoutStates.ContainsKey(title))
            {
                if (title == "Developer and App Keys")
                {
                    foldoutStates[title] = true;
                }
                else
                {
                    foldoutStates[title] = keysSet;
                }
            }

            using (var scope = new EditorGUILayout.VerticalScope(EditorCore.styles.List))
                {
                    GUILayout.BeginHorizontal();
                    foldoutStates[title] = EditorGUILayout.Foldout(foldoutStates[title], title, true);
                    if (icon != null)
                    {
                        GUILayout.Label(icon, EditorCore.styles.InlinedIconStyle);
                    }
                    GUILayout.EndHorizontal();

                    if (foldoutStates[title])
                    {
                        using (new EditorGUILayout.VerticalScope(EditorCore.styles.ListLabel))
                        {
                            EditorGUI.indentLevel++;
                            drawContent?.Invoke();
                            EditorGUI.indentLevel--;
                        }
                    }
                }
        }

        Texture2D GetStatusIcon(bool condition)
        {
            return condition ? EditorCore.CompleteCheckmark : EditorCore.CircleWarning;
        }

        private void DrawColumnSeparator()
        {
            var rect = GUILayoutUtility.GetRect(1, 18, GUILayout.Width(1));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }

        #region Developer and App Key Utilities
        private void LoadKeys()
        {
            developerKey = EditorCore.DeveloperKey;
            apiKey = EditorCore.GetPreferences().ApplicationKey;

            if (!string.IsNullOrEmpty(developerKey) && !string.IsNullOrEmpty(apiKey))
            {
                keysSet = true;
            }

            EditorCore.RefreshSceneVersionComplete += CacheCurrentScenes;
        }

        private void SaveDevKey()
        {
            EditorCore.DeveloperKey = developerKey;

            if (!string.IsNullOrEmpty(developerKey) && !string.IsNullOrEmpty(apiKey))
            {
                keysSet = true;
            }
        }

        private void SaveApplicationKey()
        {
            EditorCore.GetPreferences().ApplicationKey = apiKey;
            EditorUtility.SetDirty(EditorCore.GetPreferences());
            AssetDatabase.SaveAssets();
        }
        #endregion

#region XR SDK Utilities
        [System.NonSerialized]
        double compileStartTime = -1;
        void SetXRSDK()
        {
            selectedSDKName = SetC3DPlayerDefineName(availableXrSdks[selectedSDKIndex]);
            EditorCore.SetPlayerDefine(selectedSDKName);

            if (compileStartTime < 0)
            {
                compileStartTime = EditorApplication.timeSinceStartup;
            }
        }

        private void AutoSelectXRSDK()
        {
            EditorCore.GetPackages(OnGetPackages);
        }

        string packageName;
        void OnGetPackages(UnityEditor.PackageManager.PackageCollection packages)
        {
            //search from specific sdks (single headset support) to general runtimes (openvr, etc)
            foreach(var package in packages)
            {
                if (package.name == "com.unity.xr.picoxr")
                {
                    packageName = "PicoXR";
                    selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                    return;
                }
                if (package.name == "com.varjo.xr")
                {
                    packageName = "VarjoXR";
                    selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                    return;
                }
                if (package.name == "com.htc.upm.wave.xrsdk")
                {
                    packageName = "ViveWave";
                    selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                    return;
                }
            }

            //specific assets
            var SRAnipalAssets = AssetDatabase.FindAssets("SRanipal");
            if (SRAnipalAssets.Length > 0)
            {
                packageName = "SRAnipal";
                selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                return;
            }

            var GliaAssets = AssetDatabase.FindAssets("lib-client-csharp");
            if (GliaAssets.Length > 0)
            {
                packageName = "Omnicept";
                selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                return;
            }

            var OculusIntegrationAssets = AssetDatabase.FindAssets("t:assemblydefinitionasset oculus.vr");
            if (OculusIntegrationAssets.Length > 0)
            {
                packageName = "MetaXR";
                selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                return;
            }

            var HololensAssets = AssetDatabase.FindAssets("WindowsMRAssembly");
            if (HololensAssets.Length > 0)
            {
                packageName = "MRTK";
                selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                return;
            }

            //general packages
            foreach (var package in packages)
            {
                if (package.name == "com.openvr")
                {
                    packageName = "SteamVR/OpenVR";
                    selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                    return;
                }
            }

            var SteamVRAssets = AssetDatabase.FindAssets("t:assemblydefinitionasset steamvr");
            if (SteamVRAssets.Length > 0)
            {
                packageName = "SteamVR/OpenVR";
                selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
                return;
            }

            //default fallback
            packageName = "Default";
            selectedSDKIndex = Array.IndexOf(availableXrSdks, packageName);
            return;
        }

        string SetC3DPlayerDefineName(string name)
        {
            switch (name)
            {
                case "MetaXR":
                    return "C3D_OCULUS";
                case "ViveWave":
                    return "C3D_VIVEWAVE";
                case "PicoXR":
                    return "C3D_PICOXR";
                case "SteamVR/OpenVR":
                    return "C3D_STEAMVR2";
                case "SRAnipal":
                    return "C3D_SRANIPAL";
                case "MRTK":
                    return "C3D_MRTK";
                case "Omnicept":
                    return "C3D_OMNICEPT";
                case "VarjoXR":
                    return "C3D_VARJOXR";
                default:
                    return "C3D_DEFAULT";
            }
        }

        void WaitForCompileAndUpload()
        {
            if (EditorApplication.isCompiling)
            {
                // Simulate progress based on elapsed time
                float elapsed = (float)(EditorApplication.timeSinceStartup - compileStartTime);
                float progress = Mathf.Clamp(Mathf.Log10(elapsed + 1), 0.05f, 0.95f);
                EditorUtility.DisplayProgressBar("Compiling", $"Setting player definition...", progress);
                return;
            }

            // Done compiling
            EditorApplication.update -= WaitForCompileAndUpload;
            EditorUtility.ClearProgressBar();
            compileStartTime = -1;
        }
        #endregion
        #region Build Setting Scene Utilities
        private string[] cachedScenePaths;

        void CacheCurrentScenes()
        {
            cachedScenePaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            sceneEntries.Clear();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                var c3dScene = Cognitive3D_Preferences.FindSceneByPath(scene.path);
                int versionNumber = c3dScene?.VersionNumber ?? 0;

                bool isSelected = versionNumber == 0;

                var sceneEntry = new SceneEntry(
                    scene.path,
                    versionNumber,
                    isSelected,
                    true
                );

                sceneEntries.Add(sceneEntry);
            }
        }

        void CheckForChanges()
        {
            var currentPaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            if (!cachedScenePaths.SequenceEqual(currentPaths))
            {
                CacheCurrentScenes();
            }
        }

        void OnSelectFullResolution()
        {
            Cognitive3D_Preferences.Instance.TextureResize = 1;
        }
        void OnSelectHalfResolution()
        {
            Cognitive3D_Preferences.Instance.TextureResize = 2;
        }
        void OnSelectQuarterResolution()
        {
            Cognitive3D_Preferences.Instance.TextureResize = 4;
        }
        void OnToggleLODMeshes()
        {
            Cognitive3D_Preferences.Instance.ExportSceneLODLowest = !Cognitive3D_Preferences.Instance.ExportSceneLODLowest;
        }

        //Unity 2020.1+
        void OnToggleIncludeDisabledDynamics()
        {
            Cognitive3D_Preferences.Instance.IncludeDisabledDynamicObjects = !Cognitive3D_Preferences.Instance.IncludeDisabledDynamicObjects;
        }
        #endregion
        #region Callback Responses
        void GetDevKeyResponse(int responseCode, string error, string text)
        {
            lastDevKeyResponseCode = responseCode;
            if (responseCode == 200)
            {
                //dev key is fine
                SaveDevKey();
            }
            else
            {
                Debug.LogError("Developer Key invalid or expired: " + error);
            }
        }

        private void GetApplicationKeyResponse(int responseCode, string error, string text)
        {
            if (responseCode != 200)
            {
                SegmentAnalytics.TrackEvent("InvalidDevKey_ProjectSetup_" + responseCode, "ProjectSetupAPIPage");
                Debug.LogError("GetApplicationKeyResponse response code: " + responseCode + " error: " + error);
                return;
            }

            // Check if response data is valid
            try
            {
                JsonUtility.FromJson<EditorCore.ApplicationKeyResponseData>(text);
                SegmentAnalytics.TrackEvent("ValidDevKey_ProjectSetup", "ProjectSetupAPIPage");
            }
            catch
            {
                Debug.LogError("Invalid JSON response");
                return;
            }

            EditorCore.ApplicationKeyResponseData responseData = JsonUtility.FromJson<EditorCore.ApplicationKeyResponseData>(text);

            //display popup if application key is set but doesn't match the response
            if (!string.IsNullOrEmpty(apiKey) && apiKey != responseData.apiKey)
            {
                SegmentAnalytics.TrackEvent("APIKeyMismatch_ProjectSetup", "ProjectSetupAPIPage");
                var result = EditorUtility.DisplayDialog("Application Key Mismatch", "Do you want to use the latest Application Key available on the Dashboard?", "Ok", "No");
                if (result)
                {
                    apiKey = responseData.apiKey;
                    apiKeyFromDashboard = apiKey;
                    SaveApplicationKey();
                }
            }
            else
            {
                SegmentAnalytics.TrackEvent("APIKeyFound_ProjectSetup", "ProjectSetupAPIPage");
                apiKey = responseData.apiKey;
                apiKeyFromDashboard = apiKey;
                SaveApplicationKey();
            }
        }

        private void GetSubscriptionResponse(int responseCode, string error, string text)
        {
            if (responseCode != 200)
            {
                Debug.LogError("GetSubscriptionResponse response code: " + responseCode + " error: " + error);
                return;
            }

            // Check if response data is valid
            try
            {
                JsonUtility.FromJson<EditorCore.OrganizationData>(text);
            }
            catch
            {
                Debug.LogError("Invalid JSON response");
                return;
            }
        }
#endregion
    }
}
