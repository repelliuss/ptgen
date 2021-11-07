using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{

    public Vector2 randomHeightRange = new Vector2(0, 0.1f);

    public Texture2D heightMapTexture;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool flattenBeforeApply = true;

    public float perlinXScale = 0.01f;
    public float perlinZScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinZOffset = 0;
    public int perlinOctaves = 4;
    public float perlinPersistance = 0.5f;
    public float perlinLacunarity = 2;
    public float perlinHeightScale = 1;

    [System.Serializable]
    public class PerlinParams
    {
        public float perlinXScale = 0.01f;
        public float perlinZScale = 0.01f;
        public int perlinXOffset = 0;
        public int perlinZOffset = 0;
        public int perlinOctaves = 4;
        public float perlinPersistance = 0.5f;
        public float perlinLacunarity = 2;
        public float perlinHeightScale = 1;

        public bool active = true;
        public bool remove = false;
    };

    public List<PerlinParams> perlinParams = new List<PerlinParams>()
    {
        new PerlinParams(),
    };

    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0.1f;
    public float voronoiMaxHeight = 0.5f;
    public int voronoiPeaks = 5;
    public enum VoronoiType { Linear = 0, Power = 1, SinPow = 2, Combined = 3 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    //Midpoint Displacement
    public float MPDheightMin = -2f;
    public float MPDheightMax = 2f;
    public float MPDheightDampenerPower = 2.0f;
    public float MPDroughness = 2.0f;

    public int smoothAmount = 2;

    public Terrain terrain;
    public TerrainData terrainData;

    float[,] GetHeightMap()
    {
        if (flattenBeforeApply)
        {
            return new float[terrainData.heightmapResolution,
                             terrainData.heightmapResolution];
        }

        return terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                                      terrainData.heightmapResolution);
    }

    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                                      terrainData.heightmapResolution);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                 "Progress",
                                 smoothProgress);

        for (int s = 0; s < smoothAmount; s++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                                                                  terrainData.heightmapResolution,
                                                                  terrainData.heightmapResolution);
                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                             "Progress",
                                             smoothProgress / smoothAmount);

        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    public void MidPointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;
        float heightMin = MPDheightMin;
        float heightMax = MPDheightMax;
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);


        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                    Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {

                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0
                        || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    //Calculate the square value for the bottom side
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[midX, pmidYD] +
                                                  heightMap[cornerX, y]) / 4.0f +
                                                 Random.Range(heightMin, heightMax));
                    //Calculate the square value for the top side
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                        heightMap[midX, pmidYU]) / 4.0f +
                                                       Random.Range(heightMin, heightMax));

                    //Calculate the square value for the left side
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                            heightMap[pmidXL, midY] +
                                                            heightMap[x, cornerY] +
                                                  heightMap[midX, midY]) / 4.0f +
                                                 Random.Range(heightMin, heightMax));
                    //Calculate the square value for the right side
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                            heightMap[pmidXR, midY]) / 4.0f +
                                                       Random.Range(heightMin, heightMax));

                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();

        for (int p = 0; p < voronoiPeaks; p++)
        {
            Vector3 peak = new Vector3(Random.Range(0, terrainData.heightmapResolution),
                                       Random.Range(voronoiMinHeight, voronoiMaxHeight),
                                       Random.Range(0, terrainData.heightmapResolution)
                                      );

            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapResolution,
                                                                              terrainData.heightmapResolution));
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;

                        if (voronoiType == VoronoiType.Combined)
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff -
                                Mathf.Pow(distanceToPeak, voronoiDropOff); //combined
                        }
                        else if (voronoiType == VoronoiType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff; //power
                        }
                        else if (voronoiType == VoronoiType.SinPow)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) -
                                    Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff; //sinpow
                        }
                        else
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff; //linear
                        }

                        if (heightMap[x, y] < h)
                            heightMap[x, y] = h;
                    }

                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                foreach (PerlinParams p in perlinParams)
                {
                    if (p.active)
                    {
                        heightMap[x, z] += Noise.fBM((x + p.perlinXOffset) * p.perlinXScale, (z + p.perlinZOffset) * p.perlinZScale,
                                                     p.perlinOctaves, p.perlinPersistance, p.perlinLacunarity) * p.perlinHeightScale;
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddPerlin()
    {
        perlinParams.Add(new PerlinParams());
    }

    public void RemovePerlin()
    {
        var savedParams = new List<PerlinParams>();
        foreach (PerlinParams p in perlinParams)
        {
            if (!p.remove)
            {
                savedParams.Add(p);
            }
        }

        if (savedParams.Count == 0) savedParams.Add(new PerlinParams());

        perlinParams = savedParams;
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] += Noise.fBM((x + perlinXOffset) * perlinXScale, (z + perlinZOffset) * perlinZScale,
                                            perlinOctaves, perlinPersistance, perlinLacunarity) * perlinHeightScale;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap;

        heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] += Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    public void FlattenTerrain()
    {
        float[,] heightMap;

        heightMap = new float[terrainData.heightmapResolution,
                              terrainData.heightmapResolution];

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] = 0;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap;

        heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] += heightMapTexture.GetPixel((int)(x * heightMapScale.x),
                                                            (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    void OnEnable()
    {
        Debug.Log("Initialising terrain data");
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    void Reset()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        tagManager.ApplyModifiedProperties();

        tag = "Terrain";
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        for (int i = 0; i < tagsProp.arraySize; ++i)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
                return;
        }

        tagsProp.InsertArrayElementAtIndex(0);
        SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
        newTagProp.stringValue = newTag;
    }
}
