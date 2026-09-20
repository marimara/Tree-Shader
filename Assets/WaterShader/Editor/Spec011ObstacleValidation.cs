using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Meganeura.Water;
using Meganeura.Water.Editor;

// Isolated, repeatable regression case in the technical scene. No production setup is replaced.
public static class Spec011ObstacleValidation
{
    const string Root = "Assets/WaterShader/";
    const string Folder = Root + "Validation/SPEC-011/ObstacleCorrection";
    public static StylizedWaterFlowBaker Straight => GameObject.Find("SPEC011_StraightWater").GetComponent<StylizedWaterFlowBaker>();

    public static void Setup()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Root + "Scenes/WaterShader_TestScene.unity")
            throw new InvalidOperationException("Open the WaterShader technical scene first.");
        if (GameObject.Find("SPEC011_StraightWater") != null) return;
        var water = new GameObject("SPEC011_StraightWater", typeof(MeshFilter), typeof(MeshRenderer), typeof(StylizedWaterFlowBaker));
        water.transform.position = new Vector3(0, 0, 30);
        var mesh = new Mesh { name = "SPEC011_StraightSource" };
        mesh.vertices = new[] { new Vector3(-8,0,-2), new Vector3(-8,0,2), new Vector3(8,0,2), new Vector3(8,0,-2) };
        mesh.triangles = new[] { 0,1,2,0,2,3 };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        water.GetComponent<MeshFilter>().sharedMesh = mesh;
        string materialPath = Root + "Materials/MAT_Water_SPEC011_Straight.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
            AssetDatabase.CopyAsset(Root + "Materials/MAT_Water_FlowBaker_Constant.mat", materialPath);
        var b = water.GetComponent<StylizedWaterFlowBaker>();
        b.TargetRenderer = water.GetComponent<Renderer>();
        b.TargetMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        b.TargetRenderer.sharedMaterial = b.TargetMaterial;
        b.BakeSize = new Vector2(16, 5);
        b.ControlPoints.Clear(); b.ControlPoints.Add(new Vector3(-8,0,0)); b.ControlPoints.Add(new Vector3(8,0,0));
        b.UseBoundaryAndObstacles = true;
        b.WaterBoundaryMode = StylizedWaterFlowBaker.BoundaryMode.ExplicitPolygon;
        b.BoundaryPoints.AddRange(mesh.vertices);
        b.ConstantStrength = .62f; b.SteeringDistance = 1.15f; b.SteeringStrength = .9f;
        var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obstacle.name = "SPEC011_StraightCylinder";
        obstacle.transform.SetParent(water.transform, false);
        obstacle.transform.localPosition = new Vector3(0,.28f,0);
        obstacle.transform.localScale = new Vector3(1.15f,.55f,1.15f);
        obstacle.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/MAT_RiverBank_Technical.mat");
        UnityEngine.Object.DestroyImmediate(obstacle.GetComponent<Collider>());
        var cylinderCollider = obstacle.AddComponent<MeshCollider>();
        cylinderCollider.sharedMesh = obstacle.GetComponent<MeshFilter>().sharedMesh;
        cylinderCollider.convex = true;
        b.ExplicitObstacles.Add(cylinderCollider);
        Physics.SyncTransforms();
    }

    public static string BakeAndCapture(string label)
    {
        var b = Straight;
        b.OutputName = "T_FlowMap_SPEC011_Straight_" + label;
        StylizedWaterFlowBakerUtility.Bake(b, true);
        Capture(b, label + "_Direction", 1);
        Capture(b, label + "_Pattern", 0);
        string metrics = Audit(b, label);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(b.gameObject.scene);
        return metrics;
    }

    public static void Capture(StylizedWaterFlowBaker b, string name, float debug)
    {
        Directory.CreateDirectory(Folder);
        var cameraObject = new GameObject("SPEC011_TemporaryCapture", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.transform.position = b.transform.TransformPoint(new Vector3(b.BakeCenter.x, 22, b.BakeCenter.y));
        camera.transform.rotation = b.transform.rotation * Quaternion.Euler(90,0,0);
        camera.orthographic = true; camera.orthographicSize = b.BakeSize.y * .58f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.12f,.15f,.16f);
        int width = 1400, height = Mathf.RoundToInt(width * b.BakeSize.y / b.BakeSize.x);
        var rt = new RenderTexture(width, height, 24);
        var previous = RenderTexture.active;
        float previousDebug = b.TargetMaterial.GetFloat("_FlowDebugMode");
        try
        {
            b.TargetMaterial.SetFloat("_FlowDebugMode", debug);
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            var texture = new Texture2D(width,height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,width,height),0,0); texture.Apply();
            File.WriteAllBytes(Folder + "/" + name + ".png", texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }
        finally
        {
            b.TargetMaterial.SetFloat("_FlowDebugMode", previousDebug);
            RenderTexture.active = previous; camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    static Vector2 Direction(StylizedWaterFlowBaker b, Vector2 p)
    {
        Vector2 uv = b.UVFromLocal(new Vector3(p.x,0,p.y));
        Color c = b.BakedFlowMap.GetPixelBilinear(uv.x,uv.y);
        return new Vector2((c.r*2-1)*b.BakeSize.x,(c.g*2-1)*b.BakeSize.y).normalized;
    }

    public static string Audit(StylizedWaterFlowBaker b, string label)
    {
        Directory.CreateDirectory(Folder);
        var csv = new StringBuilder("x,z,dx,dz,blocked\n");
        int n = b.BakedFlowMap.width, reverse = 0;
        float minForward = 1, bError = 0;
        for (int y=0;y<n;y+=3) for(int x=0;x<n;x+=3)
        {
            Vector3 p = b.LocalFromUV(new Vector2((x+.5f)/n,(y+.5f)/n));
            Vector2 d = Direction(b,new Vector2(p.x,p.z));
            bool blocked = b.BakeDiagnostics.GetPixel(x,y).r > .9f;
            csv.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2},{3},{4}\n",p.x,p.z,d.x,d.y,blocked?1:0);
            if (!blocked && b.BakeDiagnostics.GetPixel(x,y).g > .1f)
            { minForward = Mathf.Min(minForward,d.x); if(d.x<=0) reverse++; }
            bError = Mathf.Max(bError,Mathf.Abs(b.BakedFlowMap.GetPixel(x,y).b-.62f));
        }
        File.WriteAllText(Folder+"/"+label+"_Field.csv",csv.ToString());
        int hits=0, escapes=0, arrived=0;
        var traces = new StringBuilder("trace,x,z\n");
        // Even seeds deliberately straddle the exact mathematical separatrix.
        for(int seed=0;seed<80;seed++)
        {
            Vector2 p = new Vector2(-7.5f,Mathf.Lerp(-1.9f,1.9f,(seed+.5f)/80));
            bool hit=false,escape=false;
            for(int step=0;step<2400 && p.x<7.5f;step++)
            {
                if(step%8==0) traces.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2}\n",seed,p.x,p.y);
                Vector2 a = Direction(b,p); p += Direction(b,p+a*.005f)*.01f;
                if(p.magnitude<.575f) {hit=true;break;}
                if(Mathf.Abs(p.y)>2) {escape=true;break;}
            }
            if(hit)hits++; else if(escape)escapes++; else if(p.x>=7.5f)arrived++;
        }
        File.WriteAllText(Folder+"/"+label+"_Traces.csv",traces.ToString());
        string report = $"{label}: forward min={minForward:F5}; reverse samples={reverse}; B error={bError}; traces: {arrived}/80 arrived, {hits} collider hits, {escapes} bank escapes.";
        File.WriteAllText(Folder+"/"+label+"_Metrics.txt",report);
        return report;
    }
}
