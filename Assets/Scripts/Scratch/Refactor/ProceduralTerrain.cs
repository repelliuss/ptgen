using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class ProceduralTerrain : MonoBehaviour
{
    public GradientHeightMapParams param;

    [Range(0,7)]
    public int lod;
    public Material material;

    [SerializeField]
    TexturePreset texturePreset;

    static GameObject previewObject;

    Queue<Action> heightMapLine = new Queue<Action>();
    Queue<Action> meshLine = new Queue<Action>();

    public void RequestHeightMap(Action<float[,]> callback, Vector2 center)
    {
        new Thread(() => MakeHeightMap(callback, center)).Start();
    }

    public void RequestSquareMesh(Action<SquareMesh> callback, float[,] heightMap,
                                  int lod)
    {
        new Thread(() => MakeMesh(callback, heightMap, lod)).Start();
    }

    public void MakePreviewTerrain()
    {
        float[,] heightMap = (new GradientHeightMapMaker(param)).Make(Vector2.zero);
        Mesh mesh = SquareMesh.FromHeightMap(heightMap, lod).Generate();

        Debug.Log("called!");

        if(previewObject == null)
        {
            previewObject = new GameObject("Preview Procedural Terrain");
            previewObject.tag = "Preview";
        }

        MeshFilter meshFilter;
        if(!previewObject.TryGetComponent<MeshFilter>(out meshFilter)) {
            meshFilter = previewObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;
        meshFilter.transform.localScale = Vector3.one * param.uniformScale;

        SetTexturesToMaterial();
        MeshRenderer meshRenderer;
        if(!previewObject.TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            meshRenderer = previewObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = material;
    }


    void Awake()
    {
        SetTexturesToMaterial();
    }

    void Start()
    {
        GameObject.FindGameObjectWithTag("Preview").SetActive(false);
    }

    void Update()
    {
        while(heightMapLine.Count > 0)
        {
            Action callback = heightMapLine.Dequeue();
            callback();
        }
        while(meshLine.Count > 0)
        {
            Action callback = meshLine.Dequeue();
            callback();
        }
    }

    void OnValidate()
    {
        if(param)
        {
            param.onChange -= MakePreviewTerrain;
            param.onChange += MakePreviewTerrain;
        }

        if(texturePreset)
        {
            texturePreset.onChange -= SetTexturesToMaterial;
            texturePreset.onChange += SetTexturesToMaterial;
        }
    }

    void SetTexturesToMaterial()
    {
        texturePreset.SetHeights(material, param.GetMinHeight(),
                                 param.GetMaxHeight());
        texturePreset.ApplyToMaterial(material);
    }

    void MakeHeightMap(Action<float[,]> callback, Vector2 center)
    {
        float[,] heightMap = (new GradientHeightMapMaker(param)).Make(center);

        lock(heightMapLine)
        {
            heightMapLine.Enqueue(() => callback(heightMap));
        }
    }

    void MakeMesh(Action<SquareMesh> callback, float[,] heightMap, int lod)
    {
        SquareMesh mesh = SquareMesh.FromHeightMap(heightMap, lod);

        lock (meshLine)
        {
            meshLine.Enqueue(() => callback(mesh));
        }
    }
}
