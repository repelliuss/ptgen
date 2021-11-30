using UnityEngine;
using System;
using System.Threading;
using System.Collections.Concurrent;

//TODO: change terrain height to length in z axis
public class ProceduralLand : MonoBehaviour
{
    [Range(0, 7)]
    public int levelOfDetail = 0;

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

    ConcurrentQueue<ThreadInfo<LandData>> readyLandData = new ConcurrentQueue<ThreadInfo<LandData>>();

    public void DrawLand()
    {
        LandData landData = GenerateLandData();
        var terrainRenderer = FindObjectOfType<TerrainRenderer>();

        terrainRenderer.DrawFromHeightMap(landData, MeshGenerator.GenerateFromHeightMap(landData.heightMap, heightCurve, levelOfDetail, height));
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

    LandData GenerateLandData()
    {
        float[,] heightMap = Noise.GeneratePerlinNoiseMap(chunkSize, chunkSize, noiseScale,
                                                         octaves, persistance, lacunarity,
                                                         landSeed, landOffset);

        return new LandData(heightMap, MakeColorMapFromHeightMap(heightMap));
    }

    public void RequestLandData(Action<LandData> callback)
    {
        new Thread(() => MakeLandData(callback)).Start();
    }

    void MakeLandData(Action<LandData> callback)
    {
        readyLandData.Enqueue(new ThreadInfo<LandData>(callback, GenerateLandData()));
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
        ThreadInfo<LandData> info;
        while (readyLandData.TryDequeue(out info))
        {
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
