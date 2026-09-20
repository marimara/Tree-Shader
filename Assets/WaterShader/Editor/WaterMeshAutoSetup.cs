using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meganeura.Water.Editor
{
    /// <summary>Editor-only, read-only analysis of a water mesh footprint.</summary>
    public static class WaterMeshAutoSetup
    {
        struct Edge : IEquatable<Edge>
        {
            public int a, b;
            public Edge(int x, int y) { a = Mathf.Min(x, y); b = Mathf.Max(x, y); }
            public bool Equals(Edge other) { return a == other.a && b == other.b; }
            public override bool Equals(object obj) { return obj is Edge && Equals((Edge)obj); }
            public override int GetHashCode() { unchecked { return a * 486187739 + b; } }
        }

        struct Grid
        {
            public int width, height;
            public Vector2 min;
            public float cell;
            public bool[] inside;
            public float[] clearance;
            public Vector2 Point(int i)
            {
                return min + new Vector2((i % width + .5f) * cell, (i / width + .5f) * cell);
            }
        }

        struct QueueNode : IComparable<QueueNode>
        {
            public int index;
            public float cost;
            public QueueNode(int index, float cost) { this.index = index; this.cost = cost; }
            public int CompareTo(QueueNode other)
            {
                int comparison = cost.CompareTo(other.cost);
                return comparison != 0 ? comparison : index.CompareTo(other.index);
            }
        }

        sealed class Analysis
        {
            public readonly List<string> warnings = new List<string>();
            public List<Vector2> boundary;
            public List<Vector2> centerline;
            public Vector2 min, max;
            public float meanY;
            public float pathLength;
            public float averageWidth;
            public bool ambiguousCenterline;
        }

        public static void Run(StylizedWaterFlowBaker baker)
        {
            if (baker == null) throw new ArgumentNullException(nameof(baker));
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Auto Setup is only available outside Play Mode.");

            MeshFilter filter = baker.GetComponent<MeshFilter>();
            Renderer renderer = baker.GetComponent<Renderer>();
            if (filter == null || filter.sharedMesh == null)
                throw new InvalidOperationException("Auto Setup requires a MeshFilter with a water surface mesh on the same GameObject.");
            if (renderer == null)
                throw new InvalidOperationException("Auto Setup requires a Renderer on the same GameObject.");

            Undo.RegisterCompleteObjectUndo(baker, "Auto Setup Water From Mesh");
            var warnings = new List<string>();
            Mesh mesh = baker.SourceMesh;
            if (mesh == null)
            {
                mesh = filter.sharedMesh;
                if (baker.GeneratedFlowMesh != null && mesh == baker.GeneratedFlowMesh)
                    warnings.Add("The original source mesh was not retained by this pre-SPEC-011.5 setup. The current generated mesh was analyzed read-only; assign Source Mesh before future geometry changes.");
                else
                    baker.SourceMesh = mesh;
            }

            Analysis analysis = Analyze(mesh);
            warnings.AddRange(analysis.warnings);
            baker.TargetRenderer = renderer;
            baker.TargetMaterial = renderer.sharedMaterial;
            string outputName = "T_FlowMap_" + SanitizeName(baker.gameObject.name);
            string outputPath = StylizedWaterFlowBakerUtility.GeneratedFolder + "/" + outputName + ".asset";
            Texture2D conflictingOutput = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);
            if (conflictingOutput != null && conflictingOutput != baker.BakedFlowMap)
            {
                string uniquePath = AssetDatabase.GenerateUniqueAssetPath(outputPath);
                outputName = Path.GetFileNameWithoutExtension(uniquePath);
                warnings.Add("The default output name already belongs to another texture. A unique output name was selected.");
            }
            baker.OutputName = outputName;

            Vector2 dimensions = analysis.max - analysis.min;
            float padding = Mathf.Max(.05f, Mathf.Max(dimensions.x, dimensions.y) * .015f);
            if (baker.AutoFitBakeBounds)
            {
                baker.BakeCenter = (analysis.min + analysis.max) * .5f;
                baker.BakeSize = dimensions + Vector2.one * (padding * 2f);
            }

            if (baker.AutoExtractBoundary && analysis.boundary != null && analysis.boundary.Count >= 3)
            {
                baker.BoundaryPoints.Clear();
                foreach (Vector2 p in analysis.boundary)
                    baker.BoundaryPoints.Add(new Vector3(p.x, analysis.meanY, p.y));
                baker.UseBoundaryAndObstacles = true;
                baker.WaterBoundaryMode = StylizedWaterFlowBaker.BoundaryMode.ExplicitPolygon;
            }

            bool centerlineApplied = false;
            if (baker.AutoGenerateCenterline)
            {
                if (analysis.ambiguousCenterline || analysis.centerline == null || analysis.centerline.Count < 2)
                {
                    warnings.Add("Centerline was left unchanged because the footprint is ambiguous. Edit the existing path manually.");
                }
                else
                {
                    baker.ControlPoints.Clear();
                    foreach (Vector2 p in analysis.centerline)
                        baker.ControlPoints.Add(new Vector3(p.x, analysis.meanY, p.y));
                    centerlineApplied = true;
                }
            }

            if (baker.AutoChooseResolution)
                baker.Resolution = ChooseResolution(baker, dimensions);

            if (baker.AutoEstimateFlowCoordinates && centerlineApplied)
            {
                float along = Mathf.Max(1f, analysis.pathLength / 3.25f);
                float across = Mathf.Max(.35f, analysis.averageWidth * .4f);
                baker.FlowCoordinateWorldScale = new Vector2(along, across);
            }

            if (baker.AutoEstimateSteeringDistance && analysis.averageWidth > .01f)
                baker.SteeringDistance = Mathf.Clamp(analysis.averageWidth * .35f, .2f, Mathf.Max(.3f, analysis.averageWidth * .6f));

            warnings.Add("Centerline direction is inferred deterministically. Use Reverse Flow Direction if the arrows point upstream.");
            string summary = "Auto Setup From Mesh\n" +
                $"Renderer: {renderer.name}; source: {mesh.name}\n" +
                $"Bounds: {baker.BakeSize.x:F2} x {baker.BakeSize.y:F2} at ({baker.BakeCenter.x:F2}, {baker.BakeCenter.y:F2})\n" +
                $"Boundary: {(analysis.boundary == null ? 0 : analysis.boundary.Count)} points; Centerline: {(centerlineApplied ? analysis.centerline.Count : baker.ControlPoints.Count)} points\n" +
                $"Resolution: {(int)baker.Resolution}; Flow Scale: {baker.FlowCoordinateWorldScale.x:F2}, {baker.FlowCoordinateWorldScale.y:F2}; Steering Distance: {baker.SteeringDistance:F2}";
            if (warnings.Count > 0) summary += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
            baker.SetAutoSetupResult(summary, warnings.Count > 1 || analysis.ambiguousCenterline);
            EditorUtility.SetDirty(baker);
            EditorSceneManager.MarkSceneDirty(baker.gameObject.scene);
            SceneView.RepaintAll();
            Debug.Log("[Water Flow Baker] " + summary, baker);
        }

        static Analysis Analyze(Mesh mesh)
        {
            var result = new Analysis();
            Vector3[] vertices;
            int[] triangles;
            ReadMesh(mesh, out vertices, out triangles, result.warnings);
            if (vertices.Length < 3 || triangles.Length < 3)
                throw new InvalidOperationException("The source mesh does not contain enough triangle data for Auto Setup.");

            result.meanY = 0f;
            result.min = new Vector2(float.MaxValue, float.MaxValue);
            result.max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 p = new Vector2(vertices[i].x, vertices[i].z);
                result.min = Vector2.Min(result.min, p);
                result.max = Vector2.Max(result.max, p);
                result.meanY += vertices[i].y;
            }
            result.meanY /= vertices.Length;
            float weldTolerance = Mathf.Max(1e-5f, (result.max - result.min).magnitude * 1e-5f);
            List<Vector2> welded;
            int[] remap = Weld(vertices, weldTolerance, out welded);
            var edgeCounts = new Dictionary<Edge, int>();
            var parent = new int[welded.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;
            int nonTriangles = 0;
            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                int a = remap[triangles[i]], b = remap[triangles[i + 1]], c = remap[triangles[i + 2]];
                if (a == b || b == c || c == a) { nonTriangles++; continue; }
                AddEdge(edgeCounts, new Edge(a, b)); AddEdge(edgeCounts, new Edge(b, c)); AddEdge(edgeCounts, new Edge(c, a));
                Union(parent, a, b); Union(parent, b, c);
            }
            if (nonTriangles > 0) result.warnings.Add(nonTriangles + " degenerate projected triangles were ignored.");

            var components = new HashSet<int>();
            for (int i = 0; i < parent.Length; i++) components.Add(Find(parent, i));
            if (components.Count > 1)
            {
                result.warnings.Add("Multiple disconnected mesh components were detected; automatic centerline is not reliable.");
                result.ambiguousCenterline = true;
            }

            var adjacency = new Dictionary<int, List<int>>();
            int nonManifoldEdges = 0;
            foreach (var pair in edgeCounts)
            {
                if (pair.Value > 2) nonManifoldEdges++;
                if (pair.Value != 1) continue;
                AddNeighbor(adjacency, pair.Key.a, pair.Key.b);
                AddNeighbor(adjacency, pair.Key.b, pair.Key.a);
            }
            if (nonManifoldEdges > 0)
            {
                result.warnings.Add(nonManifoldEdges + " non-manifold projected edges were detected; automatic centerline is disabled.");
                result.ambiguousCenterline = true;
            }
            foreach (var pair in adjacency)
                if (pair.Value.Count != 2)
                {
                    result.warnings.Add("The projected boundary is open or branched; automatic centerline is disabled.");
                    result.ambiguousCenterline = true;
                    break;
                }

            List<List<Vector2>> loops = TraceLoops(adjacency, welded);
            if (loops.Count == 0) throw new InvalidOperationException("No closed external boundary could be extracted from the mesh footprint.");
            loops.Sort((a, b) => Mathf.Abs(SignedArea(b)).CompareTo(Mathf.Abs(SignedArea(a))));
            if (loops.Count > 1)
            {
                float ratio = Mathf.Abs(SignedArea(loops[1])) / Mathf.Max(1e-6f, Mathf.Abs(SignedArea(loops[0])));
                result.warnings.Add($"{loops.Count} boundary loops were found; the largest outer loop was selected.");
                if (ratio > .18f)
                {
                    result.warnings.Add("A second significant loop makes the footprint disconnected or hole-like; automatic centerline is disabled.");
                    result.ambiguousCenterline = true;
                }
            }

            List<Vector2> outer = loops[0];
            if (SignedArea(outer) < 0f) outer.Reverse();
            float perimeter = Perimeter(outer, true);
            result.boundary = SimplifyClosed(outer, Mathf.Max(weldTolerance * 4f, perimeter * .0025f), 64);
            Grid grid = BuildGrid(result.boundary, result.min, result.max);
            if (DetectBranching(grid, result.boundary))
            {
                result.warnings.Add("The footprint contains persistent split cross-sections consistent with branching or a folded channel; automatic centerline is disabled.");
                result.ambiguousCenterline = true;
            }
            if (!result.ambiguousCenterline)
                result.centerline = GenerateCenterline(grid, result.boundary, result, weldTolerance);
            return result;
        }

        static void ReadMesh(Mesh mesh, out Vector3[] vertices, out int[] triangles, List<string> warnings)
        {
            using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(mesh))
            {
                Mesh.MeshData data = dataArray[0];
                var nativeVertices = new NativeArray<Vector3>(data.vertexCount, Allocator.Temp);
                data.GetVertices(nativeVertices);
                vertices = nativeVertices.ToArray();
                nativeVertices.Dispose();
                var all = new List<int>();
                for (int sub = 0; sub < data.subMeshCount; sub++)
                {
                    SubMeshDescriptor desc = data.GetSubMesh(sub);
                    if (desc.topology != MeshTopology.Triangles)
                    {
                        warnings.Add($"Submesh {sub} is not triangular and was ignored.");
                        continue;
                    }
                    var indices = new NativeArray<int>(desc.indexCount, Allocator.Temp);
                    data.GetIndices(indices, sub, true);
                    for (int i = 0; i < indices.Length; i++) all.Add(indices[i]);
                    indices.Dispose();
                }
                triangles = all.ToArray();
            }
        }

        static int[] Weld(Vector3[] source, float tolerance, out List<Vector2> points)
        {
            points = new List<Vector2>();
            var buckets = new Dictionary<long, List<int>>();
            var remap = new int[source.Length];
            float inverse = 1f / tolerance;
            for (int i = 0; i < source.Length; i++)
            {
                Vector2 p = new Vector2(source[i].x, source[i].z);
                int qx = Mathf.RoundToInt(p.x * inverse), qy = Mathf.RoundToInt(p.y * inverse);
                int found = -1;
                for (int oy = -1; oy <= 1 && found < 0; oy++) for (int ox = -1; ox <= 1 && found < 0; ox++)
                {
                    long key = Key(qx + ox, qy + oy);
                    List<int> bucket;
                    if (!buckets.TryGetValue(key, out bucket)) continue;
                    foreach (int candidate in bucket)
                        if ((points[candidate] - p).sqrMagnitude <= tolerance * tolerance) { found = candidate; break; }
                }
                if (found < 0)
                {
                    found = points.Count; points.Add(p);
                    long key = Key(qx, qy);
                    List<int> bucket;
                    if (!buckets.TryGetValue(key, out bucket)) buckets[key] = bucket = new List<int>();
                    bucket.Add(found);
                }
                remap[i] = found;
            }
            return remap;
        }

        static long Key(int x, int y) { return ((long)x << 32) ^ (uint)y; }
        static void AddEdge(Dictionary<Edge, int> counts, Edge edge) { int count; counts.TryGetValue(edge, out count); counts[edge] = count + 1; }
        static void AddNeighbor(Dictionary<int, List<int>> map, int a, int b) { List<int> list; if (!map.TryGetValue(a, out list)) map[a] = list = new List<int>(); if (!list.Contains(b)) list.Add(b); }
        static int Find(int[] parent, int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
        static void Union(int[] parent, int a, int b) { a = Find(parent, a); b = Find(parent, b); if (a != b) parent[b] = a; }

        static List<List<Vector2>> TraceLoops(Dictionary<int, List<int>> adjacency, List<Vector2> points)
        {
            var loops = new List<List<Vector2>>();
            var used = new HashSet<Edge>();
            foreach (var pair in adjacency)
            {
                foreach (int firstNext in pair.Value)
                {
                    if (used.Contains(new Edge(pair.Key, firstNext))) continue;
                    var loop = new List<Vector2>();
                    int start = pair.Key, previous = -1, current = start;
                    for (int guard = 0; guard <= adjacency.Count + 2; guard++)
                    {
                        loop.Add(points[current]);
                        List<int> neighbors;
                        if (!adjacency.TryGetValue(current, out neighbors) || neighbors.Count == 0) break;
                        int next = neighbors[0] == previous && neighbors.Count > 1 ? neighbors[1] : neighbors[0];
                        used.Add(new Edge(current, next));
                        previous = current; current = next;
                        if (current == start) { if (loop.Count >= 3) loops.Add(loop); break; }
                    }
                }
            }
            return loops;
        }

        static Grid BuildGrid(List<Vector2> polygon, Vector2 min, Vector2 max)
        {
            Vector2 size = max - min;
            float cell = Mathf.Max(size.x, size.y) / 144f;
            cell = Mathf.Max(cell, Mathf.Min(size.x, size.y) / 48f);
            int width = Mathf.Clamp(Mathf.CeilToInt(size.x / cell) + 4, 12, 192);
            int height = Mathf.Clamp(Mathf.CeilToInt(size.y / cell) + 4, 12, 192);
            var grid = new Grid { width = width, height = height, cell = cell, min = min - Vector2.one * (cell * 2f), inside = new bool[width * height], clearance = new float[width * height] };
            for (int i = 0; i < grid.inside.Length; i++) grid.inside[i] = PointInPolygon(grid.Point(i), polygon);
            BuildClearance(ref grid);
            return grid;
        }

        static void BuildClearance(ref Grid grid)
        {
            const float diagonal = 1.41421356f;
            for (int i = 0; i < grid.clearance.Length; i++) grid.clearance[i] = grid.inside[i] ? 1e6f : 0f;
            for (int y = 0; y < grid.height; y++) for (int x = 0; x < grid.width; x++)
            {
                int i = y * grid.width + x;
                if (x > 0) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i - 1] + 1f);
                if (y > 0) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i - grid.width] + 1f);
                if (x > 0 && y > 0) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i - grid.width - 1] + diagonal);
                if (x + 1 < grid.width && y > 0) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i - grid.width + 1] + diagonal);
            }
            for (int y = grid.height - 1; y >= 0; y--) for (int x = grid.width - 1; x >= 0; x--)
            {
                int i = y * grid.width + x;
                if (x + 1 < grid.width) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i + 1] + 1f);
                if (y + 1 < grid.height) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i + grid.width] + 1f);
                if (x + 1 < grid.width && y + 1 < grid.height) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i + grid.width + 1] + diagonal);
                if (x > 0 && y + 1 < grid.height) grid.clearance[i] = Mathf.Min(grid.clearance[i], grid.clearance[i + grid.width - 1] + diagonal);
            }
        }

        static List<Vector2> GenerateCenterline(Grid grid, List<Vector2> polygon, Analysis result, float tolerance)
        {
            Vector2 axis = PrincipalAxis(polygon);
            float minProjection = float.MaxValue, maxProjection = float.MinValue;
            for (int i = 0; i < grid.inside.Length; i++) if (grid.inside[i])
            {
                float projection = Vector2.Dot(grid.Point(i), axis);
                minProjection = Mathf.Min(minProjection, projection); maxProjection = Mathf.Max(maxProjection, projection);
            }
            float band = Mathf.Max(grid.cell * 2f, (maxProjection - minProjection) * .09f);
            int start = SelectSeed(grid, axis, minProjection, minProjection + band, false);
            int goal = SelectSeed(grid, axis, maxProjection - band, maxProjection, true);
            if (start < 0 || goal < 0 || start == goal) throw new InvalidOperationException("The footprint is too small or ambiguous to generate a centerline.");

            int count = grid.inside.Length;
            var distance = new float[count]; var previous = new int[count]; var closed = new bool[count];
            for (int i = 0; i < count; i++) { distance[i] = float.MaxValue; previous[i] = -1; }
            distance[start] = 0f;
            var open = new SortedSet<QueueNode>();
            open.Add(new QueueNode(start, 0f));
            while (open.Count > 0)
            {
                QueueNode node = open.Min;
                open.Remove(node);
                int current = node.index;
                if (closed[current]) continue;
                if (current == goal) break;
                closed[current] = true;
                int cx = current % grid.width, cy = current / grid.width;
                for (int oy = -1; oy <= 1; oy++) for (int ox = -1; ox <= 1; ox++)
                {
                    if (ox == 0 && oy == 0) continue;
                    int x = cx + ox, y = cy + oy;
                    if (x < 0 || x >= grid.width || y < 0 || y >= grid.height) continue;
                    int next = y * grid.width + x;
                    if (!grid.inside[next] || closed[next]) continue;
                    float step = (ox == 0 || oy == 0) ? 1f : 1.41421356f;
                    float clearance = Mathf.Min(grid.clearance[current], grid.clearance[next]);
                    // A pronounced clearance cost keeps the route on the medial ridge
                    // instead of taking a shorter diagonal across a widening basin.
                    float cost = step * (1f + 28f / ((clearance + .5f) * (clearance + .5f)));
                    float candidate = distance[current] + cost;
                    if (candidate < distance[next])
                    {
                        if (distance[next] < float.MaxValue) open.Remove(new QueueNode(next, distance[next]));
                        distance[next] = candidate; previous[next] = current;
                        open.Add(new QueueNode(next, candidate));
                    }
                }
            }
            if (previous[goal] < 0) throw new InvalidOperationException("No continuous interior route exists between the inferred channel ends.");
            var dense = new List<Vector2>();
            for (int i = goal; i >= 0; i = previous[i]) { dense.Add(grid.Point(i)); if (i == start) break; }
            dense.Reverse();
            for (int pass = 0; pass < 2; pass++)
            {
                var smooth = new List<Vector2>(dense);
                for (int i = 1; i < dense.Count - 1; i++)
                {
                    Vector2 candidate = (dense[i - 1] + dense[i] * 2f + dense[i + 1]) * .25f;
                    if (PointInPolygon(candidate, polygon)) smooth[i] = candidate;
                }
                dense = smooth;
            }
            result.pathLength = Perimeter(dense, false);
            float clearanceSum = 0f;
            for (int i = 0; i < dense.Count; i++) clearanceSum += DistanceToPolygon(dense[i], polygon);
            result.averageWidth = Mathf.Max(grid.cell * 2f, 2f * clearanceSum / dense.Count);
            float epsilon = Mathf.Max(grid.cell * 1.25f, result.averageWidth * .045f);
            List<Vector2> simplified = SimplifyOpen(dense, epsilon);
            while (simplified.Count > 24) { epsilon *= 1.35f; simplified = SimplifyOpen(dense, epsilon); }
            if (simplified.Count < 3 && dense.Count >= 3) simplified.Insert(1, dense[dense.Count / 2]);
            return simplified;
        }

        static int SelectSeed(Grid grid, Vector2 axis, float lo, float hi, bool high)
        {
            Vector2 normal = new Vector2(-axis.y, axis.x);
            float transverseMean = 0f;
            int candidates = 0;
            for (int i = 0; i < grid.inside.Length; i++) if (grid.inside[i])
            {
                float projection = Vector2.Dot(grid.Point(i), axis);
                if (projection < lo || projection > hi) continue;
                transverseMean += Vector2.Dot(grid.Point(i), normal);
                candidates++;
            }
            if (candidates > 0) transverseMean /= candidates;
            int best = -1; float bestScore = float.MinValue;
            for (int i = 0; i < grid.inside.Length; i++) if (grid.inside[i])
            {
                float projection = Vector2.Dot(grid.Point(i), axis);
                if (projection < lo || projection > hi) continue;
                float edgePreference = high ? projection - lo : hi - projection;
                float offCenter = Mathf.Abs(Vector2.Dot(grid.Point(i), normal) - transverseMean) / Mathf.Max(grid.cell, 1e-5f);
                float score = grid.clearance[i] * 12f - edgePreference / Mathf.Max(grid.cell, 1e-5f) - offCenter * 2f;
                if (score > bestScore) { bestScore = score; best = i; }
            }
            return best;
        }

        static bool DetectBranching(Grid grid, List<Vector2> polygon)
        {
            Vector2 axis = PrincipalAxis(polygon), normal = new Vector2(-axis.y, axis.x);
            float minA = float.MaxValue, maxA = float.MinValue, minN = float.MaxValue, maxN = float.MinValue;
            for (int i = 0; i < grid.inside.Length; i++) if (grid.inside[i])
            {
                Vector2 p = grid.Point(i); float a = Vector2.Dot(p, axis), n = Vector2.Dot(p, normal);
                minA = Mathf.Min(minA, a); maxA = Mathf.Max(maxA, a); minN = Mathf.Min(minN, n); maxN = Mathf.Max(maxN, n);
            }
            int persistent = 0;
            for (int slice = 2; slice < 30; slice++)
            {
                float a = Mathf.Lerp(minA, maxA, (slice + .5f) / 32f);
                int runs = 0; bool wasInside = false; int runLength = 0; int substantialRuns = 0;
                for (float n = minN; n <= maxN; n += grid.cell)
                {
                    Vector2 p = axis * a + normal * n;
                    bool inside = PointInPolygon(p, polygon);
                    if (inside) { if (!wasInside) { runs++; runLength = 0; } runLength++; }
                    if ((!inside || n + grid.cell > maxN) && wasInside && runLength >= 3) substantialRuns++;
                    wasInside = inside;
                }
                persistent = substantialRuns >= 2 ? persistent + 1 : 0;
                if (persistent >= 4) return true;
            }
            return false;
        }

        static StylizedWaterFlowBaker.BakeResolution ChooseResolution(StylizedWaterFlowBaker baker, Vector2 localSize)
        {
            Vector3 scale = baker.transform.lossyScale;
            float longestWorld = Mathf.Max(localSize.x * Mathf.Abs(scale.x), localSize.y * Mathf.Abs(scale.z));
            if (longestWorld <= 14f) return StylizedWaterFlowBaker.BakeResolution.R128;
            if (longestWorld <= 36f) return StylizedWaterFlowBaker.BakeResolution.R256;
            if (longestWorld <= 90f) return StylizedWaterFlowBaker.BakeResolution.R512;
            return StylizedWaterFlowBaker.BakeResolution.R1024;
        }

        static Vector2 PrincipalAxis(List<Vector2> points)
        {
            Vector2 mean = Vector2.zero; foreach (Vector2 p in points) mean += p; mean /= points.Count;
            float xx = 0f, xy = 0f, yy = 0f;
            foreach (Vector2 p in points) { Vector2 d = p - mean; xx += d.x * d.x; xy += d.x * d.y; yy += d.y * d.y; }
            float angle = .5f * Mathf.Atan2(2f * xy, xx - yy);
            Vector2 axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (axis.x < 0f || (Mathf.Abs(axis.x) < 1e-5f && axis.y < 0f)) axis = -axis;
            return axis;
        }

        static List<Vector2> SimplifyClosed(List<Vector2> points, float tolerance, int maxCount)
        {
            if (points.Count <= 3) return new List<Vector2>(points);
            int first = 0, second = 1;
            float farthest = 0f;
            for (int i = 0; i < points.Count; i++) for (int j = i + 1; j < points.Count; j++)
            {
                float distance = (points[i] - points[j]).sqrMagnitude;
                if (distance > farthest) { farthest = distance; first = i; second = j; }
            }
            List<Vector2> result;
            do
            {
                var arcA = new List<Vector2>();
                for (int i = first; ; i = (i + 1) % points.Count) { arcA.Add(points[i]); if (i == second) break; }
                var arcB = new List<Vector2>();
                for (int i = second; ; i = (i + 1) % points.Count) { arcB.Add(points[i]); if (i == first) break; }
                List<Vector2> a = SimplifyOpen(arcA, tolerance), b = SimplifyOpen(arcB, tolerance);
                a.RemoveAt(a.Count - 1); b.RemoveAt(b.Count - 1); a.AddRange(b); result = a;
                tolerance *= 1.25f;
            }
            while (result.Count > maxCount);
            return result;
        }

        static List<Vector2> SimplifyOpen(List<Vector2> points, float epsilon)
        {
            if (points.Count <= 2) return new List<Vector2>(points);
            float maxDistance = 0f; int index = 0;
            for (int i = 1; i < points.Count - 1; i++)
            {
                float distance = DistanceToSegment(points[i], points[0], points[points.Count - 1]);
                if (distance > maxDistance) { maxDistance = distance; index = i; }
            }
            if (maxDistance <= epsilon) return new List<Vector2> { points[0], points[points.Count - 1] };
            List<Vector2> left = SimplifyOpen(points.GetRange(0, index + 1), epsilon);
            List<Vector2> right = SimplifyOpen(points.GetRange(index, points.Count - index), epsilon);
            left.RemoveAt(left.Count - 1); left.AddRange(right); return left;
        }

        static bool PointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                Vector2 a = polygon[i], b = polygon[j];
                if (((a.y > point.y) != (b.y > point.y)) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }

        static float DistanceToPolygon(Vector2 point, List<Vector2> polygon)
        {
            float best = float.MaxValue;
            for (int i = 0; i < polygon.Count; i++) best = Mathf.Min(best, DistanceToSegment(point, polygon[i], polygon[(i + 1) % polygon.Count]));
            return best;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 d = b - a; float t = d.sqrMagnitude > 1e-12f ? Mathf.Clamp01(Vector2.Dot(point - a, d) / d.sqrMagnitude) : 0f;
            return Vector2.Distance(point, a + d * t);
        }

        static float SignedArea(List<Vector2> points)
        {
            float area = 0f; for (int i = 0; i < points.Count; i++) { Vector2 a = points[i], b = points[(i + 1) % points.Count]; area += a.x * b.y - b.x * a.y; } return area * .5f;
        }
        static float Perimeter(List<Vector2> points, bool closed)
        {
            float length = 0f; for (int i = 1; i < points.Count; i++) length += Vector2.Distance(points[i - 1], points[i]); if (closed && points.Count > 1) length += Vector2.Distance(points[points.Count - 1], points[0]); return length;
        }
        static string SanitizeName(string value)
        {
            string name = string.IsNullOrWhiteSpace(value) ? "Water" : value.Trim(); foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_'); return name;
        }
    }
}
