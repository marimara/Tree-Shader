using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Meganeura.Water
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class WaterFlowData : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh generatedVertexStream;

        [Header("Source Mesh")] [SerializeField]
        private Mesh sourceMesh;

        [SerializeField] private int sourceVertexCount;

        [Header("Flow Data")] [SerializeField] private List<Vector2> flowDirections = new();

        [Header("Data")] [SerializeField] private int dataVersion = 1;

        private bool HasFlowData()
        {
            return flowDirections != null && flowDirections.Count > 0;
        }

        private Mesh GetCurrentMesh()
        {
            if (meshFilter == null)
                return null;

            return meshFilter.sharedMesh;
        }

        private void Reset()
        {
            RefreshSourceMesh();
        }
        
        private void OnEnable()
        {
            CacheComponents();

            if (HasFlowData() && ValidateTopology())
            {
                RebuildVertexStream();
            }
        }
        private void OnDisable()
        {
            CacheComponents();
            ClearGeneratedVertexStream();
        }

        private void OnValidate()
        {
            RefreshSourceMesh();
        }

        private void CacheComponents()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void ReadSourceMesh()
        {
            Mesh currentMesh = GetCurrentMesh();

            if (currentMesh == null)
            {
                if (!HasFlowData())
                {
                    sourceMesh = null;
                    sourceVertexCount = 0;
                }

                return;
            }

            if (!HasFlowData())
            {
                sourceMesh = currentMesh;
                sourceVertexCount = currentMesh.vertexCount;
            }
        }

        private bool HasValidUV0(Mesh mesh)
        {
            if (mesh == null)
                return false;

            List<Vector4> uv0 = new();
            mesh.GetUVs(0, uv0);

            return uv0.Count == mesh.vertexCount;
        }
        private bool HasExistingUV3(Mesh mesh)
        {
            if (mesh == null)
                return false;

            List<Vector4> uv3 = new();
            mesh.GetUVs(3, uv3);

            return uv3.Count > 0;
        }

        private bool HasValidNormals(Mesh mesh)
        {
            if (mesh == null)
                return false;

            Vector3[] normals = mesh.normals;

            return normals != null &&
                   normals.Length == mesh.vertexCount;
        }

        private bool HasValidTangents(Mesh mesh)
        {
            if (mesh == null)
                return false;

            Vector4[] tangents = mesh.tangents;

            return tangents != null &&
                   tangents.Length == mesh.vertexCount;
        }

        private void ValidateSourceMesh()
        {
            Mesh currentMesh = GetCurrentMesh();

            if (currentMesh == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] No source mesh found on '{name}'.",
                    this
                );

                return;
            }

            bool validUV0 = HasValidUV0(currentMesh);
            bool validNormals = HasValidNormals(currentMesh);
            bool validTangents = HasValidTangents(currentMesh);
            bool hasExistingUV3 = HasExistingUV3(currentMesh);

            if (!validUV0)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Mesh '{currentMesh.name}' does not have a valid UV0 channel.",
                    this
                );
            }

            if (!validNormals)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Mesh '{currentMesh.name}' does not have valid normals.",
                    this
                );
            }

            if (!validTangents)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Mesh '{currentMesh.name}' does not have valid tangents.",
                    this
                );
            }
            if (hasExistingUV3)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Mesh '{currentMesh.name}' already contains data " +
                    $"in UV Channel 3. This channel is reserved for Water Flow.",
                    this
                );
            }

            if (validUV0 &&
                validNormals &&
                validTangents &&
                !hasExistingUV3)
            {
                Debug.Log(
                    $"[{nameof(WaterFlowData)}] Mesh '{currentMesh.name}' is valid for Flow Data. " +
                    $"Vertices: {currentMesh.vertexCount}.",
                    this
                );
            }
        }
        

        private bool ValidateTopology()
        {
            if (!HasFlowData())
                return true;

            Mesh currentMesh = GetCurrentMesh();

            if (currentMesh == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Flow Data exists, but no mesh is currently assigned.",
                    this
                );

                return false;
            }

            if (currentMesh != sourceMesh)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] The current mesh '{currentMesh.name}' is different " +
                    $"from the mesh used to create the existing Flow Data '{sourceMesh?.name}'.",
                    this
                );

                return false;
            }

            if (currentMesh.vertexCount != sourceVertexCount)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Topology mismatch on mesh '{currentMesh.name}'. " +
                    $"Stored vertex count: {sourceVertexCount}. " +
                    $"Current vertex count: {currentMesh.vertexCount}.",
                    this
                );

                return false;
            }

            if (flowDirections.Count != sourceVertexCount)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Flow Data count does not match the stored vertex count. " +
                    $"Flow entries: {flowDirections.Count}. " +
                    $"Expected: {sourceVertexCount}.",
                    this
                );

                return false;
            }

            return true;
        }

        private void ClearGeneratedVertexStream()
        {
            if (meshRenderer == null)
                return;

            Mesh streamToDestroy = generatedVertexStream;

            if (streamToDestroy == null)
            {
                Mesh rendererStream = meshRenderer.additionalVertexStreams;

                if (rendererStream != null &&
                    rendererStream.name == $"{name}_WaterFlowStream")
                {
                    streamToDestroy = rendererStream;
                }
            }

            if (meshRenderer.additionalVertexStreams == streamToDestroy)
            {
                meshRenderer.additionalVertexStreams = null;
            }

            if (streamToDestroy != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(streamToDestroy);
                }
                else
                {
                    DestroyImmediate(streamToDestroy);
                }
            }

            generatedVertexStream = null;
        }
        
        private void RebuildVertexStream()
        {
            CacheComponents();

            if (!ValidateTopology())
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Cannot build vertex stream because Flow topology is invalid.",
                    this
                );

                return;
            }

            Mesh currentMesh = GetCurrentMesh();

            if (currentMesh == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Cannot build vertex stream because no source mesh is assigned.",
                    this
                );

                return;
            }

            ClearGeneratedVertexStream();

            generatedVertexStream = new Mesh
            {
                name = $"{name}_WaterFlowStream",
                hideFlags = HideFlags.HideAndDontSave
            };

            generatedVertexStream.vertices = currentMesh.vertices;

            if (generatedVertexStream.vertexCount != currentMesh.vertexCount)
            {
                Debug.LogError(
                    $"[{nameof(WaterFlowData)}] Generated vertex stream has an incompatible vertex count.",
                    this
                );

                ClearGeneratedVertexStream();
                return;
            }

            generatedVertexStream.SetUVs(3, flowDirections);
            
            meshRenderer.additionalVertexStreams = generatedVertexStream;
            
            Debug.Log(
                $"[{nameof(WaterFlowData)}] Additional Vertex Stream created and applied. " +
                $"Vertices: {generatedVertexStream.vertexCount}, " +
                $"Flow entries: {flowDirections.Count}, " +
                $"UV Channel: 3.",
                this
            );
        }

        [ContextMenu("Initialize Flow Data")]
        private void InitializeFlowData()
        {
            CacheComponents();

            Mesh currentMesh = GetCurrentMesh();

            if (currentMesh == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Cannot initialize Flow Data because no mesh is assigned.",
                    this
                );

                return;
            }

            if (!HasValidUV0(currentMesh) ||
                !HasValidNormals(currentMesh) ||
                !HasValidTangents(currentMesh) ||
                HasExistingUV3(currentMesh))
            {
                Debug.LogWarning(
                    $"[{nameof(WaterFlowData)}] Cannot initialize Flow Data because mesh '{currentMesh.name}' " +
                    $"does not meet the required mesh validation.",
                    this
                );

                return;
            }

            sourceMesh = currentMesh;
            sourceVertexCount = currentMesh.vertexCount;

            flowDirections = new List<Vector2>(sourceVertexCount);

            for (int i = 0; i < sourceVertexCount; i++)
            {
                flowDirections.Add(Vector2.zero);
            }

            Debug.Log(
                $"[{nameof(WaterFlowData)}] Flow Data initialized for mesh '{sourceMesh.name}'. " +
                $"Created {flowDirections.Count} flow entries.",
                this
            );
            
            
            RebuildVertexStream();
        }

        private void RefreshSourceMesh()
        {
            CacheComponents();
            ReadSourceMesh();
            ValidateSourceMesh();

            if (HasFlowData())
            {
                ValidateTopology();
            }
        }

        [ContextMenu("Validate Flow Topology")]
        private void ValidateFlowTopologyContext()
        {
            CacheComponents();

            bool valid = ValidateTopology();

            if (valid)
            {
                Debug.Log(
                    $"[{nameof(WaterFlowData)}] Flow topology is valid.",
                    this
                );
            }
        }

        [ContextMenu("Validate Source Mesh")]
        private void ValidateSourceMeshContext()
        {
            RefreshSourceMesh();
        }
    }
}