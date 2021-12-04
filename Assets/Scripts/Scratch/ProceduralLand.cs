using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

//TODO: change terrain height to length in z axis
public class ProceduralLand : MonoBehaviour
{
    public LandPreset landPreset;
    public NoisePreset noisePreset;
    public TexturePreset texturePreset;

    public Material material;

    public bool autoUpdate = false;

    public const int chunkSize = 127;
    const int previewLOD = 0;

    Queue<ThreadInfo<LandData>> readyLandData = new Queue<ThreadInfo<LandData>>();
    Queue<ThreadInfo<MeshData>> readyMeshData = new Queue<ThreadInfo<MeshData>>();

    float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(chunkSize + 2);

    void Awake()
    {
        texturePreset.ApplyToMaterial(material);
        texturePreset.UpdateMeshHeights(material, landPreset.GetMinHeight(), landPreset.GetMaxHeight());
    }

    public void DrawLand()
    {
        texturePreset.UpdateMeshHeights(material, landPreset.GetMinHeight(), landPreset.GetMaxHeight());

        LandData landData = GenerateLandData(Vector2.zero);
        var terrainRenderer = FindObjectOfType<TerrainRenderer>();

        terrainRenderer.DrawFromHeightMap(landData, MeshGenerator.GenerateFromHeightMap(landData.heightMap, landPreset.heightCurve, previewLOD, landPreset.height));
    }

    void OnValueChange()
    {
        if (!Application.isPlaying)
        {
            DrawLand();
        }
    }

    void OnTextureValueChange()
    {
        texturePreset.ApplyToMaterial(material);
    }

    void OnValidate()
    {
        if (landPreset != null)
        {
            landPreset.OnValueChange -= OnValueChange;
            landPreset.OnValueChange += OnValueChange;
        }

        if (noisePreset != null)
        {
            noisePreset.OnValueChange -= OnValueChange;
            noisePreset.OnValueChange += OnValueChange;
        }

        if (texturePreset != null)
        {
            texturePreset.OnValueChange -= OnTextureValueChange;
            texturePreset.OnValueChange += OnTextureValueChange;
        }
    }

    LandData GenerateLandData(Vector2 center)
    {
        //REVIEW: may require chunkSize + (highest lod increment) instead of chunkSize+2
        float[,] heightMap = Noise.GeneratePerlinNoiseMap(chunkSize + 2, chunkSize + 2, noisePreset.noiseScale,
                                                         noisePreset.octaves, noisePreset.persistance, noisePreset.lacunarity,
                                                         noisePreset.landSeed, center + noisePreset.landOffset, noisePreset.normalizeMode);

        if (landPreset.useFalloff)
        {
            //REVIEW: put this to noise gen
            //BUG: bugs out updatemeshheights
            for (int y = 0; y < falloffMap.GetLength(0); ++y)
            {
                for (int x = 0; x < falloffMap.GetLength(1); ++x)
                {
                    heightMap[x, y] = Mathf.Clamp01(heightMap[x, y] - falloffMap[x, y]);
                }
            }
        }

        return new LandData(heightMap);
    }

    public void RequestLandData(Action<LandData> callback, Vector2 center)
    {
        new Thread(() => MakeLandData(callback, center)).Start();
    }

    void MakeLandData(Action<LandData> callback, Vector2 center)
    {
        LandData landData = GenerateLandData(center);
        lock (readyLandData)
        {
            readyLandData.Enqueue(new ThreadInfo<LandData>(callback, landData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, LandData landData, int lod)
    {
        new Thread(() => MakeMeshData(callback, landData, lod)).Start();
    }

    void MakeMeshData(Action<MeshData> callback, LandData landData, int lod)
    {
        MeshData meshData = MeshGenerator.GenerateFromHeightMap(landData.heightMap, landPreset.heightCurve,
                                                                lod, landPreset.height);
        lock (readyMeshData)
        {
            readyMeshData.Enqueue(new ThreadInfo<MeshData>(callback, meshData));
        }
    }

    readonly struct ThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T data;

        public ThreadInfo(Action<T> callback, T data)
        {
            this.callback = callback;
            this.data = data;
        }
    }

    void Update()
    {
        while (readyLandData.Count > 0)
        {
            ThreadInfo<LandData> info = readyLandData.Dequeue();
            info.callback(info.data);
        }

        while (readyMeshData.Count > 0)
        {
            ThreadInfo<MeshData> info = readyMeshData.Dequeue();
            info.callback(info.data);
        }
    }
}

public readonly struct LandData
{
    public readonly float[,] heightMap;

    public LandData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
