using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Fixed SPEC-009 diagnostic fixture only. No scene discovery, colliders or baker.
public static class Spec009ChannelFixture
{
    const string Root = "Assets/WaterShader/";

    [MenuItem("Tools/WaterShader/SPEC-009/Upgrade Diagnostic Precision")]
    public static void UpgradePrecision()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Root+"Scenes/WaterShader_TestScene.unity")
            throw new InvalidOperationException("Open the WaterShader test scene first.");
        const int stations=401, lanes=17;
        var vertices=new Vector3[stations*lanes];
        var normals=new Vector3[vertices.Length];
        var uv=new Vector2[vertices.Length];
        var chart=new Vector2[vertices.Length];
        var frames=new System.Collections.Generic.List<Vector4>();
        var indices=new int[(stations-1)*(lanes-1)*6];
        float distance=0;
        for(int i=0;i<stations;i++)
        {
            float t=(float)i/(stations-1);
            Vector2 center=Center(t), tangent=Tangent(t), normal=new Vector2(-tangent.y,tangent.x);
            if(i>0) distance+=Vector2.Distance(center,Center((float)(i-1)/(stations-1)));
            for(int j=0;j<lanes;j++)
            {
                int k=i*lanes+j;
                float across=3.1f*j/(lanes-1);
                Vector2 p=center+normal*(across-1.55f);
                vertices[k]=new Vector3(p.x,0,p.y); normals[k]=Vector3.up;
                uv[k]=new Vector2(p.x/22+.5f,p.y/11+.5f);
                chart[k]=new Vector2(distance/7.2f,across/2.2f);
                frames.Add(new Vector4(tangent.x*7.2f/22,tangent.y*7.2f/11,normal.x*2.2f/22,normal.y*2.2f/11));
                if(i<stations-1 && j<lanes-1)
                {
                    int n=(i*(lanes-1)+j)*6;
                    indices[n]=k; indices[n+1]=k+1; indices[n+2]=k+lanes;
                    indices[n+3]=k+1; indices[n+4]=k+lanes+1; indices[n+5]=k+lanes;
                }
            }
        }
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Meshes/MESH_FlowCurve_Water.asset");
        mesh.Clear(); mesh.vertices=vertices; mesh.normals=normals; mesh.uv=uv; mesh.uv2=chart;
        mesh.SetUVs(2,frames); mesh.triangles=indices; mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh); AssetDatabase.SaveAssetIfDirty(mesh);
        WriteMap(false,true); WriteMap(true,true);
        foreach(string suffix in new[]{"Constant","Strength"})
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/MAT_Water_FlowCurve_"+suffix+".mat");
            mat.SetTexture("_FlowMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/Flow/T_FlowMap_Curve"+suffix+"_Precise.asset"));
            EditorUtility.SetDirty(mat); AssetDatabase.SaveAssetIfDirty(mat);
        }
        Debug.Log("SPEC-009 precision fixture: 401 x 17 vertices, linear RGBAFloat maps; artist parameters preserved.");
    }

    static Vector2 Center(float t)
    {
        float q = Mathf.Clamp01((t - .25f) / .5f);
        return new Vector2(Mathf.Lerp(-10, 10, t), Mathf.Lerp(-3, 3, q*q*(3-2*q)));
    }

    static Vector2 Tangent(float t)
    {
        float q = Mathf.Clamp01((t - .25f) / .5f);
        return new Vector2(20, 72*q*(1-q)).normalized;
    }

    [MenuItem("Tools/WaterShader/SPEC-009/Rebuild Diagnostic Coordinates")]
    public static void Rebuild()
    {
        UpgradePrecision();
    }
    static void WriteMap(bool variableStrength, bool precise=false)
    {
        int size=precise ? 512 : 256;
        int segments=precise ? 64 : 320;
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++) for(int x=0;x<size;x++)
        {
            // Texel centers agree with bilinear GPU UV addressing.
            Vector2 p=new Vector2(((x+.5f)/size-.5f)*22,((y+.5f)/size-.5f)*11);
            float best=float.MaxValue, t=0;
            for(int k=0;k<segments;k++)
            {
                Vector2 a=Center((float)k/segments), ab=Center((float)(k+1)/segments)-a;
                float f=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);
                float error=(p-a-f*ab).sqrMagnitude;
                if(error<best){ best=error; t=(k+f)/segments; }
            }
            // A polyline projection creates tiny plateaus at segment joins.
            // Refine on the actual smooth curve before encoding its tangent;
            // long advection makes even small direction errors visible.
            if(precise) for(int iteration=0;iteration<8;iteration++)
            {
                float q=Mathf.Clamp01((t-.25f)/.5f);
                Vector2 derivative=new Vector2(20,72*q*(1-q));
                Vector2 second=new Vector2(0,t>.25f && t<.75f ? 144*(1-2*q) : 0);
                Vector2 delta=Center(t)-p;
                float denominator=derivative.sqrMagnitude+Vector2.Dot(delta,second);
                if(denominator>1) t=Mathf.Clamp01(t-Vector2.Dot(delta,derivative)/denominator);
            }
            Vector2 tangent=Tangent(t);
            Vector2 uvDirection=new Vector2(tangent.x/22,tangent.y/11).normalized;
            float strength=variableStrength ? Mathf.Lerp(.82f,.28f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,t))) : .62f;
            pixels[y*size+x]=new Color(uvDirection.x*.5f+.5f,uvDirection.y*.5f+.5f,strength,1);
        }
        var texture=new Texture2D(size,size,precise ? TextureFormat.RGBAFloat : TextureFormat.RGBA32,false,true);
        texture.SetPixels(pixels); texture.Apply();
        if(precise)
        {
            string assetPath=Root+"Textures/Flow/T_FlowMap_Curve"+(variableStrength?"Strength":"Constant")+"_Precise.asset";
            texture.name=Path.GetFileNameWithoutExtension(assetPath);
            texture.filterMode=FilterMode.Bilinear; texture.wrapMode=TextureWrapMode.Clamp;
            var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if(existing==null) AssetDatabase.CreateAsset(texture,assetPath);
            else { EditorUtility.CopySerialized(texture,existing); UnityEngine.Object.DestroyImmediate(texture); EditorUtility.SetDirty(existing); AssetDatabase.SaveAssetIfDirty(existing); }
            return;
        }
        string path=Root+"Textures/Flow/T_FlowMap_Curve"+(variableStrength?"Strength":"Constant")+".png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.sRGBTexture=false; importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.filterMode=FilterMode.Bilinear; importer.wrapMode=TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }
}
