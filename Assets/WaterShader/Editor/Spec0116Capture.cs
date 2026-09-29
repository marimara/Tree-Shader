using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Meganeura.Water.Editor
{
    // Deterministic GPU capture in the technical scene. All fixture objects and
    // materials are transient; no user scene, baker or material is overwritten.
    public static class Spec0116Capture
    {
        static GameObject root;
        static Camera camera;
        static Material material;
        static RenderTexture target;
        static Texture2D pixels;
        static readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        static string folder;
        static int index, count;
        static float fps, start;
        public static string Status = "Idle";

        public static void Start(string name, string shaderPath, int stage, float strength,
            int frames = 360, float rate = 30f, string sourceName = "", float startTime = 0f,
            string outputRoot = "Assets/WaterShader/Validation/SPEC-011.6/.frames",
            Action<Material> configure = null)
        {
            if (root != null) throw new InvalidOperationException("Capture already running.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path !=
                "Assets/WaterShader/Scenes/WaterShader_TestScene.unity")
                throw new InvalidOperationException("Open WaterShader_TestScene first.");
            var source = string.IsNullOrEmpty(sourceName) ? null : GameObject.Find(sourceName);
            if (!string.IsNullOrEmpty(sourceName) && source == null)
                throw new InvalidOperationException("Capture source not found: " + sourceName);
            folder = outputRoot + "/" + name;
            Directory.CreateDirectory(folder);
            root = new GameObject("SPEC0116_TransientCapture") { hideFlags = HideFlags.HideAndDontSave };
            var water = new GameObject("Water", typeof(MeshFilter), typeof(MeshRenderer));
            water.transform.SetParent(root.transform);
            water.transform.position = new Vector3(0, 0, 1000);
            Mesh mesh;
            Material original;
            if (source != null)
            {
                mesh = UnityEngine.Object.Instantiate(source.GetComponent<MeshFilter>().sharedMesh);
                var vertices = mesh.vertices;
                Vector3 center = source.GetComponent<Renderer>().bounds.center;
                for (int i = 0; i < vertices.Length; i++) vertices[i] = source.transform.TransformPoint(vertices[i]) - center;
                mesh.vertices = vertices; mesh.RecalculateBounds();
                original = source.GetComponent<Renderer>().sharedMaterial;
                var baker = source.GetComponent<StylizedWaterFlowBaker>();
                if (baker != null)
                    foreach (var collider in baker.ExplicitObstacles)
                    {
                        if (collider == null) continue;
                        var filter = collider.GetComponent<MeshFilter>();
                        var renderer = collider.GetComponent<Renderer>();
                        if (filter == null || renderer == null) continue;
                        var obstacle = new GameObject("Obstacle", typeof(MeshFilter), typeof(MeshRenderer));
                        obstacle.transform.SetParent(root.transform);
                        obstacle.transform.position = water.transform.position;
                        var obstacleMesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
                        var obstacleVertices = obstacleMesh.vertices;
                        for (int i = 0; i < obstacleVertices.Length; i++)
                            obstacleVertices[i] = collider.transform.TransformPoint(obstacleVertices[i]) - center;
                        obstacleMesh.vertices = obstacleVertices;
                        obstacleMesh.RecalculateBounds(); obstacleMesh.RecalculateNormals();
                        owned.Add(obstacleMesh);
                        obstacle.GetComponent<MeshFilter>().sharedMesh = obstacleMesh;
                        obstacle.GetComponent<MeshRenderer>().sharedMaterial = renderer.sharedMaterial;
                    }
            }
            else
            {
                mesh = new Mesh();
                mesh.vertices = new[] { new Vector3(-10,0,-2), new Vector3(-10,0,2), new Vector3(10,0,2), new Vector3(10,0,-2) };
                mesh.triangles = new[] {0,1,2,0,2,3};
                mesh.uv = new[] {new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};
                mesh.uv2 = new[] {new Vector2(0,-1.265f),new Vector2(0,1.265f),new Vector2(4,1.265f),new Vector2(4,-1.265f)};
                mesh.SetUVs(2,new List<Vector4>{new Vector4(.25f,0,0,.395f),new Vector4(.25f,0,0,.395f),new Vector4(.25f,0,0,.395f),new Vector4(.25f,0,0,.395f)});
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                original = AssetDatabase.LoadAssetAtPath<Material>("Assets/WaterShader/Materials/MAT_Water_Uniform_Matched.mat");
            }
            owned.Add(mesh);
            material = new Material(original) { shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath) };
            owned.Add(material);
            if (source == null)
            {
                var map = new Texture2D(2,2,TextureFormat.RGBAFloat,false,true);
                map.SetPixels(new[] {new Color(1,.5f,strength,1),new Color(1,.5f,strength,1),new Color(1,.5f,strength,1),new Color(1,.5f,strength,1)});
                map.Apply(); owned.Add(map); material.SetTexture("_FlowMap",map);
            }
            material.SetFloat("_UseFlowMap",1); material.SetFloat("_UseFlowCoordinates",1);
            material.SetFloat("_FlowMapMinStrength",strength); material.SetFloat("_FlowMapMaxStrength",strength);
            material.SetFloat("_FlowStrength",strength); material.SetFloat("_FlowDebugMode",0);
            if (material.HasProperty("_DiagnosticStage")) material.SetFloat("_DiagnosticStage",stage);
            if (shaderPath.EndsWith("Baseline.shader", StringComparison.Ordinal))
            {
                material.SetFloat("_PatternScale",2.61f); material.SetFloat("_PatternStretch",4.9f);
                material.SetFloat("_PatternThreshold",.4f); material.SetFloat("_NoiseContrast",1.55f);
                material.SetFloat("_PatternSoftness",.085f); material.SetFloat("_PatternStrength",.72f);
                material.SetColor("_PatternColor",new Color(.58f,1.15f,1.35f,.92f));
            }
            configure?.Invoke(material);
            // Flat matched base isolates the surface pattern from scene depth.
            material.SetColor("_ShallowColor",new Color(.02f,.55f,.75f,1));
            material.SetColor("_DeepColor",new Color(.02f,.55f,.75f,1)); material.SetFloat("_Opacity",1);
            water.GetComponent<MeshFilter>().sharedMesh=mesh; water.GetComponent<MeshRenderer>().sharedMaterial=material;
            var cg = new GameObject("Camera",typeof(Camera)); cg.transform.SetParent(root.transform);
            camera=cg.GetComponent<Camera>(); camera.enabled=false;
            camera.transform.position=new Vector3(0,30,1000); camera.transform.rotation=Quaternion.Euler(90,0,0);
            camera.orthographic=true; camera.orthographicSize=Mathf.Max(3.5f,mesh.bounds.extents.z+1);
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.035f,.055f,.075f);
            camera.farClipPlane=60;
            target=new RenderTexture(960,480,24,RenderTextureFormat.ARGB32); target.Create(); owned.Add(target);
            camera.targetTexture=target; camera.aspect=2;
            camera.orthographicSize=Mathf.Max(camera.orthographicSize,(mesh.bounds.extents.x+1)/2);
            pixels=new Texture2D(960,480,TextureFormat.RGB24,false); owned.Add(pixels);
            index=0; count=frames; fps=rate; start=startTime;
            Status=name+" 0/"+count;
            EditorApplication.update+=Tick;
        }

        static void Tick()
        {
            try
            {
                material.SetFloat("_ValidationTime",start+index/fps);
                camera.Render();
                var previous=RenderTexture.active; RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,960,480),0,0); pixels.Apply(); RenderTexture.active=previous;
                File.WriteAllBytes(folder+"/"+index.ToString("D5")+".png",pixels.EncodeToPNG());
                index++; Status=folder+" "+index+"/"+count;
                if(index>=count)
                {
                    string info = "GPU capture; fixed camera; explicit shader time\nShader: " + material.shader.name +
                        "\nFrames: " + count + "\nFPS: " + fps + "\nStart time: " + start + "\n";
                    foreach (string property in new[] { "_FlowSpeed", "_FlowStrength", "_FlowMapMinStrength", "_FlowMapMaxStrength",
                        "_PatternScale", "_PatternStretch", "_PatternThreshold", "_PrimaryMarkWidth", "_SecondaryMarkStrength" })
                        if (material.HasProperty(property)) info += property + ": " + material.GetFloat(property) + "\n";
                    File.WriteAllText(folder + "/Capture.txt", info);
                    Status="Complete: "+folder; Cleanup();
                }
            }
            catch(Exception e) { Status="FAILED: "+e; Cleanup(); Debug.LogException(e); }
        }

        public static void Cleanup()
        {
            EditorApplication.update-=Tick;
            if(root!=null) UnityEngine.Object.DestroyImmediate(root);
            foreach(var obj in owned) if(obj!=null) UnityEngine.Object.DestroyImmediate(obj);
            owned.Clear(); root=null;
        }

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Straight")]
        public static void CaptureStraight() => Start("Straight_Rerun", ProductionCopy(),7,.66f,540,30);

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Existing Obstacle")]
        public static void CaptureObstacle() => Start("Obstacle_Rerun", ProductionCopy(),7,.66f,360,20,"CurvedWater_Channel");

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Calm")]
        public static void CaptureCalm() => Start("Calm_Rerun", ProductionCopy(),7,0,720,10);

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Fast")]
        public static void CaptureFast() => Start("Fast_Rerun", ProductionCopy(),7,1,360,20);

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Scale 1")]
        public static void CaptureScale1() => CaptureTransform(false);

        [MenuItem("Tools/WaterShader/SPEC-011.6/Capture Non-uniform Scale")]
        public static void CaptureNonUniform() => CaptureTransform(true);

        static void CaptureTransform(bool scaled)
        {
            string suffix=scaled?"NonUniformSource":"Scale1Source";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/WaterShader/Generated/Meshes/MESH_FlowCoordinates_SPEC0116_"+suffix+".asset");
            var map=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WaterShader/Generated/FlowMaps/T_SPEC0116_"+suffix+".asset");
            if(mesh==null||map==null)throw new InvalidOperationException("SPEC-011.6 equivalent-transform fixture assets are missing.");
            var fixture=new GameObject("SPEC0116_TransformCaptureSource",typeof(MeshFilter),typeof(MeshRenderer));
            fixture.hideFlags=HideFlags.HideAndDontSave;
            var sourceMaterial=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/WaterShader/Materials/MAT_Water_Uniform_Matched.mat"));
            try
            {
                fixture.transform.localScale=scaled?new Vector3(10,1,2):Vector3.one;
                fixture.transform.rotation=Quaternion.Euler(0,17,0);
                fixture.GetComponent<MeshFilter>().sharedMesh=mesh;
                sourceMaterial.SetTexture("_FlowMap",map);
                fixture.GetComponent<Renderer>().sharedMaterial=sourceMaterial;
                Start(suffix+"_Rerun",ProductionCopy(),7,.66f,360,20,fixture.name);
            }
            finally { UnityEngine.Object.DestroyImmediate(fixture); UnityEngine.Object.DestroyImmediate(sourceMaterial); }
        }

        static string ProductionCopy()
        {
            const string path="Assets/WaterShader/Validation/SPEC-011.6/ProductionCapture.shader";
            string shader=File.ReadAllText("Assets/WaterShader/Shaders/StylizedWater.shader")
                .Replace("Shader \"Meganeura/Water/Stylized Water\"","Shader \"Hidden/Water/SPEC0116ProductionCapture\"")
                .Replace("_Time.y","_ValidationTime")
                .Replace("half4 _ShallowColor;","float _ValidationTime; half4 _ShallowColor;")
                .Replace("        _ShallowColor (","        _ValidationTime (\"Validation Time\", Float) = 0\n        _ShallowColor (");
            File.WriteAllText(path,shader);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            return path;
        }
    }
}
