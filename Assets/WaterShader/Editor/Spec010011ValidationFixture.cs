using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Meganeura.Water;
using Meganeura.Water.Editor;

public static class Spec010011ValidationFixture
{
    const string Root = "Assets/WaterShader/";
    const string ScenePath = Root + "Scenes/WaterShader_TestScene.unity";
    const string SourceMaterial = Root + "Materials/MAT_Water_FlowCurve_Constant.mat";

    [MenuItem("Tools/WaterShader/SPEC-010/1 Setup And Bake Constant")]
    public static void SetupSpec010Constant()
    {
        RequireScene();
        var baker = GetOrCreateBaker();
        Undo.RecordObject(baker, "Configure SPEC-010 Flow Baker");
        ConfigureCommon(baker);
        baker.UseBoundaryAndObstacles = false;
        baker.FlowStrengthMode = StylizedWaterFlowBaker.StrengthMode.Constant;
        baker.ConstantStrength = .62f;
        baker.OutputName = "T_FlowMap_FlowCurve_Baked_Constant";
        baker.TargetMaterial = GetOrCreateMaterial("MAT_Water_FlowBaker_Constant");
        baker.TargetRenderer.sharedMaterial = baker.TargetMaterial;
        StylizedWaterFlowBakerUtility.Bake(baker, true);
        SetDebug(baker.TargetMaterial, 0f);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-010/2 Bake Strength Falloff")]
    public static void BakeSpec010Falloff()
    {
        RequireScene();
        var baker = GetRequiredBaker();
        Undo.RecordObject(baker, "Configure SPEC-010 Strength Falloff");
        baker.UseBoundaryAndObstacles = false;
        baker.FlowStrengthMode = StylizedWaterFlowBaker.StrengthMode.PathFalloff;
        baker.StartStrength = .82f;
        baker.EndStrength = .28f;
        baker.FalloffStart = .55f;
        baker.OutputName = "T_FlowMap_FlowCurve_Baked_Falloff";
        baker.TargetMaterial = GetOrCreateMaterial("MAT_Water_FlowBaker_Falloff");
        baker.TargetRenderer.sharedMaterial = baker.TargetMaterial;
        StylizedWaterFlowBakerUtility.Bake(baker, true);
        ValidateDirectionIdentity(
            Root + "Generated/FlowMaps/T_FlowMap_FlowCurve_Baked_Constant.asset",
            Root + "Generated/FlowMaps/T_FlowMap_FlowCurve_Baked_Falloff.asset");
        SetDebug(baker.TargetMaterial, 0f);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-010/3 Re-bake Edited Path Test")]
    public static void RebakeEditedPathTest()
    {
        RequireScene();
        var baker = GetRequiredBaker();
        ConfigureCommon(baker);
        baker.UseBoundaryAndObstacles = false;
        baker.FlowStrengthMode = StylizedWaterFlowBaker.StrengthMode.Constant;
        baker.OutputName = "T_FlowMap_FlowCurve_RebakeTest";
        Texture2D beforeTexture = StylizedWaterFlowBakerUtility.Bake(baker, false);
        Color[] before = beforeTexture.GetPixels();
        int point = baker.ControlPoints.Count / 2;
        Vector3 original = baker.ControlPoints[point];
        baker.ControlPoints[point] = original + new Vector3(0f, 0f, .8f);
        Texture2D afterTexture = StylizedWaterFlowBakerUtility.Bake(baker, false);
        Color[] after = afterTexture.GetPixels();
        int changed = 0;
        for (int i = 0; i < before.Length; i++)
            if (Mathf.Abs(before[i].r - after[i].r) + Mathf.Abs(before[i].g - after[i].g) > .002f) changed++;
        if (changed < before.Length / 100)
            throw new InvalidOperationException("Re-bake did not update enough RG texels after editing the centerline.");
        Debug.Log($"[SPEC-010 validation] Edited centerline and re-baked the same persistent asset: {changed}/{before.Length} direction texels changed.", baker);
        baker.ControlPoints[point] = original;
        StylizedWaterFlowBakerUtility.Bake(baker, false);
        EditorUtility.SetDirty(baker);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-010/View Constant")]
    public static void ViewSpec010Constant() => ViewMaterial("MAT_Water_FlowBaker_Constant", 0f);

    [MenuItem("Tools/WaterShader/SPEC-010/View Falloff")]
    public static void ViewSpec010Falloff() => ViewMaterial("MAT_Water_FlowBaker_Falloff", 0f);

    [MenuItem("Tools/WaterShader/SPEC-010/View Direction Debug")]
    public static void ViewSpec010Direction() => ViewMaterial("MAT_Water_FlowBaker_Constant", 1f);

    [MenuItem("Tools/WaterShader/SPEC-010/View Strength Debug")]
    public static void ViewSpec010Strength() => ViewMaterial("MAT_Water_FlowBaker_Falloff", 2f);

    [MenuItem("Tools/WaterShader/SPEC-011/1 Bake Boundary Only")]
    public static void BakeBoundaryOnly()
    {
        RequireScene();
        var baker = GetRequiredBaker();
        ConfigureCommon(baker);
        ConfigureBoundary(baker);
        baker.ExplicitObstacles.Clear();
        SetObstacleActive(false);
        baker.FlowStrengthMode = StylizedWaterFlowBaker.StrengthMode.Constant;
        baker.ConstantStrength = .62f;
        baker.OutputName = "T_FlowMap_FlowCurve_Boundary";
        baker.TargetMaterial = GetOrCreateMaterial("MAT_Water_FlowBaker_Boundary");
        baker.TargetRenderer.sharedMaterial = baker.TargetMaterial;
        StylizedWaterFlowBakerUtility.Bake(baker, true);
        SetDebug(baker.TargetMaterial, 0f);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-011/2 Bake Local Obstacle")]
    public static void BakeLocalObstacle()
    {
        RequireScene();
        var baker = GetRequiredBaker();
        ConfigureCommon(baker);
        ConfigureBoundary(baker);
        Collider obstacle = GetOrCreateObstacle(false);
        baker.ExplicitObstacles.Clear();
        baker.ExplicitObstacles.Add(obstacle);
        baker.OutputName = "T_FlowMap_FlowCurve_Obstacle";
        baker.TargetMaterial = GetOrCreateMaterial("MAT_Water_FlowBaker_Obstacle");
        baker.TargetRenderer.sharedMaterial = baker.TargetMaterial;
        StylizedWaterFlowBakerUtility.Bake(baker, true);
        SetDebug(baker.TargetMaterial, 0f);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-011/3 Move Obstacle And Rebake")]
    public static void MoveObstacleAndRebake()
    {
        RequireScene();
        var baker = GetRequiredBaker();
        ConfigureCommon(baker);
        ConfigureBoundary(baker);
        Texture2D before = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Generated/FlowMaps/T_FlowMap_FlowCurve_Obstacle.asset");
        if (before == null) throw new InvalidOperationException("Bake the local obstacle case first.");
        Color[] beforePixels = before.GetPixels();
        Collider obstacle = GetOrCreateObstacle(true);
        baker.ExplicitObstacles.Clear();
        baker.ExplicitObstacles.Add(obstacle);
        baker.OutputName = "T_FlowMap_FlowCurve_ObstacleMoved";
        baker.TargetMaterial = GetOrCreateMaterial("MAT_Water_FlowBaker_ObstacleMoved");
        baker.TargetRenderer.sharedMaterial = baker.TargetMaterial;
        Texture2D moved = StylizedWaterFlowBakerUtility.Bake(baker, true);
        Color[] movedPixels = moved.GetPixels();
        int changed = 0, stable = 0;
        for (int i = 0; i < beforePixels.Length; i++)
        {
            float delta = Mathf.Abs(beforePixels[i].r - movedPixels[i].r) + Mathf.Abs(beforePixels[i].g - movedPixels[i].g);
            if (delta > .005f) changed++; else stable++;
        }
        if (changed == 0 || stable < movedPixels.Length / 2)
            throw new InvalidOperationException("Moved-obstacle validation failed: affected field did not change locally.");
        Debug.Log($"[SPEC-011 validation] Moved obstacle rebake changed {changed}/{movedPixels.Length} RG texels; {stable} remained stable.", baker);
        SetDebug(baker.TargetMaterial, 0f);
        Save();
    }

    [MenuItem("Tools/WaterShader/SPEC-011/View Boundary")]
    public static void ViewBoundary() { SetObstacleActive(false); ViewMaterial("MAT_Water_FlowBaker_Boundary", 0f); }

    [MenuItem("Tools/WaterShader/SPEC-011/View Obstacle")]
    public static void ViewObstacle() { GetOrCreateObstacle(false); ViewMaterial("MAT_Water_FlowBaker_Obstacle", 0f); }

    [MenuItem("Tools/WaterShader/SPEC-011/View Moved Obstacle")]
    public static void ViewMovedObstacle() { GetOrCreateObstacle(true); ViewMaterial("MAT_Water_FlowBaker_ObstacleMoved", 0f); }

    [MenuItem("Tools/WaterShader/SPEC-011/View Direction Debug")]
    public static void ViewObstacleDirection() { GetOrCreateObstacle(false); ViewMaterial("MAT_Water_FlowBaker_Obstacle", 1f); }

    static StylizedWaterFlowBaker GetOrCreateBaker()
    {
        GameObject water = GameObject.Find("CurvedWater_Channel");
        if (water == null) throw new InvalidOperationException("SPEC-009 CurvedWater_Channel is missing.");
        var baker = water.GetComponent<StylizedWaterFlowBaker>();
        if (baker == null) baker = Undo.AddComponent<StylizedWaterFlowBaker>(water);
        return baker;
    }

    static StylizedWaterFlowBaker GetRequiredBaker()
    {
        var baker = UnityEngine.Object.FindFirstObjectByType<StylizedWaterFlowBaker>(FindObjectsInactive.Include);
        if (baker == null) throw new InvalidOperationException("Run SPEC-010 Setup And Bake Constant first.");
        return baker;
    }

    static void ConfigureCommon(StylizedWaterFlowBaker baker)
    {
        baker.TargetRenderer = baker.GetComponent<Renderer>();
        baker.BakeCenter = Vector2.zero;
        baker.BakeSize = new Vector2(22f, 11f);
        baker.Resolution = StylizedWaterFlowBaker.BakeResolution.R256;
        baker.ControlPoints.Clear();
        for (int i = 0; i <= 16; i++)
        {
            Vector2 p = Center(i / 16f);
            baker.ControlPoints.Add(new Vector3(p.x, 0f, p.y));
        }
        baker.SteeringDistance = 1.15f;
        baker.SteeringStrength = .9f;
        EditorUtility.SetDirty(baker);
    }

    static void ConfigureBoundary(StylizedWaterFlowBaker baker)
    {
        baker.UseBoundaryAndObstacles = true;
        baker.WaterBoundaryMode = StylizedWaterFlowBaker.BoundaryMode.ExplicitPolygon;
        baker.BoundaryPoints.Clear();
        const int stations = 64;
        for (int i = 0; i <= stations; i++)
        {
            float t = i / (float)stations;
            Vector2 tangent = Tangent(t), normal = new Vector2(-tangent.y, tangent.x);
            Vector2 p = Center(t) + normal * 1.53f;
            baker.BoundaryPoints.Add(new Vector3(p.x, 0f, p.y));
        }
        for (int i = stations; i >= 0; i--)
        {
            float t = i / (float)stations;
            Vector2 tangent = Tangent(t), normal = new Vector2(-tangent.y, tangent.x);
            Vector2 p = Center(t) - normal * 1.53f;
            baker.BoundaryPoints.Add(new Vector3(p.x, 0f, p.y));
        }
    }

    static Collider GetOrCreateObstacle(bool moved)
    {
        GameObject obstacle = FindSceneObject("SPEC011_LocalObstacle", true);
        if (obstacle == null)
        {
            obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obstacle.name = "SPEC011_LocalObstacle";
            GameObject parent = GameObject.Find("FlowCurve_Test");
            if (parent != null) obstacle.transform.SetParent(parent.transform, false);
            var renderer = obstacle.GetComponent<Renderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/MAT_RiverBank_Technical.mat");
        }
        obstacle.SetActive(true);
        float t = moved ? .63f : .43f;
        Vector2 center = Center(t), tangent = Tangent(t), normal = new Vector2(-tangent.y, tangent.x);
        Vector2 p = center + normal * (moved ? -.72f : .72f);
        obstacle.transform.localPosition = new Vector3(p.x, .28f, p.y);
        obstacle.transform.localRotation = Quaternion.identity;
        obstacle.transform.localScale = new Vector3(1.15f, .55f, 1.15f);
        EditorUtility.SetDirty(obstacle.transform);
        return obstacle.GetComponent<Collider>();
    }

    static void SetObstacleActive(bool active)
    {
        GameObject obstacle = FindSceneObject("SPEC011_LocalObstacle", true);
        if (obstacle != null) obstacle.SetActive(active);
    }

    static GameObject FindSceneObject(string name, bool removeDuplicates)
    {
        var matches = new List<GameObject>();
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            if (candidate.name == name && candidate.scene == activeScene) matches.Add(candidate);
        if (matches.Count == 0) return null;
        GameObject chosen = matches.Find(candidate => candidate.activeSelf) ?? matches[0];
        if (removeDuplicates)
            foreach (GameObject candidate in matches)
                if (candidate != chosen) Undo.DestroyObjectImmediate(candidate);
        return chosen;
    }

    static Material GetOrCreateMaterial(string name)
    {
        string path = Root + "Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        if (!AssetDatabase.CopyAsset(SourceMaterial, path)) throw new InvalidOperationException("Could not create validation material: " + path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    static void ViewMaterial(string name, float debug)
    {
        RequireScene();
        var baker = GetRequiredBaker();
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
        if (material == null) throw new InvalidOperationException("Validation material does not exist: " + name);
        baker.TargetRenderer.sharedMaterial = material;
        baker.TargetMaterial = material;
        SetDebug(material, debug);
        EditorUtility.SetDirty(baker.TargetRenderer);
        Save();
    }

    static void SetDebug(Material material, float value)
    {
        material.SetFloat("_FlowDebugMode", value);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssetIfDirty(material);
    }

    static void ValidateDirectionIdentity(string constantPath, string falloffPath)
    {
        Texture2D a = AssetDatabase.LoadAssetAtPath<Texture2D>(constantPath);
        Texture2D b = AssetDatabase.LoadAssetAtPath<Texture2D>(falloffPath);
        if (a == null || b == null || a.width != b.width) throw new InvalidOperationException("Constant/Falloff maps are unavailable for comparison.");
        Color[] pa = a.GetPixels(), pb = b.GetPixels();
        float maxRG = 0f, maxB = 0f;
        for (int i = 0; i < pa.Length; i++)
        {
            maxRG = Mathf.Max(maxRG, Mathf.Abs(pa[i].r - pb[i].r), Mathf.Abs(pa[i].g - pb[i].g));
            maxB = Mathf.Max(maxB, Mathf.Abs(pa[i].b - pb[i].b));
        }
        if (maxRG > 1e-6f || maxB < .1f) throw new InvalidOperationException($"Strength separation failed: max RG delta={maxRG}, max B delta={maxB}.");
        Debug.Log($"[SPEC-010 validation] Constant/Falloff direction fields are identical (max RG delta {maxRG}); B differs by up to {maxB}.");
    }

    static Vector2 Center(float t)
    {
        float q = Mathf.Clamp01((t - .25f) / .5f);
        return new Vector2(Mathf.Lerp(-10f, 10f, t), Mathf.Lerp(-3f, 3f, q * q * (3f - 2f * q)));
    }

    static Vector2 Tangent(float t)
    {
        float q = Mathf.Clamp01((t - .25f) / .5f);
        return new Vector2(20f, 72f * q * (1f - q)).normalized;
    }

    static void RequireScene()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath)
            throw new InvalidOperationException("Open WaterShader_TestScene before running validation.");
    }

    static void Save()
    {
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
