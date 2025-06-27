using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cognitive3D
{
    internal class FeaturesWindow : EditorWindow
    {
        private float slideProgress = 0f;
        private float slideSpeed = 4f;
        private bool slidingForward = false;
        private bool slidingBackward = false;

        private Vector2 mainScroll;

        internal static void Init()
        {
            FeaturesWindow window = GetWindow<FeaturesWindow>("Features");
            window.minSize = new Vector2(600, 800);
            window.titleContent = new GUIContent("Features");
            window.ShowUtility();
        }

        private List<FeatureData> features;
        private int currentFeatureIndex = -1;

        private void OnEnable()
        {
            features = FeatureLibrary.CreateFeatures((index) =>
            {
                currentFeatureIndex = index;
                slidingForward = true;
            });
        }

        private void OnGUI()
        {
            // Slide transition handler
            if (slidingForward || slidingBackward)
            {
                slideProgress += Time.deltaTime * slideSpeed * (slidingForward ? 1 : -1);
                slideProgress = Mathf.Clamp01(slideProgress);

                if (slideProgress == 1f)
                {
                    slidingForward = false;
                }
                else if (slideProgress == 0f)
                {
                    slidingBackward = false;
                }

                Repaint();
            }

            float width = position.width;

            Rect mainRect = new Rect(-width * slideProgress, 0, width, position.height);
            Rect detailRect = new Rect(width - width * slideProgress, 0, width, position.height);

            GUILayout.BeginArea(mainRect);
            mainScroll = GUILayout.BeginScrollView(mainScroll);
            DrawMainPage();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(detailRect);
            DrawDetailPage();
            GUILayout.EndArea();
        }

        private void DrawMainPage()
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();

            if (EditorCore.LogoIcon != null)
            {
                GUILayout.Label(EditorCore.LogoIcon, GUILayout.Width(100), GUILayout.Height(80));
            }
            else
            {
                EditorGUILayout.HelpBox("Logo texture not found", MessageType.Warning);
            }

            GUILayout.Space(10); // spacing between logo and text

            GUILayout.BeginVertical();
            GUILayout.Label("Welcome to the Feature Builder", EditorCore.styles.FeatureTitleStyle);
            GUILayout.Label(
                "Explore the features of our platform. Each feature unlocks powerful capabilities you can use in your experience—from analytics to live control and more.",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            foreach (var feature in features)
            {
                DrawFeatureButton(feature);
            }
        }

        private void DrawFeatureButton(FeatureData featureData)
        {
            if (featureData == null || featureData.Icon == null) return;

            GUILayout.BeginHorizontal();

            // Reserve the button area
            Rect buttonRect = GUILayoutUtility.GetRect(
                new GUIContent(featureData.Title, featureData.Icon),
                EditorCore.styles.buttonStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(100)
            );

            // Define sub-rects
            Rect iconRect = new Rect(buttonRect.x, buttonRect.y + 1, 98, 98);
            Rect labelRect = new Rect(iconRect.xMax + 20, buttonRect.y, buttonRect.width - 180, buttonRect.height);

            // Draw background of main button
            GUI.Box(buttonRect, GUIContent.none, EditorCore.styles.buttonStyle);

            // Draw icon and label
            GUI.DrawTexture(iconRect, featureData.Icon);
            GUI.Label(labelRect, featureData.Title, EditorStyles.label);

            // Draw action buttons (Apply, Upload, LinkTo, etc.)
            if (featureData.Actions != null && featureData.Actions.Count > 0)
            {
                float buttonWidth = 40;
                float spacing = 5;
                float yOffset = 30;

                for (int i = 0; i < featureData.Actions.Count; i++)
                {
                    var action = featureData.Actions[i];
                    float x = buttonRect.xMax - ((featureData.Actions.Count - i) * (buttonWidth + spacing));
                    Rect actionRect = new Rect(x, buttonRect.y + yOffset, buttonWidth, buttonRect.height - 60);

                    // Draw the button
                    if (GUI.Button(actionRect, new GUIContent(EditorCore.ExternalIcon, action.Tooltip), EditorCore.styles.applyButtonStyle))
                    {
                        action.OnClick?.Invoke();
                        Event.current.Use();
                    }
                }
            }

            // Handle main button click (make sure it's not overlapping an action button)
            if (buttonRect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
            {
                // Don't fire if clicking any action button area
                bool clickedAction = false;

                if (featureData.Actions != null)
                {
                    float buttonWidth = 40;
                    float spacing = 5;
                    float yOffset = 30;

                    for (int i = 0; i < featureData.Actions.Count; i++)
                    {
                        float x = buttonRect.xMax - ((featureData.Actions.Count - i) * (buttonWidth + spacing));
                        Rect actionRect = new Rect(x, buttonRect.y + yOffset, buttonWidth, buttonRect.height - 60);

                        if (actionRect.Contains(Event.current.mousePosition))
                        {
                            clickedAction = true;
                            break;
                        }
                    }
                }

                if (!clickedAction)
                {
                    featureData.OnClick?.Invoke();
                    Event.current.Use();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawDetailPage()
        {
            if (currentFeatureIndex < 0 || currentFeatureIndex >= features.Count)
            {
                GUILayout.Label("Invalid feature selected.");
                return;
            }

            var feature = features[currentFeatureIndex];

            // Custom GUI
            GUILayout.BeginVertical(EditorCore.styles.DetailContainer);
            feature.DetailGUI?.OnGUI();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", GUILayout.Height(40)))
            {
                slidingBackward = true;
            }
        }
    }
}
