using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Meganeura.Water;

namespace Meganeura.Water.Editor
{
    /// <summary>Repeatable Editor-only regression fixture for SPEC-011.5 transform equivalence.</summary>
    public static class Spec0115TransformValidation
    {
        const string RootName = "SPEC0115_TransformAware_Validation";

        [MenuItem("Tools/WaterShader/SPEC-011.5/Validate Transform Awareness")]
        public static void Validate()
        {
            Cleanup();
            var root = new GameObject(RootName);
            Shader shader = Shader.Find("Meganeura/Water/Stylized Water");
            if (shader == null) throw new InvalidOperationException("Stylized Water shader not found.");

            Mesh finalSizeMesh = BuildQuad("SPEC0115_FinalSize_Source", 10f, 2f);
            Mesh unitMesh = BuildQuad("SPEC0115_Unit_Source", 1f, 1f);
            Material finalSizeMaterial = BuildMaterial(shader, "SPEC0115_FinalSize_Material");
            Material nonUniformMaterial = BuildMaterial(shader, "SPEC0115_NonUniform_Material");
            StylizedWaterFlowBaker finalSize = CreateWater(root.transform, "SPEC0115_FinalSize", finalSizeMesh,
                finalSizeMaterial, new Vector3(0f, 0f, 40f), Vector3.one);
            StylizedWaterFlowBaker nonUniform = CreateWater(root.transform, "SPEC0115_NonUniform", unitMesh,
                nonUniformMaterial, new Vector3(0f, 0f, 47f), new Vector3(10f, 1f, 2f));

            WaterMeshAutoSetup.Run(finalSize);
            WaterMeshAutoSetup.Run(nonUniform);
            Vector2 initialScale = nonUniform.FlowCoordinateWorldScale;
            float initialSteering = nonUniform.SteeringDistance;
            StylizedWaterFlowBaker.BakeResolution initialResolution = nonUniform.Resolution;
            Vector2 initialBakeSize = nonUniform.BakeSize;

            nonUniform.transform.localScale = new Vector3(15f, 1f, 1f);
            WaterMeshAutoSetup.Run(nonUniform);
            Vector2 changedScale = nonUniform.FlowCoordinateWorldScale;
            float changedSteering = nonUniform.SteeringDistance;
            StylizedWaterFlowBaker.BakeResolution changedResolution = nonUniform.Resolution;
            Vector2 changedBakeSize = nonUniform.BakeSize;
            nonUniform.transform.localScale = new Vector3(10f, 1f, 2f);
            WaterMeshAutoSetup.Run(nonUniform);

            finalSize.ExplicitObstacles.Add(CreateObstacle(root.transform, finalSize, "SPEC0115_Obstacle_FinalSize", .72f));
            nonUniform.ExplicitObstacles.Add(CreateObstacle(root.transform, nonUniform, "SPEC0115_Obstacle_NonUniform", .36f));
            Physics.SyncTransforms();
            finalSize.OutputName = "T_SPEC0115_Transform_FinalSize";
            nonUniform.OutputName = "T_SPEC0115_Transform_NonUniform";
            Texture2D mapA = StylizedWaterFlowBakerUtility.Bake(finalSize, true);
            Texture2D mapB = StylizedWaterFlowBakerUtility.Bake(nonUniform, true);

            Vector2 uvRangeA = UVRange(finalSize.GeneratedFlowMesh.uv2);
            Vector2 uvRangeB = UVRange(nonUniform.GeneratedFlowMesh.uv2);
            Vector2 rgDelta = FlowMapDelta(mapA, mapB);
            Vector2 velocityDelta = ChartVelocityDelta(finalSize.GeneratedFlowMesh, mapA, nonUniform.GeneratedFlowMesh, mapB);
            Vector3Int traceA = TraceObstacle(finalSize);
            Vector3Int traceB = TraceObstacle(nonUniform);
            bool channelA = UsesChannelCoordinates(finalSizeMaterial);
            bool channelB = UsesChannelCoordinates(nonUniformMaterial);

            Require(channelA && channelB, "Bake And Assign did not enable channel coordinates.");
            Require(finalSize.Resolution == nonUniform.Resolution, "Equivalent world surfaces selected different resolutions.");
            Require(Vector2.Distance(finalSize.FlowCoordinateWorldScale, nonUniform.FlowCoordinateWorldScale) <= .2f,
                "Equivalent world surfaces produced incompatible Flow Coordinate World Scale.");
            Require(Mathf.Abs(finalSize.SteeringDistance - nonUniform.SteeringDistance) <= .15f,
                "Equivalent world surfaces produced incompatible Steering Distance.");
            Require(Vector2.Distance(uvRangeA, uvRangeB) <= .16f,
                "Generated physical channel-coordinate density is inconsistent.");
            Require(velocityDelta.x <= .02f && velocityDelta.y <= .08f,
                "Equivalent surfaces produced inconsistent channel-coordinate motion vectors.");
            Require(traceA.x >= 39 && traceB.x >= 39 && traceA.z == 0 && traceB.z == 0 && traceA.y == traceB.y,
                "Obstacle avoidance was not physically comparable between the two authoring representations.");
            Require(Mathf.Abs(changedScale.x - initialScale.x) >= .5f && Mathf.Abs(changedScale.y - initialScale.y) >= .2f &&
                Mathf.Abs(changedSteering - initialSteering) >= .1f,
                "Re-running Auto Setup after scale change did not update physical metrics.");
            Require((changedBakeSize - initialBakeSize).sqrMagnitude >= .0001f,
                "Transform-aware local padding did not respond to the scale change.");

            string report =
                "[SPEC-011.5 Transform validation] PASS\n" +
                $"Final-size: FlowScale {finalSize.FlowCoordinateWorldScale:F3}; Steering {finalSize.SteeringDistance:F3}; Resolution {(int)finalSize.Resolution}\n" +
                $"Non-uniform: FlowScale {nonUniform.FlowCoordinateWorldScale:F3}; Steering {nonUniform.SteeringDistance:F3}; Resolution {(int)nonUniform.Resolution}\n" +
                $"UV2 ranges: final {uvRangeA:F3}; non-uniform {uvRangeB:F3}; FlowMap RG mean/max delta {rgDelta.x:F5}/{rgDelta.y:F5}\n" +
                $"Chart motion-vector mean/max delta {velocityDelta.x:F5}/{velocityDelta.y:F5}\n" +
                $"Obstacle traces arrived/hit/escaped: final {traceA.x}/{traceA.y}/{traceA.z}; non-uniform {traceB.x}/{traceB.y}/{traceB.z}\n" +
                $"Scale re-run: FlowScale {initialScale:F3} -> {changedScale:F3}; Steering {initialSteering:F3} -> {changedSteering:F3}; " +
                $"Resolution {(int)initialResolution} -> {(int)changedResolution}; local BakeSize {initialBakeSize:F3} -> {changedBakeSize:F3}\n" +
                $"Channel coordinates enabled: {channelA}/{channelB}.";
            Debug.Log(report, root);
            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("Tools/WaterShader/SPEC-011.5/Cleanup Transform Validation")]
        public static void Cleanup()
        {
            GameObject root = GameObject.Find(RootName);
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            GameObject camera = GameObject.Find("SPEC0115_CaptureCamera");
            if (camera != null) UnityEngine.Object.DestroyImmediate(camera);
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/FlowMaps/T_SPEC0115_Transform_FinalSize.asset");
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/FlowMaps/T_SPEC0115_Transform_NonUniform.asset");
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/Meshes/MESH_FlowCoordinates_SPEC0115_FinalSize.asset");
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/Meshes/MESH_FlowCoordinates_SPEC0115_NonUniform.asset");
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/Diagnostics/T_FlowDebug_SPEC0115_FinalSize.asset");
            AssetDatabase.DeleteAsset("Assets/WaterShader/Generated/Diagnostics/T_FlowDebug_SPEC0115_NonUniform.asset");
        }

        static Mesh BuildQuad(string name, float halfX, float halfZ)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[]
            {
                new Vector3(-halfX, 0f, -halfZ), new Vector3(-halfX, 0f, halfZ),
                new Vector3(halfX, 0f, halfZ), new Vector3(halfX, 0f, -halfZ)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Material BuildMaterial(Shader shader, string name)
        {
            var material = new Material(shader) { name = name };
            material.SetFloat("_PatternSourceMode", 0f);
            material.SetFloat("_PatternScale", 2.2f);
            material.SetFloat("_PatternStretch", 4f);
            material.SetFloat("_FlowSpeed", .35f);
            material.SetFloat("_FlowStrength", .66f);
            material.SetFloat("_UseFlowCoordinates", 0f);
            return material;
        }

        static StylizedWaterFlowBaker CreateWater(Transform parent, string name, Mesh mesh, Material material,
            Vector3 position, Vector3 scale)
        {
            var water = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(StylizedWaterFlowBaker));
            water.transform.SetParent(parent, false);
            water.transform.position = position;
            water.transform.rotation = Quaternion.Euler(0f, 17f, 0f);
            water.transform.localScale = scale;
            water.GetComponent<MeshFilter>().sharedMesh = mesh;
            water.GetComponent<MeshRenderer>().sharedMaterial = material;
            return water.GetComponent<StylizedWaterFlowBaker>();
        }

        static Collider CreateObstacle(Transform parent, StylizedWaterFlowBaker baker, string name, float localZ)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obstacle.name = name;
            obstacle.transform.SetParent(parent, true);
            obstacle.transform.position = baker.transform.TransformPoint(new Vector3(0f, .25f, localZ));
            obstacle.transform.localScale = new Vector3(.9f, .55f, .9f);
            return obstacle.GetComponent<Collider>();
        }

        static Vector2 UVRange(Vector2[] coordinates)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector2 coordinate in coordinates)
            {
                min = Vector2.Min(min, coordinate);
                max = Vector2.Max(max, coordinate);
            }
            return max - min;
        }

        static Vector2 FlowMapDelta(Texture2D a, Texture2D b)
        {
            Color[] pixelsA = a.GetPixels(), pixelsB = b.GetPixels();
            int count = Mathf.Min(pixelsA.Length, pixelsB.Length);
            float sum = 0f, maximum = 0f;
            for (int i = 0; i < count; i++)
            {
                float delta = Mathf.Abs(pixelsA[i].r - pixelsB[i].r) + Mathf.Abs(pixelsA[i].g - pixelsB[i].g);
                sum += delta;
                maximum = Mathf.Max(maximum, delta);
            }
            return new Vector2(sum / Mathf.Max(1, count), maximum);
        }

        static Vector2 ChartVelocityDelta(Mesh meshA, Texture2D mapA, Mesh meshB, Texture2D mapB)
        {
            Vector2[] uvA = meshA.uv, uvB = meshB.uv;
            var framesA = new List<Vector4>();
            var framesB = new List<Vector4>();
            meshA.GetUVs(2, framesA);
            meshB.GetUVs(2, framesB);
            // Refinement can insert equivalent vertices in a different order. The
            // first four source vertices are preserved and are exact physical
            // correspondences between the two authoring representations.
            int count = Mathf.Min(4, uvA.Length, uvB.Length, framesA.Count, framesB.Count);
            float sum = 0f, maximum = 0f;
            for (int i = 0; i < count; i++)
            {
                Vector2 velocityA = ChartVelocity(mapA.GetPixelBilinear(uvA[i].x, uvA[i].y), framesA[i]);
                Vector2 velocityB = ChartVelocity(mapB.GetPixelBilinear(uvB[i].x, uvB[i].y), framesB[i]);
                float delta = Vector2.Distance(velocityA, velocityB);
                sum += delta;
                maximum = Mathf.Max(maximum, delta);
            }
            return new Vector2(sum / Mathf.Max(1, count), maximum);
        }

        static Vector2 ChartVelocity(Color flowSample, Vector4 frame)
        {
            Vector2 flow = new Vector2(flowSample.r * 2f - 1f, flowSample.g * 2f - 1f).normalized;
            Vector2 along = new Vector2(frame.x, frame.y);
            Vector2 across = new Vector2(frame.z, frame.w);
            float determinant = along.x * across.y - along.y * across.x;
            if (Mathf.Abs(determinant) <= 1e-12f) return Vector2.zero;
            return new Vector2(
                across.y * flow.x - across.x * flow.y,
                along.x * flow.y - along.y * flow.x).normalized;
        }

        static Vector3Int TraceObstacle(StylizedWaterFlowBaker baker)
        {
            int arrived = 0, hits = 0, escaped = 0;
            Collider obstacle = baker.ExplicitObstacles.Count > 0 ? baker.ExplicitObstacles[0] : null;
            for (int seed = 0; seed < 40; seed++)
            {
                Vector2 uv = new Vector2(.025f, Mathf.Lerp(.08f, .92f, (seed + .5f) / 40f));
                bool failed = false;
                for (int step = 0; step < 6000 && uv.x < .975f; step++)
                {
                    Vector3 local = baker.LocalFromUV(uv);
                    Vector3 world = baker.transform.TransformPoint(local);
                    if (obstacle != null && Vector3.Distance(obstacle.ClosestPoint(world), world) <= 1e-4f)
                    { hits++; failed = true; break; }
                    if (uv.y <= .015f || uv.y >= .985f) { escaped++; failed = true; break; }
                    Color flow = baker.BakedFlowMap.GetPixelBilinear(uv.x, uv.y);
                    Vector2 uvDirection = new Vector2(flow.r * 2f - 1f, flow.g * 2f - 1f).normalized;
                    Vector3 worldDirection = StylizedWaterFlowBakerUtility.WorldDirectionFromUV(baker, uvDirection);
                    Vector3 midpointWorld = world + worldDirection * .005f;
                    Vector3 midpointLocal = baker.transform.InverseTransformPoint(midpointWorld);
                    Vector2 midpointUV = baker.UVFromLocal(midpointLocal);
                    flow = baker.BakedFlowMap.GetPixelBilinear(midpointUV.x, midpointUV.y);
                    uvDirection = new Vector2(flow.r * 2f - 1f, flow.g * 2f - 1f).normalized;
                    worldDirection = StylizedWaterFlowBakerUtility.WorldDirectionFromUV(baker, uvDirection);
                    uv = baker.UVFromLocal(baker.transform.InverseTransformPoint(world + worldDirection * .01f));
                }
                if (!failed && uv.x >= .975f) arrived++;
            }
            return new Vector3Int(arrived, hits, escaped);
        }

        static bool UsesChannelCoordinates(Material material)
        {
            return material.HasProperty("_UseFlowCoordinates") && material.GetFloat("_UseFlowCoordinates") > .5f;
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("[SPEC-011.5 Transform validation] " + message);
        }
    }
}
