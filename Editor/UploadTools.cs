using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Cognitive3D
{
    internal static class UploadTools
    {
        #region Scene Upload Tools
        internal enum SceneManagementUploadState
        {
            /// <summary>
            /// We start here <br/>
            /// If target scene is open, we move to scene setup <br/>
            /// Otherwise, we open scene, and then move to scene setup
            /// </summary>
            Init,

            /// <summary>
            /// Perform basic scene setup
            /// </summary>
            SceneSetup,

            /// <summary>
            /// Sets up controllers
            /// </summary>
            GameObjectSetup,

            /// <summary>
            /// Export Scene
            /// </summary>
            Export,

            /// <summary>
            /// Wait for the delayed export operation to finish before starting upload
            /// </summary>
            WaitingForExportDelay,

            /// <summary>
            /// Begin upload process
            /// </summary>
            StartUpload,

            /// <summary>
            /// Wait for upload to complete
            /// </summary>
            Uploading,

            /// <summary>
            /// Complete process, cleanup
            /// </summary>
            Complete
        };

        static List<SceneEntry> entries = new List<SceneEntry>();

        /// <summary>
        /// If true, export dynamics from the scenes too <br/>
        /// Otherwise, just export the scenes
        /// </summary>
        static bool exportDynamics = false;

        /// <summary>
        /// Set to false until user clicks export button <br/>
        /// User's click sets it to true <br/>
        /// Once export complete, this variable is set back to false
        /// </summary>
        static bool isExporting = false;

        /// <summary>
        /// Used to track the current scene in the Update FSM
        /// </summary>
        static int sceneIndex = 0;

        internal static SceneManagementUploadState sceneUploadState = SceneManagementUploadState.Init;

        internal delegate void onUploadScenesComplete();
        /// <summary>
        /// Called just after a session has begun
        /// </summary>
        internal static event onUploadScenesComplete OnUploadScenesComplete;
        private static void InvokeUploadScenesCompleteEvent() { if (OnUploadScenesComplete != null) { OnUploadScenesComplete.Invoke(); } }

        internal static List<SceneEntry> GetSelectedScenes(List<SceneEntry> sceneEntries)
        {
            var selectedScenes = new List<SceneEntry>();
            foreach (var sceneEntry in sceneEntries)
            {
                if (sceneEntry.selected)
                {
                    selectedScenes.Add(sceneEntry);
                }
            }
            return selectedScenes;
        }

        internal static void UploadScenes(List<SceneEntry> scenes)
        {
            entries = scenes;
            sceneIndex = 0;
            isExporting = true;
            sceneUploadState = SceneManagementUploadState.Init;

            UnityEditor.EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (!isExporting)
                return;

            if (sceneIndex < entries.Count)
            {
                float progress = (float)sceneIndex / entries.Count;
                UnityEditor.EditorUtility.DisplayProgressBar(
                    "Uploading Scenes",
                    $"Processing {System.IO.Path.GetFileNameWithoutExtension(entries[sceneIndex].path)}...",
                    progress
                );
                UnityEditor.SceneView.RepaintAll();
            }

            if (sceneIndex > entries.Count - 1)
            {
                isExporting = false;
                InvokeUploadScenesCompleteEvent();
                UnityEditor.EditorApplication.update -= Update;
                UnityEditor.EditorUtility.ClearProgressBar();
                return;
            }

            if (!entries[sceneIndex].selected)
            {
                sceneIndex++;
                sceneUploadState = SceneManagementUploadState.Init;
                return;
            }

            switch (sceneUploadState)
            {
                case SceneManagementUploadState.Init:
                    if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path == entries[sceneIndex].path)
                    {
                        sceneUploadState = SceneManagementUploadState.SceneSetup;
                    }
                    else
                    {
                        var sceneEntry = entries[sceneIndex];
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneEntry.path);
                        sceneUploadState = SceneManagementUploadState.Export;
                    }
                    return;

                case SceneManagementUploadState.SceneSetup:
                    // Let Unity refresh editor before continuing
                    EditorApplication.delayCall += () =>
                    {
                        sceneUploadState = SceneManagementUploadState.Export;
                    };
                    return;

                case SceneManagementUploadState.Export:
                    EditorApplication.delayCall += () =>
                    {
                        string currentScenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                        var currentSettings = Cognitive3D_Preferences.FindSceneByPath(currentScenePath);

                        if (currentSettings == null)
                            Cognitive3D_Preferences.AddSceneSettings(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

                        ExportUtility.ExportGLTFScene();

                        string fullName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
                        string path = EditorCore.GetSubDirectoryPath(fullName);
                        ExportUtility.GenerateSettingsFile(path, fullName);
                        DebugInformationWindow.WriteDebugToFile(path + "debug.log");

                        UnityEditor.EditorUtility.SetDirty(EditorCore.GetPreferences());
                        UnityEditor.AssetDatabase.SaveAssets();

                        // Advance state after export completes
                        sceneUploadState = SceneManagementUploadState.StartUpload;
                    };
                    // Prevent loop from running repeatedly while waiting for delayCall
                    sceneUploadState = SceneManagementUploadState.WaitingForExportDelay;
                    return;

                case SceneManagementUploadState.WaitingForExportDelay:
                    // Do nothing — waiting for delayCall to complete
                    return;

                case SceneManagementUploadState.StartUpload:
                    SceneSetupWindow.CompletedUpload = false;
                    SceneSetupWindow.UploadSceneAndDynamics(exportDynamics, exportDynamics, true, true, false);
                    sceneUploadState = SceneManagementUploadState.Uploading;
                    return;

                case SceneManagementUploadState.Uploading:
                    if (SceneSetupWindow.CompletedUpload)
                    {
                        sceneUploadState = SceneManagementUploadState.Complete;
                    }
                    return;

                case SceneManagementUploadState.Complete:
                    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                    SceneSetupWindow.CompletedUpload = false;
                    sceneUploadState = SceneManagementUploadState.Init;
                    sceneIndex++;
                    return;
            }
        }
        #endregion

        #region Dynamic Objects Upload Tools
        internal static List<DynamicObject> GetDynamicObjectsInScene()
        {
            return GameObject.FindObjectsByType<DynamicObject>(FindObjectsSortMode.None).ToList();
        }
        internal static void ExportAndUploadAllDynamicsInScene()
        {
            List<DynamicObject> dynsInSceneList = new List<DynamicObject>();

            // This array HAS TO BE reinitialized here because
            // this function can be from other places and
            // we cannot guarantee that it has been initialized
            var dynamicObjectsInScene = GetDynamicObjectsInScene();
            foreach (var dyn in dynamicObjectsInScene)
            {
                dynsInSceneList.Add(dyn);
            }
            ExportUtility.ExportDynamicObjects(dynsInSceneList);

            UploadDynamics(true, true);
        }

        internal static void UploadDynamics(bool uploadExportedDynamics, bool exportAndUploadDynamicsFromScene, bool showPopups = false)
        {
            void OnManifestUploadComplete()
            {
                if (uploadExportedDynamics)
                {
                    ExportUtility.UploadAllDynamicObjectMeshes(showPopups);
                }
                else if (exportAndUploadDynamicsFromScene)
                {
                    var dynamicMeshNames = new List<string>();
                    var dynamicObjectsInScene = GetDynamicObjectsInScene();
                    foreach (var dyn in dynamicObjectsInScene)
                    {
                        dynamicMeshNames.Add(dyn.MeshName);
                    }
                    ExportUtility.UploadDynamicObjects(dynamicMeshNames, showPopups);
                }
            }

            void OnSceneVersionRefreshed()
            {
                if (uploadExportedDynamics || exportAndUploadDynamicsFromScene)
                {
                    AggregationManifest manifest = new AggregationManifest();
                    manifest.AddOrReplaceDynamic(GetDynamicObjectsInScene());
                    EditorCore.UploadManifest(manifest, OnManifestUploadComplete, OnManifestUploadComplete);
                }
                else
                {
                    OnManifestUploadComplete();
                }
            }
            EditorCore.RefreshSceneVersion(OnSceneVersionRefreshed);
        }
        #endregion
    }

    #region Scene Entry Data
    internal class SceneEntry
    {
        internal string path;
        internal bool selected;
        internal bool shouldDisplay;
        internal int versionNumber;

        internal SceneEntry(string pathToScene, int versionNum, bool sceneSelected = false, bool sceneShouldDisplay = true)
        {
            path = pathToScene;
            versionNumber = versionNum;
            selected = sceneSelected;
            shouldDisplay = sceneShouldDisplay;
        }
    }
    #endregion

    #region Dynamic Entry Data
    internal class DynamicObjectEntry
    {
        //IMPROVEMENT for objects in scene, cache warning for missing collider in children
        internal bool visible = true; //currently shown in the filtered list
        internal bool selected; //not necessarily selected as a gameobject, just checked in this list
        internal string meshName;
        internal bool hasExportedMesh;
        internal bool isIdPool;
        internal int idPoolCount;
        internal DynamicObject objectReference;
        internal DynamicObjectIdPool poolReference;
        internal string gameobjectName;
        internal bool hasBeenUploaded;
        internal DynamicObjectEntry(string meshName, bool exportedMesh, DynamicObject reference, string name, bool initiallySelected, bool uploaded)
        {
            objectReference = reference;
            gameobjectName = name;
            this.meshName = meshName;
            hasExportedMesh = exportedMesh;
            selected = initiallySelected;
            hasBeenUploaded = uploaded;
        }
        internal DynamicObjectEntry(bool exportedMesh, DynamicObjectIdPool reference, bool initiallySelected, bool uploaded)
        {
            isIdPool = true;
            poolReference = reference;
            idPoolCount = poolReference.Ids.Length;
            gameobjectName = poolReference.PrefabName;
            meshName = poolReference.MeshName;
            hasExportedMesh = exportedMesh;
            selected = initiallySelected;
            hasBeenUploaded = uploaded;
        }
    }
    #endregion
}
