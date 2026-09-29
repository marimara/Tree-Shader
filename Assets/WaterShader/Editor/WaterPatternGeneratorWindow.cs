using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.Water.Editor
{
    public sealed class WaterPatternGeneratorWindow : EditorWindow
    {
        const int PreviewResolution = 128;
        const double PreviewDebounceSeconds = .25;
        enum PreviewMode { OneByOne = 1, Tiled2x2 = 2, Tiled4x4 = 4 }

        [SerializeField] WaterPatternSettings settings = new WaterPatternSettings();
        [SerializeField] PreviewMode previewMode = PreviewMode.Tiled2x2;
        [SerializeField] bool silhouette;
        [SerializeField] bool autoPreview = true;
        WaterPatternResult previewResult;
        Texture2D silhouettePreview;
        Vector2 scroll;
        string diagnostics = "Generate a preview to inspect the pattern.";
        bool previewQueued;
        double previewDueTime;

        [MenuItem("Tools/WaterShader/Water Pattern Generator")]
        static void Open()
        {
            var window = GetWindow<WaterPatternGeneratorWindow>();
            window.titleContent = new GUIContent("Water Patterns");
            window.minSize = new Vector2(450f, 720f);
            window.Show();
        }

        void OnEnable()
        {
            if (settings == null) settings = new WaterPatternSettings();
            if (settings.MacroBandCount < 3) settings.ApplyPreset(settings.Role);
            EditorApplication.update += ProcessPreviewQueue;
            QueuePreview(true);
        }

        void OnDisable()
        {
            EditorApplication.update -= ProcessPreviewQueue;
            DestroyPreviewTextures();
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Water Ribbon Pattern Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Layered water ribbons bend, compress and overlap into directional painterly streaks. Highlights emerge locally from compression instead of tracing every ribbon.", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            WaterPatternRole role = (WaterPatternRole)EditorGUILayout.EnumPopup("Pattern Role", settings.Role);
            if (role != settings.Role) { settings.ApplyPreset(role); GUI.changed = true; }
            settings.Seed = EditorGUILayout.IntField("Seed", settings.Seed);
            settings.Direction = EditorGUILayout.Slider("Flow Direction", settings.Direction, 0f, 360f);

            Section("Macro Ribbons");
            settings.MacroBandCount = EditorGUILayout.IntSlider("Macro Band Count", settings.MacroBandCount, 3, 10);
            settings.BandWidth = EditorGUILayout.Slider("Band Width", settings.BandWidth, .025f, .16f);
            settings.WidthVariation = EditorGUILayout.Slider("Width Variation", settings.WidthVariation, 0f, 1f);
            settings.BaselineCurvature = EditorGUILayout.Slider("Baseline Curvature", settings.BaselineCurvature, 0f, 1f);

            Section("Pulse And Compression");
            settings.PulseAmount = EditorGUILayout.Slider("Pulse Amount", settings.PulseAmount, 0f, 1f);
            settings.PulseStrength = EditorGUILayout.Slider("Pulse Strength", settings.PulseStrength, 0f, 1f);
            settings.PulseWidth = EditorGUILayout.Slider("Pulse Width", settings.PulseWidth, .06f, .3f);
            settings.PulseSharpness = EditorGUILayout.Slider("Pulse Sharpness", settings.PulseSharpness, 0f, 1f);
            settings.CompressionStrength = EditorGUILayout.Slider("Compression Strength", settings.CompressionStrength, 0f, 1f);

            Section("Directional Brush");
            settings.BrushSmearLength = EditorGUILayout.Slider("Brush Smear Length", settings.BrushSmearLength, .03f, .35f);
            settings.BrushWidth = EditorGUILayout.Slider("Brush Width", settings.BrushWidth, .025f, .18f);
            settings.BrushSoftness = EditorGUILayout.Slider("Brush Softness", settings.BrushSoftness, 0f, 1f);
            settings.DryBrushVariation = EditorGUILayout.Slider("Dry Brush Variation", settings.DryBrushVariation, 0f, 1f);

            Section("Organic Breakup");
            settings.BreakupAmount = EditorGUILayout.Slider("Breakup Amount", settings.BreakupAmount, 0f, 1f);
            settings.BreakupScale = EditorGUILayout.Slider("Breakup Scale", settings.BreakupScale, 1f, 12f);
            settings.SecondaryDetail = EditorGUILayout.Slider("Secondary Detail", settings.SecondaryDetail, 0f, 1f);
            settings.Contrast = EditorGUILayout.Slider("Contrast", settings.Contrast, .5f, 2.5f);
            bool sourceChanged = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space(6f);
            settings.Resolution = EditorGUILayout.IntPopup("Final Resolution", settings.Resolution,
                new[] { "256", "512", "1024" }, new[] { 256, 512, 1024 });
            EditorGUI.BeginChangeCheck();
            previewMode = (PreviewMode)EditorGUILayout.EnumPopup("Preview Tiling", previewMode);
            silhouette = EditorGUILayout.Toggle("High Contrast Silhouette", silhouette);
            bool displayChanged = EditorGUI.EndChangeCheck();
            autoPreview = EditorGUILayout.Toggle("Auto Preview", autoPreview);

            if (sourceChanged && autoPreview) QueuePreview();
            if (displayChanged) UpdateDisplayPreview();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("New Seed", GUILayout.Height(28f)))
                {
                    settings.Seed = unchecked(settings.Seed * 1664525 + 1013904223);
                    QueuePreview();
                }
                if (GUILayout.Button("Generate Preview", GUILayout.Height(28f))) GeneratePreviewNow();
                if (GUILayout.Button("Save Texture", GUILayout.Height(28f))) SaveTexture();
            }

            Rect previewRect = GUILayoutUtility.GetAspectRect(1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(.035f, .045f, .055f));
            Texture2D display = silhouette && silhouettePreview != null ? silhouettePreview : previewResult?.Texture;
            if (display != null) DrawTiledPreview(previewRect, display, (int)previewMode);
            else EditorGUI.LabelField(previewRect, "Generating...", new GUIStyle(EditorStyles.centeredGreyMiniLabel));
            EditorGUILayout.HelpBox(diagnostics, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        static void Section(string label)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        void QueuePreview(bool immediate = false)
        {
            previewQueued = true;
            previewDueTime = EditorApplication.timeSinceStartup + (immediate ? 0.0 : PreviewDebounceSeconds);
        }

        void ProcessPreviewQueue()
        {
            if (!previewQueued || EditorApplication.timeSinceStartup < previewDueTime || GUIUtility.hotControl != 0) return;
            GeneratePreviewNow();
        }

        void GeneratePreviewNow()
        {
            previewQueued = false; DestroyPreviewTextures();
            try
            {
                previewResult = WaterPatternGenerator.GeneratePreview(settings, PreviewResolution);
                UpdateDisplayPreview();
                float edgeDifference = WaterPatternGenerator.MeasureOppositeEdgeDifference(previewResult.Texture);
                diagnostics = previewResult.Summary(settings) +
                    $"\nFinal output: {settings.Resolution} px. Opposite-edge delta: {edgeDifference:0.0000}.";
            }
            catch (Exception exception) { diagnostics = exception.Message; Debug.LogException(exception); }
            Repaint();
        }

        void SaveTexture()
        {
            WaterPatternResult result = null;
            try
            {
                WaterPatternSettings finalSettings = settings.Clone();
                EditorUtility.DisplayProgressBar("Water Pattern Generator", "Painting final texture...", .2f);
                result = WaterPatternGenerator.Generate(finalSettings);
                string path = WaterPatternGenerator.Save(finalSettings, result);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                EditorGUIUtility.PingObject(Selection.activeObject);
                diagnostics = "Saved: " + path + "\n" + result.Summary(finalSettings);
            }
            catch (Exception exception) { diagnostics = exception.Message; Debug.LogException(exception); }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (result?.Texture != null) DestroyImmediate(result.Texture);
            }
            Repaint();
        }

        void UpdateDisplayPreview()
        {
            if (silhouettePreview != null) DestroyImmediate(silhouettePreview);
            silhouettePreview = null;
            if (silhouette && previewResult?.Texture != null)
                silhouettePreview = WaterPatternGenerator.CreateSilhouettePreview(previewResult.Texture);
            Repaint();
        }

        static void DrawTiledPreview(Rect rect, Texture2D texture, int tiles)
        {
            float width = rect.width / tiles, height = rect.height / tiles;
            for (int y = 0; y < tiles; y++)
                for (int x = 0; x < tiles; x++)
                    GUI.DrawTexture(new Rect(rect.x + x * width, rect.y + y * height, width, height), texture, ScaleMode.StretchToFill, false);
        }

        void DestroyPreviewTextures()
        {
            if (silhouettePreview != null) DestroyImmediate(silhouettePreview);
            if (previewResult?.Texture != null) DestroyImmediate(previewResult.Texture);
            silhouettePreview = null; previewResult = null;
        }
    }
}
