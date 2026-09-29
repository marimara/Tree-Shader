using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Meganeura.Water.Editor
{
    public static class Spec0117Capture
    {
        const string Root = "Assets/WaterShader/Validation/SPEC-011.7/.frames";
        const string NoisePath = "Assets/WaterShader/Textures/Noise 1.png";
        const string PrimaryPath = "Assets/WaterShader/Generated/Patterns/T_WaterPattern_Primary_1107.png";
        const string SecondaryPath = "Assets/WaterShader/Generated/Patterns/T_WaterPattern_Secondary_1107.png";

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Straight - Noise 1")]
        public static void CaptureNoise()
        {
            Capture("Straight_Noise1", NoisePath, 3f, 7f, .50f, 1.55f, 1.65f, .66f, 540, 30f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Straight - Generated Primary")]
        public static void CapturePrimary()
        {
            Capture("Straight_Primary_1107", PrimaryPath, 4f, 1.5f, .43f, 1.0f, .60f, .66f, 540, 30f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Straight - Generated Secondary")]
        public static void CaptureSecondary()
        {
            Capture("Straight_Secondary_1107", SecondaryPath, 2.1f, 1.0f, .40f, 1.0f, .60f, .66f, 540, 30f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Curved Obstacle - Primary")]
        public static void CaptureCurvedObstacle()
        {
            CapturePrimaryScenario("CurvedObstacle_Primary", .66f, 360, 20f, "CurvedWater_Channel");
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Calm - Primary")]
        public static void CaptureCalm()
        {
            CapturePrimaryScenario("Calm_Primary", 0f, 360, 10f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture River - Primary")]
        public static void CaptureRiver()
        {
            CapturePrimaryScenario("River_Primary", .52f, 360, 20f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Fast - Primary")]
        public static void CaptureFast()
        {
            CapturePrimaryScenario("Fast_Primary", 1f, 360, 20f);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Scale 1 - Primary")]
        public static void CaptureScale1()
        {
            CaptureTransform(false);
        }

        [MenuItem("Tools/WaterShader/SPEC-011.7/Capture Non-uniform Scale - Primary")]
        public static void CaptureNonUniform()
        {
            CaptureTransform(true);
        }

        static void Capture(string name, string texturePath, float width, float stretch,
            float threshold, float contrast, float scale, float strength, int frames,
            float rate, string sourceName = "")
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null) throw new InvalidOperationException("Pattern texture is missing: " + texturePath);
            string shaderPath = ProductionCopy();
            Spec0116Capture.Start(name, shaderPath, 7, strength, frames, rate, sourceName, 0f, Root, material =>
            {
                material.SetTexture("_NoiseTex", texture);
                material.SetFloat("_PatternSourceMode", texturePath == NoisePath ? 3f : 4f);
                material.SetFloat("_PrimaryMarkWidth", width);
                material.SetFloat("_PatternStretch", stretch);
                material.SetFloat("_PatternThreshold", threshold);
                material.SetFloat("_PatternSoftness", .075f);
                material.SetFloat("_NoiseContrast", contrast);
                material.SetFloat("_PatternScale", scale);
                material.SetFloat("_PatternStrength", .72f);
            });
        }

        static void CapturePrimaryScenario(string name, float strength, int frames, float rate,
            string sourceName = "")
        {
            Capture(name, PrimaryPath, 4f, 1.5f, .43f, 1f, .60f,
                strength, frames, rate, sourceName);
        }

        static void CaptureTransform(bool scaled)
        {
            string suffix = scaled ? "NonUniformSource" : "Scale1Source";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                "Assets/WaterShader/Generated/Meshes/MESH_FlowCoordinates_SPEC0116_" + suffix + ".asset");
            Texture2D map = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/WaterShader/Generated/FlowMaps/T_SPEC0116_" + suffix + ".asset");
            if (mesh == null || map == null)
                throw new InvalidOperationException("SPEC-011.6 transform fixtures are missing.");

            var fixture = new GameObject("SPEC0117_TransformCaptureSource", typeof(MeshFilter), typeof(MeshRenderer));
            fixture.hideFlags = HideFlags.HideAndDontSave;
            var sourceMaterial = new Material(AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/WaterShader/Materials/MAT_Water_Uniform_Matched.mat"));
            try
            {
                fixture.transform.localScale = scaled ? new Vector3(10f, 1f, 2f) : Vector3.one;
                fixture.transform.rotation = Quaternion.Euler(0f, 17f, 0f);
                fixture.GetComponent<MeshFilter>().sharedMesh = mesh;
                sourceMaterial.SetTexture("_FlowMap", map);
                fixture.GetComponent<MeshRenderer>().sharedMaterial = sourceMaterial;
                CapturePrimaryScenario(suffix + "_Primary", .66f, 360, 20f, fixture.name);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
                UnityEngine.Object.DestroyImmediate(sourceMaterial);
            }
        }

        static string ProductionCopy()
        {
            const string path = "Assets/WaterShader/Validation/SPEC-011.7/ProductionCapture.shader";
            string shader = File.ReadAllText("Assets/WaterShader/Shaders/StylizedWater.shader")
                .Replace("Shader \"Meganeura/Water/Stylized Water\"", "Shader \"Hidden/Water/SPEC0117ProductionCapture\"")
                .Replace("_Time.y", "_ValidationTime")
                .Replace("half4 _ShallowColor;", "float _ValidationTime; half4 _ShallowColor;")
                .Replace("        _ShallowColor (", "        _ValidationTime (\"Validation Time\", Float) = 0\n        _ShallowColor (");
            File.WriteAllText(path, shader);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return path;
        }
    }
}
