using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Fixed SPEC-009 diagnostic fixture only. No scene discovery, colliders or baker.
public static class Spec009ChannelFixture
{
    const string Root = "Assets/WaterShader/";

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
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Root+"Scenes/WaterShader_TestScene.unity")
            throw new InvalidOperationException("Open the WaterShader test scene first.");
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Meshes/MESH_FlowCurve_Water.asset");
        var vertices = mesh.vertices;
        if(vertices.Length != 202) throw new InvalidOperationException("Expected the 101-station technical strip.");
        var chart = new Vector2[vertices.Length];
        var frames = new System.Collections.Generic.List<Vector4>();
        float distance = 0;
        Vector3 previous = Vector3.zero;
        for(int i=0; i<vertices.Length; i+=2)
        {
            var center = (vertices[i]+vertices[i+1])*.5f;
            if(i>0) distance += Vector3.Distance(center,previous);
            // Match the physical pattern density of the 7.2 x 2.2 straight control.
            chart[i] = new Vector2(distance/7.2f,0);
            chart[i+1] = new Vector2(distance/7.2f,Vector3.Distance(vertices[i],vertices[i+1])/2.2f);
            Vector2 tangent=Tangent((float)i/(vertices.Length-2));
            Vector2 normal=new Vector2(-tangent.y,tangent.x);
            var frame=new Vector4(tangent.x*7.2f/22,tangent.y*7.2f/11,normal.x*2.2f/22,normal.y*2.2f/11);
            frames.Add(frame); frames.Add(frame);
            previous=center;
        }
        mesh.uv2=chart;
        mesh.SetUVs(2,frames);
        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssetIfDirty(mesh);
        WriteMap(false);
        WriteMap(true);
        var reference = AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/MAT_Water_Uniform_Comparison.mat");
        foreach(string suffix in new[]{"Constant","Strength"})
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/MAT_Water_FlowCurve_"+suffix+".mat");
            mat.CopyPropertiesFromMaterial(reference);
            mat.SetFloat("_UseFlowMap",1);
            mat.SetFloat("_UseFlowCoordinates",1);
            mat.SetFloat("_FlowMapMinStrength",.46f);
            mat.SetFloat("_FlowMapMaxStrength",.88f);
            mat.SetFloat("_FlowSpeed",.25f);
            mat.SetFloat("_FlowDebugMode",0);
            mat.SetTexture("_FlowMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/Flow/T_FlowMap_Curve"+suffix+".png"));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssetIfDirty(mat);
        }
        reference.SetFloat("_FlowSpeed",.25f);
        // Uniform's historical +UV offset moves opposite its vector. Preserve
        // that shader behavior; reverse this comparison material to travel right.
        reference.SetVector("_FlowDirection",new Vector4(-1,0,0,0));
        EditorUtility.SetDirty(reference);
        AssetDatabase.SaveAssetIfDirty(reference);
        GameObject.Find("CurvedWater_Channel").GetComponent<Renderer>().sharedMaterial=
            AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/MAT_Water_FlowCurve_Constant.mat");
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("SPEC-009 chart rebuilt: length "+distance+"; UV0 planar (22 x 11), UV2 longitudinal; RG remains planar direction.");
    }

    static void WriteMap(bool variableStrength)
    {
        const int size=256, segments=320;
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
            Vector2 tangent=Tangent(t);
            Vector2 uvDirection=new Vector2(tangent.x/22,tangent.y/11).normalized;
            float strength=variableStrength ? Mathf.Lerp(.82f,.28f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,t))) : .62f;
            pixels[y*size+x]=new Color(uvDirection.x*.5f+.5f,uvDirection.y*.5f+.5f,strength,1);
        }
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false,true);
        texture.SetPixels(pixels); texture.Apply();
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
