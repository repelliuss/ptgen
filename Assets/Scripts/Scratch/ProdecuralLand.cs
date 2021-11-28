using UnityEngine;

//TODO: change terrain height to length in z axis
public class ProdecuralLand : MonoBehaviour
{
    [Range(0,7)]
    public int levelOfDetail = 0;

    const int chunkSize = 129;
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

    public bool autoUpdate = false;

    public void Generate()
    {
        float[,] heightMap = Noise.GeneratePerlinNoiseMap(chunkSize, chunkSize, noiseScale,
                                                         octaves, persistance, lacunarity,
                                                         landSeed, landOffset);

        var terrainRenderer = FindObjectOfType<TerrainRenderer>();

        terrainRenderer.DrawFromHeightMap(heightMap, MeshGenerator.GenerateFromHeightMap(heightMap, heightCurve, levelOfDetail, height));
    }
}
