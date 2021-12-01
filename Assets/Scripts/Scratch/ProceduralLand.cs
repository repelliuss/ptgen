using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

//TODO: change terrain height to length in z axis
public class ProceduralLand : MonoBehaviour
{
    public NormalizeMode normalizeMode;

    [Range(0, 7)]
    public int previewLOD = 0;

    public const int chunkSize = 129;
    [Min(1)]
    public int height = 64;

    [Min(0.001f)]
    public float noiseScale = 0.3f;

    [Min(1)]
    public int octaves;

    [Range(0, 1)]
    public float persistance;
    [Min(1)]
    public float lacunarity;

    public AnimationCurve heightCurve;

    public int landSeed;
    public Vector2 landOffset;

    public Gradient gradient;

    public bool autoUpdate = false;

    Queue<ThreadInfo<LandData>> readyLandData = new Queue<ThreadInfo<LandData>>();
    Queue<ThreadInfo<MeshData>> readyMeshData = new Queue<ThreadInfo<MeshData>>();

    public void DrawLand()
    {
        LandData landData = GenerateLandData(Vector2.zero);
        var terrainRenderer = FindObjectOfType<TerrainRenderer>();

        terrainRenderer.DrawFromHeightMap(landData, MeshGenerator.GenerateFromHeightMap(landData.heightMap, heightCurve, previewLOD, height));
    }

    public Color[] MakeColorMapFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                colorMap[y * width + x] = gradient.Evaluate(heightMap[x, y]);
            }
        }

        return colorMap;
    }

    LandData GenerateLandData(Vector2 center)
    {
        float[,] heightMap = Noise.GeneratePerlinNoiseMap(chunkSize, chunkSize, noiseScale,
                                                         octaves, persistance, lacunarity,
                                                         landSeed, center + landOffset, normalizeMode);

        return new LandData(heightMap, MakeColorMapFromHeightMap(heightMap));
    }

    public void RequestLandData(Action<LandData> callback, Vector2 center)
    {
        new Thread(() => MakeLandData(callback, center)).Start();
    }

    void MakeLandData(Action<LandData> callback, Vector2 center)
    {
        LandData landData = GenerateLandData(center);
        lock (readyLandData) {
            readyLandData.Enqueue(new ThreadInfo<LandData>(callback, landData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, LandData landData, int lod)
    {
        new Thread(() => MakeMeshData(callback, landData, lod)).Start();
    }

    void MakeMeshData(Action<MeshData> callback, LandData landData, int lod)
    {
        MeshData meshData = MeshGenerator.GenerateFromHeightMap(landData.heightMap, heightCurve,
                                                                lod, height);
        lock (readyMeshData) {
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
    public readonly Color[] colorMap;

    public LandData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
