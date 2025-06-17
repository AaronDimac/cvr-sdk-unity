using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cognitive3D
{
    internal class DynamicObjectDetailGUI : IFeatureDetailGUI
    {
        private Vector2 scrollPos;

        private bool selectAll;

        DynamicObject[] _cachedDynamics;
        DynamicObject[] DynamicObjects
        {
            get
            {
                if (_cachedDynamics == null || _cachedDynamics.Length == 0)
                {
#if UNITY_2020_1_OR_NEWER
                    _cachedDynamics = Cognitive3D_Preferences.Instance.IncludeDisabledDynamicObjects ? GameObject.FindObjectsByType<DynamicObject>(FindObjectsInactive.Include, FindObjectsSortMode.None) : GameObject.FindObjectsByType<DynamicObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                    _cachedDynamics = FindObjectsOfType<DynamicObject>();
#endif
                }
                return _cachedDynamics;
            }
        }

        private List<DynamicObjectData> dynamicObjects = new List<DynamicObjectData>();
        bool refreshList;
        private int lastDynamicCount = -1;
        private void CheckDynamicObjectChanges()
        {
#if UNITY_2020_1_OR_NEWER
            int currentCount = Cognitive3D_Preferences.Instance.IncludeDisabledDynamicObjects
            ? GameObject.FindObjectsByType<DynamicObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length
            : GameObject.FindObjectsByType<DynamicObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
#else
            int currentCount = FindObjectsOfType<DynamicObject>().Length;
#endif
            if (currentCount != lastDynamicCount)
            {
                lastDynamicCount = currentCount;
                refreshList = true;
            }
        }

        private double lastCheckTime = 0;

        #region Visual Elements
        public void OnGUI()
        {
            // Check for updates only once per second
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastCheckTime > 1.0)
            {
                lastCheckTime = currentTime;
                CheckDynamicObjectChanges();
            }

            if (refreshList)
            {
                GetEntryList();
            }

            GUILayout.Label("Dynamic Objects", EditorCore.styles.FeatureTitleStyle);
            GUILayout.Label(
                "A Dynamic Object is a specific object in your experience which you wish to track.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Refresh List"))
            {
                refreshList = true;
            }

            DrawHeader();

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));
            foreach (var obj in dynamicObjects)
            {
                DrawDynamicObjectRow(obj);
            }
            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            bool allSelected = true;

            foreach (var obj in dynamicObjects)
            {
                if (!obj.IsSelected)
                {
                    allSelected = false;
                    break;
                }
            }

            if (selectAll != allSelected)
            {
                selectAll = allSelected;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool newSelectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(20));
            DrawColumnSeparator();

            // If the checkbox changed, update all rows
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                foreach (var obj in dynamicObjects)
                {
                    obj.IsSelected = selectAll;
                }
            }

            GUILayout.Label("GameObject", GUILayout.Width(100));
            DrawColumnSeparator();

            GUILayout.Label("Mesh Name", GUILayout.Width(100));
            DrawColumnSeparator();

            GUILayout.Label("Dynamic ID", GUILayout.Width(100));
            DrawColumnSeparator();

            GUILayout.Label("Exported", EditorCore.styles.centeredLabelStyle, GUILayout.Width(100));
            DrawColumnSeparator();

            GUILayout.Label("Uploaded", EditorCore.styles.centeredLabelStyle, GUILayout.Width(100));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawColumnSeparator()
        {
            var rect = GUILayoutUtility.GetRect(1, 18, GUILayout.Width(1));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }

        private void DrawDynamicObjectRow(DynamicObjectData obj)
        {
            Rect rowRect = EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);

            EditorGUILayout.BeginVertical(); // Inner padding container
            GUILayout.Space(3); // Top padding

            EditorGUILayout.BeginHorizontal();

            obj.IsSelected = EditorGUILayout.Toggle(obj.IsSelected, GUILayout.Width(20));

            GUILayout.Space(5);
            GUILayout.Label(obj.Name, GUILayout.Width(100));

            GUILayout.Space(5);
            GUILayout.Label(obj.MeshName, GUILayout.Width(100));

            GUILayout.Space(5);
            GUILayout.Label(new GUIContent(obj.DynamicId, obj.DynamicId), GUILayout.Width(100));

            GUILayout.Space(5);
            DrawStatusIcon(obj.Exported, GUILayout.Width(100));

            GUILayout.Space(5);
            DrawStatusIcon(obj.Uploaded, GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3); // Bottom padding
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Handle click event
            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                Selection.activeGameObject = obj.referenceObj;
                Event.current.Use();
            }
        }

        private void DrawStatusIcon(bool state, params GUILayoutOption[] options)
        {
            // var icon = state
            //     ? new GUIContent(EditorCore.CircleEmpty, "Tooltip text")
            //     : new GUIContent(EditorCore.CompleteCheckmark, "Tooltip text");

            GUILayout.BeginHorizontal(options);
            GUILayout.FlexibleSpace();
            if (state)
            {
                GUILayout.Label(new GUIContent(EditorCore.CompleteCheckmark, "Tooltip text"), EditorCore.styles.completeIconStyle);
            }
            else
            {
                GUILayout.Label(new GUIContent(EditorCore.CircleEmpty, "Tooltip text"), EditorCore.styles.incompleteIconStyle);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Retrieve Dynamics
        private void GetEntryList()
        {
            Clear();
            foreach (var dynamic in DynamicObjects)
            {
                dynamicObjects.Add(
                    new DynamicObjectData
                    {
                        Name = dynamic.gameObject.name,
                        MeshName = dynamic.MeshName,
                        DynamicId = dynamic.GetId(),
                        Exported = EditorCore.GetExportedDynamicObjectNames().Contains(dynamic.MeshName) || !dynamic.UseCustomMesh,
                        Uploaded = true,
                        referenceObj = dynamic.gameObject
                    }
                );
            }

            refreshList = false;
        }

        void Clear()
        {
            _cachedDynamics = null;
            dynamicObjects.Clear();
        }

        private class DynamicObjectData
        {
            internal string Name;
            internal string MeshName;
            internal string DynamicId;
            internal bool Exported;
            internal bool Uploaded;
            internal bool IsSelected;
            internal GameObject referenceObj;
        }
        #endregion
    }
}
