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
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var baker = (StylizedWaterFlowBaker)target;
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
            public Vector2 position;
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

                float pathT = ClosestPathParameter(baker, path, new Vector2(local.x, local.z));
                pathParameters[index] = pathT;
                Vector3 tangent = baker.EvaluateLocalTangent(pathT);
                Vector3 center = baker.EvaluateLocal(pathT);
                transverse[index] = Vector3.Dot(local - center, new Vector3(-tangent.z, 0f, tangent.x));
                Vector2 uvDirection = new Vector2(tangent.x / baker.BakeSize.x, tangent.z / baker.BakeSize.y);
                baseDirections[index] = uvDirection.sqrMagnitude > 1e-10f ? uvDirection.normalized : Vector2.right;
            }

            Vector2[] finalDirections = baseDirections;
            float[] obstacleCoordinates = null;
            float[] distance = null;
            if (baker.UseBoundaryAndObstacles)
            {
                distance = BuildDistanceField(valid, size);
                // Banks keep their validated local treatment; internal solids have separate constraints.
                finalDirections = ApplyLocalSteering(baker, water, blocked, BuildDistanceField(water, size), baseDirections, size);
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
                samples[i] = new PathSample { t = t, position = new Vector2(p.x, p.z) };
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
            Mesh working = UnityEngine.Object.Instantiate(filter.sharedMesh);
            if (obstacleCoordinates != null) WaterObstacleContours.RefineMesh(working, baker, obstacles, size);
            working.name = Path.GetFileNameWithoutExtension(meshPath);
            Vector3[] vertices = working.vertices;
            var mapUV = new Vector2[vertices.Length];
            var chartUV = new Vector2[vertices.Length];
            var frames = new List<Vector4>(vertices.Length);
            float[] accumulated = new float[path.Length];
            for (int i = 1; i < path.Length; i++)
                accumulated[i] = accumulated[i - 1] + Vector2.Distance(path[i - 1].position, path[i].position);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 local = vertices[i];
                Vector2 point = new Vector2(local.x, local.z);
                float t = ClosestPathParameter(baker, path, point);
                Vector3 center3 = baker.EvaluateLocal(t), tangent3 = baker.EvaluateLocalTangent(t);
                Vector2 tangent = new Vector2(tangent3.x, tangent3.z).normalized;
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                float scaled = t * (path.Length - 1);
                int sample = Mathf.Min(Mathf.FloorToInt(scaled), path.Length - 2);
                float alongDistance = Mathf.Lerp(accumulated[sample], accumulated[sample + 1], scaled - sample);
                float acrossDistance = Vector2.Dot(point - new Vector2(center3.x, center3.z), normal);
                mapUV[i] = baker.UVFromLocal(local);
                if (obstacleCoordinates != null)
                    acrossDistance = WaterObstacleContours.Sample(obstacleCoordinates, size, mapUV[i]);
                chartUV[i] = new Vector2(alongDistance / baker.FlowCoordinateWorldScale.x, acrossDistance / baker.FlowCoordinateWorldScale.y);
                Vector2 alongFrame = tangent, acrossFrame = normal;
                if (obstacleCoordinates != null)
                {
                    // Match the exact stored/bilinearly sampled RG field, including
                    // its derivative stencil and bank blend, not a second derivative
                    // approximation that introduces transverse chart velocity.
                    Vector2 uvFlow = WaterObstacleContours.Sample(directions, size, mapUV[i]);
                    alongFrame = Vector2.Scale(uvFlow, baker.BakeSize).normalized;
                    acrossFrame = new Vector2(-alongFrame.y, alongFrame.x);
                }
                frames.Add(new Vector4(
                    alongFrame.x * baker.FlowCoordinateWorldScale.x / baker.BakeSize.x,
                    alongFrame.y * baker.FlowCoordinateWorldScale.x / baker.BakeSize.y,
                    acrossFrame.x * baker.FlowCoordinateWorldScale.y / baker.BakeSize.x,
                    acrossFrame.y * baker.FlowCoordinateWorldScale.y / baker.BakeSize.y));
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

        static float ClosestPathParameter(StylizedWaterFlowBaker baker, PathSample[] samples, Vector2 point)
        {
            float bestDistance = float.MaxValue, bestT = 0f;
            for (int i = 0; i < samples.Length - 1; i++)
            {
                Vector2 a = samples[i].position, delta = samples[i + 1].position - a;
                float f = delta.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector2.Dot(point - a, delta) / delta.sqrMagnitude) : 0f;
                float distance = (point - (a + delta * f)).sqrMagnitude;
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
                Vector3 a = baker.EvaluateLocal(aT), b = baker.EvaluateLocal(bT);
                if ((new Vector2(a.x, a.z) - point).sqrMagnitude < (new Vector2(b.x, b.z) - point).sqrMagnitude) hi = bT;
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
                foreach (Collider collider in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    if (((1 << collider.gameObject.layer) & mask) != 0 && collider.enabled && !result.Contains(collider)) result.Add(collider);
            }
            return result.ToArray();
        }

        static bool IsBlocked(StylizedWaterFlowBaker baker, Vector3 local, Collider[] obstacles, int size)
        {
            Vector3 world = baker.transform.TransformPoint(local);
            // Clearance for the derivative stencil and bilinear runtime sampling.
            float cellRadius = 1.5f * Mathf.Max(baker.BakeSize.x, baker.BakeSize.y) / size;
            foreach (Collider collider in obstacles)
            {
                Vector3 closest = collider.ClosestPoint(world);
                Vector3 delta = baker.transform.InverseTransformVector(closest - world);
                float horizontal = new Vector2(delta.x, delta.z).magnitude;
                bool verticallyRelevant = collider.bounds.min.y <= world.y + .5f && collider.bounds.max.y >= world.y - .5f;
                if (verticallyRelevant && horizontal <= cellRadius) return true;
            }
            return false;
        }

        static float[] BuildDistanceField(bool[] valid, int size)
        {
            const float diagonal = 1.41421356f;
            var distance = new float[valid.Length];
            for (int i = 0; i < distance.Length; i++) distance[i] = valid[i] ? 1e6f : 0f;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                if (x > 0) distance[i] = Mathf.Min(distance[i], distance[i - 1] + 1f);
                if (y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size] + 1f);
                if (x > 0 && y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size - 1] + diagonal);
                if (x + 1 < size && y > 0) distance[i] = Mathf.Min(distance[i], distance[i - size + 1] + diagonal);
            }
            for (int y = size - 1; y >= 0; y--) for (int x = size - 1; x >= 0; x--)
            {
                int i = y * size + x;
                if (x + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + 1] + 1f);
                if (y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size] + 1f);
                if (x + 1 < size && y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size + 1] + diagonal);
                if (x > 0 && y + 1 < size) distance[i] = Mathf.Min(distance[i], distance[i + size - 1] + diagonal);
            }
            return distance;
        }

        static void WriteDiagnostics(StylizedWaterFlowBaker baker, int size, bool[] valid, bool[] blocked, float[] distance)
        {
            EnsureFolder("Assets/WaterShader/Generated/Diagnostics");
            var pixels = new Color[valid.Length];
            float cellWorld = .5f * (baker.BakeSize.x + baker.BakeSize.y) / size;
            float range = Mathf.Max(1f, baker.SteeringDistance / Mathf.Max(.0001f, cellWorld));
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
            float cellWorld = .5f * (baker.BakeSize.x + baker.BakeSize.y) / size;
            float influenceCells = Mathf.Max(1f, baker.SteeringDistance / Mathf.Max(.0001f, cellWorld));
            for (int y = 1; y < size - 1; y++) for (int x = 1; x < size - 1; x++)
            {
                int i = y * size + x;
                if (!valid[i]) continue;
                float proximity = 1f - Mathf.Clamp01(distance[i] / influenceCells);
                if (proximity <= 0f) continue;
                Vector2 inward = new Vector2(distance[i + 1] - distance[i - 1], distance[i + size] - distance[i - size]);
                if (inward.sqrMagnitude < 1e-8f) continue;
                inward.Normalize();
                Vector2 forward = baseDirections[i];
                Vector2 lateral = inward - forward * Vector2.Dot(inward, forward);
                float outwardRisk = Mathf.Max(0f, -Vector2.Dot(forward, inward));
                float localInfluence = proximity * baker.SteeringStrength * Mathf.Max(.35f, outwardRisk);
                if (lateral.sqrMagnitude > 1e-8f)
                    result[i] = (forward + lateral.normalized * localInfluence).normalized;
                if (Vector2.Dot(result[i], forward) < .35f)
                    result[i] = Vector2.Lerp(forward, result[i], .55f).normalized;
            }
            return result;
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
