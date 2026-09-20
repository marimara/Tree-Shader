using System.Collections.Generic;
using UnityEngine;

namespace Meganeura.Water
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class StylizedWaterFlowBaker : MonoBehaviour
    {
        public enum BakeResolution { R128 = 128, R256 = 256, R512 = 512, R1024 = 1024 }
        public enum StrengthMode { Constant, PathFalloff }
        public enum BoundaryMode { BakeBounds, ExplicitPolygon }

        [Header("Target")]
        [SerializeField] Renderer targetRenderer;
        [SerializeField] Material targetMaterial;
        [SerializeField, Tooltip("Original authoring mesh. Auto Setup stores this before generated flow-coordinate meshes are assigned.")]
        Mesh sourceMesh;
        [SerializeField] string outputName = "T_FlowMap_Water";

        [Header("Quick Setup Options")]
        [SerializeField, HideInInspector] bool autoFitBakeBounds = true;
        [SerializeField, HideInInspector] bool autoExtractBoundary = true;
        [SerializeField, HideInInspector] bool autoGenerateCenterline = true;
        [SerializeField, HideInInspector] bool autoChooseResolution = true;
        [SerializeField, HideInInspector] bool autoEstimateFlowCoordinates = true;
        [SerializeField, HideInInspector] bool autoEstimateSteeringDistance = true;

        [Header("Bake Region (local XZ)")]
        [SerializeField] Vector2 bakeCenter;
        [SerializeField] Vector2 bakeSize = new Vector2(22f, 11f);
        [SerializeField] BakeResolution resolution = BakeResolution.R256;
        [SerializeField] bool generateCompatibleFlowCoordinates = true;
        [SerializeField] Vector2 flowCoordinateWorldScale = new Vector2(7.2f, 2.2f);

        [Header("Continuous Centerline (local space)")]
        [SerializeField] List<Vector3> controlPoints = new List<Vector3>
        {
            new Vector3(-10f, 0f, -3f),
            new Vector3(-5f, 0f, -3f),
            new Vector3(0f, 0f, 0f),
            new Vector3(5f, 0f, 3f),
            new Vector3(10f, 0f, 3f)
        };

        [Header("Flow Strength (B)")]
        [SerializeField] StrengthMode strengthMode = StrengthMode.Constant;
        [SerializeField, Range(0f, 1f)] float constantStrength = .62f;
        [SerializeField, Range(0f, 1f)] float startStrength = .82f;
        [SerializeField, Range(0f, 1f)] float endStrength = .28f;
        [SerializeField, Range(0f, 1f)] float falloffStart = .55f;

        [Header("SPEC-011 Boundary")]
        [SerializeField] bool useBoundaryAndObstacles;
        [SerializeField] BoundaryMode boundaryMode = BoundaryMode.BakeBounds;
        [SerializeField] List<Vector3> boundaryPoints = new List<Vector3>();
        [SerializeField] LayerMask obstacleLayers;
        [SerializeField] List<Collider> explicitObstacles = new List<Collider>();
        [SerializeField, Min(.01f)] float steeringDistance = 1.1f;
        [SerializeField, Range(0f, 2f)] float steeringStrength = .9f;

        [Header("Diagnostics")]
        [SerializeField] bool showPath = true;
        [SerializeField] bool showBounds = true;
        [SerializeField] bool showBoundary = true;
        [SerializeField] bool showDirectionArrows = true;
        [SerializeField, Range(4, 32)] int arrowCount = 14;
        [SerializeField, HideInInspector] Texture2D bakedFlowMap;
        [SerializeField, HideInInspector] Texture2D bakeDiagnostics;
        [SerializeField, HideInInspector] Mesh generatedFlowMesh;
        [SerializeField, HideInInspector] string lastBakeSummary;
        [SerializeField, HideInInspector] string lastAutoSetupSummary;
        [SerializeField, HideInInspector] bool lastAutoSetupHadWarnings;

        public Renderer TargetRenderer { get => targetRenderer; set => targetRenderer = value; }
        public Material TargetMaterial { get => targetMaterial; set => targetMaterial = value; }
        public Mesh SourceMesh { get => sourceMesh; set => sourceMesh = value; }
        public string OutputName { get => outputName; set => outputName = value; }
        public bool AutoFitBakeBounds => autoFitBakeBounds;
        public bool AutoExtractBoundary => autoExtractBoundary;
        public bool AutoGenerateCenterline => autoGenerateCenterline;
        public bool AutoChooseResolution => autoChooseResolution;
        public bool AutoEstimateFlowCoordinates => autoEstimateFlowCoordinates;
        public bool AutoEstimateSteeringDistance => autoEstimateSteeringDistance;
        public Vector2 BakeCenter { get => bakeCenter; set => bakeCenter = value; }
        public Vector2 BakeSize { get => bakeSize; set => bakeSize = value; }
        public BakeResolution Resolution { get => resolution; set => resolution = value; }
        public bool GenerateCompatibleFlowCoordinates { get => generateCompatibleFlowCoordinates; set => generateCompatibleFlowCoordinates = value; }
        public Vector2 FlowCoordinateWorldScale { get => flowCoordinateWorldScale; set => flowCoordinateWorldScale = value; }
        public List<Vector3> ControlPoints => controlPoints;
        public StrengthMode FlowStrengthMode { get => strengthMode; set => strengthMode = value; }
        public float ConstantStrength { get => constantStrength; set => constantStrength = Mathf.Clamp01(value); }
        public float StartStrength { get => startStrength; set => startStrength = Mathf.Clamp01(value); }
        public float EndStrength { get => endStrength; set => endStrength = Mathf.Clamp01(value); }
        public float FalloffStart { get => falloffStart; set => falloffStart = Mathf.Clamp01(value); }
        public bool UseBoundaryAndObstacles { get => useBoundaryAndObstacles; set => useBoundaryAndObstacles = value; }
        public BoundaryMode WaterBoundaryMode { get => boundaryMode; set => boundaryMode = value; }
        public List<Vector3> BoundaryPoints => boundaryPoints;
        public LayerMask ObstacleLayers { get => obstacleLayers; set => obstacleLayers = value; }
        public List<Collider> ExplicitObstacles => explicitObstacles;
        public float SteeringDistance { get => steeringDistance; set => steeringDistance = Mathf.Max(.01f, value); }
        public float SteeringStrength { get => steeringStrength; set => steeringStrength = Mathf.Clamp(value, 0f, 2f); }
        public Texture2D BakedFlowMap => bakedFlowMap;
        public Texture2D BakeDiagnostics => bakeDiagnostics;
        public Mesh GeneratedFlowMesh => generatedFlowMesh;
        public string LastBakeSummary => lastBakeSummary;
        public string LastAutoSetupSummary => lastAutoSetupSummary;
        public bool LastAutoSetupHadWarnings => lastAutoSetupHadWarnings;
        public bool ShowPath => showPath;
        public bool ShowBounds => showBounds;
        public bool ShowBoundary => showBoundary;
        public bool ShowDirectionArrows => showDirectionArrows;
        public int ArrowCount => arrowCount;

        public Vector3 EvaluateLocal(float normalizedT)
        {
            if (controlPoints == null || controlPoints.Count == 0) return Vector3.zero;
            if (controlPoints.Count == 1) return controlPoints[0];
            float scaled = Mathf.Clamp01(normalizedT) * (controlPoints.Count - 1);
            int segment = Mathf.Min(Mathf.FloorToInt(scaled), controlPoints.Count - 2);
            float t = scaled - segment;
            Vector3 p0 = controlPoints[Mathf.Max(0, segment - 1)];
            Vector3 p1 = controlPoints[segment];
            Vector3 p2 = controlPoints[segment + 1];
            Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Count - 1, segment + 2)];
            return CatmullRom(p0, p1, p2, p3, t);
        }

        public Vector3 EvaluateLocalTangent(float normalizedT)
        {
            if (controlPoints == null || controlPoints.Count < 2) return Vector3.right;
            float scaled = Mathf.Clamp01(normalizedT) * (controlPoints.Count - 1);
            int segment = Mathf.Min(Mathf.FloorToInt(scaled), controlPoints.Count - 2);
            float t = scaled - segment;
            Vector3 p0 = controlPoints[Mathf.Max(0, segment - 1)];
            Vector3 p1 = controlPoints[segment];
            Vector3 p2 = controlPoints[segment + 1];
            Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Count - 1, segment + 2)];
            Vector3 tangent = CatmullRomDerivative(p0, p1, p2, p3, t);
            tangent.y = 0f;
            return tangent.sqrMagnitude > 1e-8f ? tangent.normalized : Vector3.right;
        }

        public Vector3 LocalFromUV(Vector2 uv)
        {
            Vector2 p = bakeCenter + Vector2.Scale(uv - Vector2.one * .5f, bakeSize);
            return new Vector3(p.x, 0f, p.y);
        }

        public Vector2 UVFromLocal(Vector3 local)
        {
            return new Vector2(
                (local.x - bakeCenter.x) / Mathf.Max(.0001f, bakeSize.x) + .5f,
                (local.z - bakeCenter.y) / Mathf.Max(.0001f, bakeSize.y) + .5f);
        }

        public float EvaluateStrength(float pathT)
        {
            if (strengthMode == StrengthMode.Constant) return constantStrength;
            float q = Mathf.InverseLerp(falloffStart, 1f, pathT);
            return Mathf.Lerp(startStrength, endStrength, q * q * (3f - 2f * q));
        }

        public bool HasValidPath(out string error)
        {
            if (controlPoints == null || controlPoints.Count < 2)
            {
                error = "At least two centerline control points are required.";
                return false;
            }
            if (bakeSize.x <= .0001f || bakeSize.y <= .0001f)
            {
                error = "Bake Size must be positive on both axes.";
                return false;
            }
            for (int i = 1; i < controlPoints.Count; i++)
                if ((controlPoints[i] - controlPoints[i - 1]).sqrMagnitude < 1e-8f)
                {
                    error = $"Centerline points {i - 1} and {i} are coincident.";
                    return false;
                }
            error = null;
            return true;
        }

        public void SetBakeResult(Texture2D texture, string summary)
        {
            bakedFlowMap = texture;
            lastBakeSummary = summary;
        }

        public void SetGeneratedFlowMesh(Mesh mesh)
        {
            generatedFlowMesh = mesh;
        }

        public void SetBakeDiagnostics(Texture2D texture)
        {
            bakeDiagnostics = texture;
        }

        public void SetAutoSetupResult(string summary, bool hadWarnings)
        {
            lastAutoSetupSummary = summary;
            lastAutoSetupHadWarnings = hadWarnings;
        }

        public void ReverseFlowDirection()
        {
            controlPoints.Reverse();
        }

        static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return .5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t2 + (-a + 3f * b - 3f * c + d) * t3);
        }

        static Vector3 CatmullRomDerivative(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float t2 = t * t;
            return .5f * ((-a + c) + 2f * (2f * a - 5f * b + 4f * c - d) * t + 3f * (-a + 3f * b - 3f * c + d) * t2);
        }
    }
}
