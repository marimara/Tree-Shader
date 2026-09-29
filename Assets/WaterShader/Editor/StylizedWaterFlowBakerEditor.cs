using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Meganeura.Water;

namespace Meganeura.Water.Editor
{
    [CustomEditor(typeof(StylizedWaterFlowBaker))]
    public sealed class StylizedWaterFlowBakerEditor : UnityEditor.Editor
    {
        bool showQuickSetupOptions;

        public override void OnInspectorGUI()
        {
            var baker = (StylizedWaterFlowBaker)target;
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Auto Setup reads the Source Mesh and replaces only the enabled Quick Setup values. It does not Bake. The complete operation supports Undo.", MessageType.None);
            showQuickSetupOptions = EditorGUILayout.Foldout(showQuickSetupOptions, "Setup Options", true);
            if (showQuickSetupOptions)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoFitBakeBounds"), new GUIContent("Fit Bake Bounds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoExtractBoundary"), new GUIContent("Extract Boundary"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoGenerateCenterline"), new GUIContent("Generate Centerline"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoChooseResolution"), new GUIContent("Choose Resolution"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoEstimateFlowCoordinates"), new GUIContent("Estimate Flow Coordinates"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoEstimateSteeringDistance"), new GUIContent("Estimate Steering Distance"));
                serializedObject.ApplyModifiedProperties();
            }
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Auto Setup From Mesh", GUILayout.Height(32f)))
                {
                    try { WaterMeshAutoSetup.Run(baker); }
                    catch (Exception exception) { Debug.LogException(exception, baker); }
                }
                using (new EditorGUI.DisabledScope(baker.ControlPoints.Count < 2))
                {
                    if (GUILayout.Button("Reverse Flow Direction"))
                    {
                        Undo.RecordObject(baker, "Reverse Water Flow Direction");
                        baker.ReverseFlowDirection();
                        EditorUtility.SetDirty(baker);
                        EditorSceneManager.MarkSceneDirty(baker.gameObject.scene);
                        SceneView.RepaintAll();
                    }
                }
            }
            if (!string.IsNullOrEmpty(baker.LastAutoSetupSummary))
                EditorGUILayout.HelpBox(baker.LastAutoSetupSummary, baker.LastAutoSetupHadWarnings ? MessageType.Warning : MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manual Baker Settings", EditorStyles.boldLabel);
            DrawDefaultInspector();
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Bake And Assign Flow Map", GUILayout.Height(30f)))
                    StylizedWaterFlowBakerUtility.Bake(baker, true);
                if (GUILayout.Button("Bake Without Material Assignment"))
                    StylizedWaterFlowBakerUtility.Bake(baker, false);
                if (GUILayout.Button("Assign Existing Baked Map"))
                    StylizedWaterFlowBakerUtility.AssignToMaterial(baker);
            }
            if (!string.IsNullOrEmpty(baker.LastBakeSummary))
                EditorGUILayout.HelpBox(baker.LastBakeSummary, MessageType.Info);
            if (baker.BakeDiagnostics != null)
            {
                EditorGUILayout.ObjectField("Mask / Distance Debug", baker.BakeDiagnostics, typeof(Texture2D), false);
                EditorGUILayout.HelpBox("Debug legend: black = outside water, magenta = blocked collider, cyan/green = valid water with increasing clearance.", MessageType.None);
            }
        }

        void OnSceneGUI()
        {
            var baker = (StylizedWaterFlowBaker)target;
            Transform tr = baker.transform;
            Handles.matrix = tr.localToWorldMatrix;

            if (baker.ShowBounds)
            {
                Handles.color = new Color(.1f, .9f, 1f, .9f);
                Vector3 center = new Vector3(baker.BakeCenter.x, 0f, baker.BakeCenter.y);
                Vector3 size = new Vector3(baker.BakeSize.x, 0f, baker.BakeSize.y);
                Handles.DrawWireCube(center, size);
            }

            if (baker.ShowPath && baker.ControlPoints.Count >= 2)
            {
                Handles.color = new Color(1f, .45f, .05f, 1f);
                Vector3 previous = baker.EvaluateLocal(0f);
                for (int i = 1; i <= 128; i++)
                {
                    Vector3 next = baker.EvaluateLocal(i / 128f);
                    Handles.DrawLine(previous, next, 3f);
                    previous = next;
                }
                for (int i = 0; i < baker.ControlPoints.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 moved = Handles.PositionHandle(baker.ControlPoints[i], Quaternion.identity);
                    Handles.Label(baker.ControlPoints[i] + Vector3.up * .15f, i.ToString());
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(baker, "Move Water Centerline Point");
                        baker.ControlPoints[i] = moved;
                        EditorUtility.SetDirty(baker);
                    }
                }
            }

            if (baker.ShowBoundary && baker.UseBoundaryAndObstacles && baker.WaterBoundaryMode == StylizedWaterFlowBaker.BoundaryMode.ExplicitPolygon)
            {
                Handles.color = new Color(.25f, 1f, .3f, .9f);
                for (int i = 0; i < baker.BoundaryPoints.Count; i++)
                {
                    int next = (i + 1) % baker.BoundaryPoints.Count;
                    Handles.DrawDottedLine(baker.BoundaryPoints[i], baker.BoundaryPoints[next], 4f);
                    EditorGUI.BeginChangeCheck();
                    Vector3 moved = Handles.FreeMoveHandle(baker.BoundaryPoints[i], .12f, Vector3.zero, Handles.DotHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(baker, "Move Water Boundary Point");
                        baker.BoundaryPoints[i] = moved;
                        EditorUtility.SetDirty(baker);
                    }
                }
            }

            if (baker.ShowDirectionArrows && baker.ControlPoints.Count >= 2)
            {
                Handles.color = Color.yellow;
                for (int i = 0; i < baker.ArrowCount; i++)
                {
                    float t = (i + .5f) / baker.ArrowCount;
                    Vector3 p = baker.EvaluateLocal(t) + Vector3.up * .05f;
                    Vector3 tangent = baker.EvaluateLocalTangent(t);
                    Handles.ArrowHandleCap(0, p, Quaternion.LookRotation(tangent, Vector3.up), .55f, EventType.Repaint);
                }
            }
            Handles.matrix = Matrix4x4.identity;
        }
    }

    public static class StylizedWaterFlowBakerUtility
    {
        public const string GeneratedFolder = "Assets/WaterShader/Generated/FlowMaps";

        struct PathSample
        {
            public float t;
            public Vector3 worldPosition;
        }

        public static Texture2D Bake(StylizedWaterFlowBaker baker, bool assign)
        {
            if (baker == null) throw new ArgumentNullException(nameof(baker));
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Flow Maps can only be baked outside Play Mode.");
            if (!baker.HasValidPath(out string error)) throw new InvalidOperationException(error);
            int size = (int)baker.Resolution;
            if (size < 16 || size > 4096) throw new InvalidOperationException("Bake resolution is invalid.");
            EnsureFolder(GeneratedFolder);

            var path = BuildPathSamples(baker, 257);
            bool[] valid = new bool[size * size];
            bool[] water = new bool[size * size];
            bool[] blocked = new bool[size * size];
            Vector2[] baseDirections = new Vector2[size * size];
            float[] transverse = new float[size * size];
            float[] pathParameters = new float[size * size];
            Collider[] obstacles = GatherObstacles(baker);
            int validCount = 0, blockedCount = 0;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                Vector2 uv = new Vector2((x + .5f) / size, (y + .5f) / size);
                Vector3 local = baker.LocalFromUV(uv);
                bool inside = IsInsideWater(baker, new Vector2(local.x, local.z));
                water[index] = inside;
                bool isBlocked = inside && baker.UseBoundaryAndObstacles && IsBlocked(baker, local, obstacles, size);
                valid[index] = inside && !isBlocked;
                blocked[index] = isBlocked;
                if (valid[index]) validCount++;
                if (isBlocked) blockedCount++;

                float pathT = ClosestPathParameter(baker, path, local);
                pathParameters[index] = pathT;
                Vector3 tangent = baker.EvaluateLocalTangent(pathT);
                Vector3 center = baker.EvaluateLocal(pathT);
                Vector3 worldTangent = TransformPlanarDirection(baker.transform, tangent);
                Vector3 worldAcross = WorldAcrossDirection(baker.transform, worldTangent, tangent);
                transverse[index] = Vector3.Dot(
                    baker.transform.TransformPoint(local) - baker.transform.TransformPoint(center),
                    worldAcross);
                Vector2 uvDirection = new Vector2(tangent.x / baker.BakeSize.x, tangent.z / baker.BakeSize.y);
                baseDirections[index] = uvDirection.sqrMagnitude > 1e-10f ? uvDirection.normalized : Vector2.right;
            }

            Vector2[] finalDirections = baseDirections;
            float[] obstacleCoordinates = null;
            float[] distance = null;
            if (baker.UseBoundaryAndObstacles)
            {
                distance = BuildDistanceField(baker, valid, size);
                // Banks and obstacles use physical distances while preserving the local Flow Map domain.
                finalDirections = ApplyLocalSteering(baker, water, blocked, BuildDistanceField(baker, water, size), baseDirections, size);
                finalDirections = WaterObstacleContours.Steer(baker, water, blocked, transverse, baseDirections, finalDirections, size, out obstacleCoordinates);
                ValidateCenterline(baker, obstacles);
                WriteDiagnostics(baker, size, valid, blocked, distance);
            }

            if (baker.GenerateCompatibleFlowCoordinates) RebuildFlowCoordinates(baker, path, obstacleCoordinates, finalDirections, obstacles, size);

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                Vector2 direction = finalDirections[i];
                float strength = baker.EvaluateStrength(pathParameters[i]);
                pixels[i] = new Color(direction.x * .5f + .5f, direction.y * .5f + .5f, strength, 1f);
            }

            var generated = new Texture2D(size, size, TextureFormat.RGBAFloat, false, true)
            {
                name = SanitizeName(baker.OutputName),
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            generated.SetPixels(pixels);
            generated.Apply(false, false);

            string assetPath = $"{GeneratedFolder}/{SanitizeName(baker.OutputName)}.asset";
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Texture2D result;
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, assetPath);
                result = generated;
            }
            else
            {
                EditorUtility.CopySerialized(generated, existing);
                UnityEngine.Object.DestroyImmediate(generated);
                EditorUtility.SetDirty(existing);
                result = existing;
            }
            AssetDatabase.SaveAssetIfDirty(result);
            string summary = $"Baked {size}x{size} RGBAFloat to {assetPath}. Valid water: {validCount}; blocked: {blockedCount}; strength: {baker.FlowStrengthMode}.";
            baker.SetBakeResult(result, summary);
            EditorUtility.SetDirty(baker);
            if (assign) AssignToMaterial(baker);
            EditorSceneManager.MarkSceneDirty(baker.gameObject.scene);
            Debug.Log($"[Water Flow Baker] {summary}", baker);
            return result;
        }

        public static void AssignToMaterial(StylizedWaterFlowBaker baker)
        {
            Texture2D texture = baker.BakedFlowMap;
            if (texture == null) throw new InvalidOperationException("Bake a Flow Map before assigning it.");
            Material material = baker.TargetMaterial;
            if (material == null && baker.TargetRenderer != null) material = baker.TargetRenderer.sharedMaterial;
            if (material == null) throw new InvalidOperationException("Assign a target material or renderer.");
            if (!material.HasProperty("_FlowMap")) throw new InvalidOperationException($"Material {material.name} does not use the Stylized Water Flow Map interface.");
            Undo.RecordObject(material, "Assign Baked Water Flow Map");
            material.SetTexture("_FlowMap", texture);
            material.SetFloat("_UseFlowMap", 1f);
            MeshFilter filter = baker.GetComponent<MeshFilter>();
            bool compatibleMeshAssigned = baker.GenerateCompatibleFlowCoordinates &&
                baker.GeneratedFlowMesh != null && filter != null && filter.sharedMesh == baker.GeneratedFlowMesh;
            if (compatibleMeshAssigned && material.HasProperty("_UseFlowCoordinates"))
                material.SetFloat("_UseFlowCoordinates", 1f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
        }

        static PathSample[] BuildPathSamples(StylizedWaterFlowBaker baker, int count)
        {
            var samples = new PathSample[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (count - 1f);
                Vector3 p = baker.EvaluateLocal(t);
                samples[i] = new PathSample
                {
                    t = t,
                    worldPosition = baker.transform.TransformPoint(p)
                };
            }
            return samples;
        }

        static void RebuildFlowCoordinates(StylizedWaterFlowBaker baker, PathSample[] path, float[] obstacleCoordinates, Vector2[] directions, Collider[] obstacles, int size)
        {
            MeshFilter filter = baker.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                throw new InvalidOperationException("Compatible SPEC-009 flow coordinates require a MeshFilter with a mesh on the baker object.");
            if (baker.FlowCoordinateWorldScale.x <= .0001f || baker.FlowCoordinateWorldScale.y <= .0001f)
                throw new InvalidOperationException("Flow Coordinate World Scale must be positive.");

            EnsureFolder("Assets/WaterShader/Generated/Meshes");
            string meshPath = $"Assets/WaterShader/Generated/Meshes/MESH_FlowCoordinates_{SanitizeName(baker.gameObject.name)}.asset";
            Mesh source = baker.SourceMesh;
            if (source == null || source == baker.GeneratedFlowMesh)
            {
                source = filter.sharedMesh;
                if (source == baker.GeneratedFlowMesh)
                    Debug.LogWarning("[Water Flow Baker] Source Mesh is not assigned; this legacy setup is rebuilding from its generated mesh. Run Auto Setup with the original mesh assigned to migrate safely.", baker);
                else
                    baker.SourceMesh = source;
            }
            Mesh working = UnityEngine.Object.Instantiate(source);
            if (obstacleCoordinates != null) WaterObstacleContours.RefineMesh(working, baker, obstacles, size);
            working.name = Path.GetFileNameWithoutExtension(meshPath);
            Vector3[] vertices = working.vertices;
            var mapUV = new Vector2[vertices.Length];
            var chartUV = new Vector2[vertices.Length];
            var frames = new List<Vector4>(vertices.Length);
            float[] accumulated = new float[path.Length];
            for (int i = 1; i < path.Length; i++)
                accumulated[i] = accumulated[i - 1] + Vector3.Distance(path[i - 1].worldPosition, path[i].worldPosition);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 local = vertices[i];
                float t = ClosestPathParameter(baker, path, local);
                Vector3 center3 = baker.EvaluateLocal(t), tangent3 = baker.EvaluateLocalTangent(t);
                Vector3 worldTangent = TransformPlanarDirection(baker.transform, tangent3);
                Vector3 worldAcross = WorldAcrossDirection(baker.transform, worldTangent, tangent3);
                float scaled = t * (path.Length - 1);
                int sample = Mathf.Min(Mathf.FloorToInt(scaled), path.Length - 2);
                float alongDistance = Mathf.Lerp(accumulated[sample], accumulated[sample + 1], scaled - sample);
                float acrossDistance = Vector3.Dot(
                    baker.transform.TransformPoint(local) - baker.transform.TransformPoint(center3),
                    worldAcross);
                mapUV[i] = baker.UVFromLocal(local);
                if (obstacleCoordinates != null)
                    acrossDistance = WaterObstacleContours.Sample(obstacleCoordinates, size, mapUV[i]);
                chartUV[i] = new Vector2(alongDistance / baker.FlowCoordinateWorldScale.x, acrossDistance / baker.FlowCoordinateWorldScale.y);
                Vector3 frameWorldAlong = worldTangent;
                if (obstacleCoordinates != null)
                {
                    // Match the exact stored/bilinearly sampled RG field, including
                    // its derivative stencil and bank blend, not a second derivative
                    // approximation that introduces transverse chart velocity.
                    Vector2 uvFlow = WaterObstacleContours.Sample(directions, size, mapUV[i]);
                    Vector2 localFlow = Vector2.Scale(uvFlow, baker.BakeSize);
                    frameWorldAlong = TransformPlanarDirection(baker.transform, new Vector3(localFlow.x, 0f, localFlow.y));
                }
                Vector3 frameWorldAcross = WorldAcrossDirection(baker.transform, frameWorldAlong, tangent3);
                Vector3 localAlongPerWorldUnit = baker.transform.InverseTransformVector(frameWorldAlong);
                Vector3 localAcrossPerWorldUnit = baker.transform.InverseTransformVector(frameWorldAcross);
                frames.Add(new Vector4(
                    localAlongPerWorldUnit.x * baker.FlowCoordinateWorldScale.x / baker.BakeSize.x,
                    localAlongPerWorldUnit.z * baker.FlowCoordinateWorldScale.x / baker.BakeSize.y,
                    localAcrossPerWorldUnit.x * baker.FlowCoordinateWorldScale.y / baker.BakeSize.x,
                    localAcrossPerWorldUnit.z * baker.FlowCoordinateWorldScale.y / baker.BakeSize.y));
            }
            working.uv = mapUV;
            working.uv2 = chartUV;
            working.SetUVs(2, frames);

            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            Mesh result;
            if (existing == null)
            {
                AssetDatabase.CreateAsset(working, meshPath);
                result = working;
            }
            else
            {
                // CopySerialized updates serialized CPU data but can leave the live
                // renderer using stale vertex buffers/layout after subdivision.
                // Set mesh buffers explicitly so a rebake is visible immediately.
                existing.Clear();
                existing.indexFormat = working.indexFormat;
                existing.vertices = working.vertices;
                existing.normals = working.normals;
                existing.uv = mapUV;
                existing.uv2 = chartUV;
                existing.SetUVs(2, frames);
                existing.subMeshCount = working.subMeshCount;
                for (int sub = 0; sub < working.subMeshCount; sub++)
                    existing.SetTriangles(working.GetTriangles(sub), sub);
                existing.bounds = working.bounds;
                UnityEngine.Object.DestroyImmediate(working);
                EditorUtility.SetDirty(existing);
                result = existing;
            }
            filter.sharedMesh = result;
            baker.SetGeneratedFlowMesh(result);
            EditorUtility.SetDirty(filter);
            AssetDatabase.SaveAssetIfDirty(result);
        }

        static float ClosestPathParameter(StylizedWaterFlowBaker baker, PathSample[] samples, Vector3 localPoint)
        {
            Vector3 worldPoint = baker.transform.TransformPoint(localPoint);
            float bestDistance = float.MaxValue, bestT = 0f;
            for (int i = 0; i < samples.Length - 1; i++)
            {
                Vector3 a = samples[i].worldPosition, delta = samples[i + 1].worldPosition - a;
                float f = delta.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(worldPoint - a, delta) / delta.sqrMagnitude) : 0f;
                float distance = (worldPoint - (a + delta * f)).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestT = Mathf.Lerp(samples[i].t, samples[i + 1].t, f);
                }
            }
            float radius = 2f / (samples.Length - 1);
            float lo = Mathf.Max(0f, bestT - radius), hi = Mathf.Min(1f, bestT + radius);
            for (int iteration = 0; iteration < 8; iteration++)
            {
                float aT = Mathf.Lerp(lo, hi, 1f / 3f), bT = Mathf.Lerp(lo, hi, 2f / 3f);
                Vector3 a = baker.transform.TransformPoint(baker.EvaluateLocal(aT));
                Vector3 b = baker.transform.TransformPoint(baker.EvaluateLocal(bT));
                if ((a - worldPoint).sqrMagnitude < (b - worldPoint).sqrMagnitude) hi = bT;
                else lo = aT;
            }
            return (lo + hi) * .5f;
        }

        static bool IsInsideWater(StylizedWaterFlowBaker baker, Vector2 point)
        {
            if (!baker.UseBoundaryAndObstacles || baker.WaterBoundaryMode == StylizedWaterFlowBaker.BoundaryMode.BakeBounds)
                return true;
            var polygon = baker.BoundaryPoints;
            if (polygon == null || polygon.Count < 3) throw new InvalidOperationException("Explicit boundary requires at least three points.");
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                Vector2 a = new Vector2(polygon[i].x, polygon[i].z), b = new Vector2(polygon[j].x, polygon[j].z);
                Vector2 edge = b - a;
                float edgeT = edge.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude) : 0f;
                if ((point - (a + edge * edgeT)).sqrMagnitude <= 1e-8f) return true;
                if (((a.y > point.y) != (b.y > point.y)) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        static Collider[] GatherObstacles(StylizedWaterFlowBaker baker)
        {
            var result = new List<Collider>();
            foreach (Collider collider in baker.ExplicitObstacles)
                if (collider != null && collider.enabled && collider.gameObject.activeInHierarchy && !result.Contains(collider)) result.Add(collider);
            int mask = baker.ObstacleLayers.value;
            if (mask != 0)
            {
                foreach (Collider collider in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude))
                    if (((1 << collider.gameObject.layer) & mask) != 0 && collider.enabled && !result.Contains(collider)) result.Add(collider);
            }
            return result.ToArray();
        }

        static bool IsBlocked(StylizedWaterFlowBaker baker, Vector3 local, Collider[] obstacles, int size)
        {
            Vector3 world = baker.transform.TransformPoint(local);
            // Clearance for the derivative stencil and bilinear runtime sampling.
            Vector3 cellX = baker.transform.TransformVector(new Vector3(baker.BakeSize.x / size, 0f, 0f));
            Vector3 cellZ = baker.transform.TransformVector(new Vector3(0f, 0f, baker.BakeSize.y / size));
            float cellRadius = 1.5f * Mathf.Max(cellX.magnitude, cellZ.magnitude);
            Vector3 planeNormal = WorldPlaneNormal(baker.transform);
            foreach (Collider collider in obstacles)
            {
                Vector3 closest = collider.ClosestPoint(world);
                Vector3 delta = closest - world;
                float planeDistance = Mathf.Abs(Vector3.Dot(delta, planeNormal));
                float horizontal = (delta - planeNormal * Vector3.Dot(delta, planeNormal)).magnitude;
                bool verticallyRelevant = planeDistance <= Mathf.Max(.5f, cellRadius);
                if (verticallyRelevant && horizontal <= cellRadius) return true;
            }
            return false;
        }

        static float[] BuildDistanceField(StylizedWaterFlowBaker baker, bool[] valid, int size)
        {
            Vector3 stepX = baker.transform.TransformVector(new Vector3(baker.BakeSize.x / size, 0f, 0f));
            Vector3 stepZ = baker.transform.TransformVector(new Vector3(0f, 0f, baker.BakeSize.y / size));
            float costX = stepX.magnitude;
            float costZ = stepZ.magnitude;
            float diagonalPositive = (stepX + stepZ).magnitude;
            float diagonalNegative = (stepX - stepZ).magnitude;
            var distance = new float[valid.Length];
            for (int i = 0; i < distance.Length; i++) distance[i] = valid[i] ? 1e6f : 0f;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                if (x > 0) distance[i] = Mathf.Min(distance[i], distance[i - 1] + costX);
                if (y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size] + costZ);
                if (x > 0 && y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size - 1] + diagonalPositive);
                if (x + 1 < size && y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size + 1] + diagonalNegative);
            }
            for (int y = size - 1; y >= 0; y--) for (int x = size - 1; x >= 0; x--)
            {
                int i = y * size + x;
                if (x + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + 1] + costX);
                if (y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size] + costZ);
                if (x + 1 < size && y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size + 1] + diagonalPositive);
                if (x > 0 && y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size - 1] + diagonalNegative);
            }
            return distance;
        }

        static void WriteDiagnostics(StylizedWaterFlowBaker baker, int size, bool[] valid, bool[] blocked, float[] distance)
        {
            EnsureFolder("Assets/WaterShader/Generated/Diagnostics");
            var pixels = new Color[valid.Length];
            float range = Mathf.Max(.0001f, baker.SteeringDistance);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (blocked[i]) pixels[i] = new Color(1f, 0f, .65f, 1f);
                else if (!valid[i]) pixels[i] = Color.black;
                else
                {
                    float clearance = Mathf.Clamp01(distance[i] / range);
                    pixels[i] = new Color(.05f, Mathf.Lerp(.25f, 1f, clearance), Mathf.Lerp(.9f, .35f, clearance), 1f);
                }
            }
            var generated = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "T_FlowDebug_" + SanitizeName(baker.gameObject.name),
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            generated.SetPixels(pixels);
            generated.Apply(false, false);
            string path = $"Assets/WaterShader/Generated/Diagnostics/{generated.name}.asset";
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Texture2D result;
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, path);
                result = generated;
            }
            else
            {
                EditorUtility.CopySerialized(generated, existing);
                UnityEngine.Object.DestroyImmediate(generated);
                EditorUtility.SetDirty(existing);
                result = existing;
            }
            baker.SetBakeDiagnostics(result);
            AssetDatabase.SaveAssetIfDirty(result);
        }

        static Vector2[] ApplyLocalSteering(StylizedWaterFlowBaker baker, bool[] valid, bool[] blocked, float[] distance, Vector2[] baseDirections, int size)
        {
            var result = (Vector2[])baseDirections.Clone();
            for (int y = 1; y < size - 1; y++) for (int x = 1; x < size - 1; x++)
            {
                int i = y * size + x;
                if (!valid[i]) continue;
                float proximity = 1f - Mathf.Clamp01(distance[i] / Mathf.Max(.0001f, baker.SteeringDistance));
                if (proximity <= 0f) continue;
                Vector3 inwardWorld = GridGradientWorld(baker, distance, i, size);
                if (inwardWorld.sqrMagnitude < 1e-8f) continue;
                inwardWorld.Normalize();
                Vector2 forward = baseDirections[i];
                Vector3 forwardWorld = WorldDirectionFromUV(baker, forward);
                Vector3 lateralWorld = inwardWorld - forwardWorld * Vector3.Dot(inwardWorld, forwardWorld);
                float outwardRisk = Mathf.Max(0f, -Vector3.Dot(forwardWorld, inwardWorld));
                float localInfluence = proximity * baker.SteeringStrength * Mathf.Max(.35f, outwardRisk);
                Vector3 resultWorld = forwardWorld;
                if (lateralWorld.sqrMagnitude > 1e-8f)
                    resultWorld = (forwardWorld + lateralWorld.normalized * localInfluence).normalized;
                result[i] = UVDirectionFromWorld(baker, resultWorld);
                if (Vector2.Dot(result[i], forward) < .35f)
                    result[i] = Vector2.Lerp(forward, result[i], .55f).normalized;
            }
            return result;
        }

        internal static Vector3 WorldPlaneNormal(Transform transform)
        {
            Vector3 x = transform.TransformVector(Vector3.right);
            Vector3 z = transform.TransformVector(Vector3.forward);
            Vector3 normal = Vector3.Cross(z, x);
            return normal.sqrMagnitude > 1e-12f ? normal.normalized : transform.up;
        }

        internal static Vector3 TransformPlanarDirection(Transform transform, Vector3 localDirection)
        {
            localDirection.y = 0f;
            Vector3 world = transform.TransformVector(localDirection);
            return world.sqrMagnitude > 1e-12f ? world.normalized : transform.right;
        }

        internal static Vector3 WorldAcrossDirection(Transform transform, Vector3 worldAlong, Vector3 localAlong)
        {
            Vector3 across = Vector3.Cross(WorldPlaneNormal(transform), worldAlong).normalized;
            Vector3 localReference = new Vector3(-localAlong.z, 0f, localAlong.x);
            if (Vector3.Dot(across, transform.TransformVector(localReference)) < 0f) across = -across;
            return across;
        }

        internal static Vector3 WorldDirectionFromUV(StylizedWaterFlowBaker baker, Vector2 uvDirection)
        {
            Vector2 local = Vector2.Scale(uvDirection, baker.BakeSize);
            return TransformPlanarDirection(baker.transform, new Vector3(local.x, 0f, local.y));
        }

        internal static Vector2 UVDirectionFromWorld(StylizedWaterFlowBaker baker, Vector3 worldDirection)
        {
            Vector3 local = baker.transform.InverseTransformVector(worldDirection);
            Vector2 uv = new Vector2(local.x / baker.BakeSize.x, local.z / baker.BakeSize.y);
            return uv.sqrMagnitude > 1e-12f ? uv.normalized : Vector2.right;
        }

        internal static Vector3 GridGradientWorld(StylizedWaterFlowBaker baker, float[] values, int index, int size)
        {
            Vector3 stepX = baker.transform.TransformVector(new Vector3(baker.BakeSize.x / size, 0f, 0f));
            Vector3 stepZ = baker.transform.TransformVector(new Vector3(0f, 0f, baker.BakeSize.y / size));
            float lengthX = Mathf.Max(1e-6f, stepX.magnitude);
            float lengthZ = Mathf.Max(1e-6f, stepZ.magnitude);
            Vector3 axisX = stepX / lengthX;
            Vector3 axisZ = stepZ / lengthZ;
            float derivativeX = (values[index + 1] - values[index - 1]) / (2f * lengthX);
            float derivativeZ = (values[index + size] - values[index - size]) / (2f * lengthZ);
            float dot = Mathf.Clamp(Vector3.Dot(axisX, axisZ), -.9999f, .9999f);
            float inverse = 1f / Mathf.Max(1e-5f, 1f - dot * dot);
            float coefficientX = (derivativeX - dot * derivativeZ) * inverse;
            float coefficientZ = (derivativeZ - dot * derivativeX) * inverse;
            return axisX * coefficientX + axisZ * coefficientZ;
        }

        static void ValidateCenterline(StylizedWaterFlowBaker baker, Collider[] obstacles)
        {
            int outside = 0, blocked = 0;
            for (int i = 0; i <= 80; i++)
            {
                Vector3 local = baker.EvaluateLocal(i / 80f);
                if (!IsInsideWater(baker, new Vector2(local.x, local.z))) outside++;
                if (IsBlocked(baker, local, obstacles, (int)baker.Resolution)) blocked++;
            }
            if (outside > 0) Debug.LogWarning($"[Water Flow Baker] Centerline leaves the valid water boundary at {outside}/81 samples.", baker);
            if (blocked > 0) Debug.LogWarning($"[Water Flow Baker] Centerline intersects configured obstacles at {blocked}/81 samples. Local steering is not a pathfinder; move the path or obstacle if the channel is blocked.", baker);
        }

        static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static string SanitizeName(string value)
        {
            string name = string.IsNullOrWhiteSpace(value) ? "T_FlowMap_Water" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
            return name;
        }
    }
}
