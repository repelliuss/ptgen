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

    public void MultiplePerlin()
    {
        float[,] heightMap = GetHeightMap();;

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
            if(!p.remove)
            {
                savedParams.Add(p);
            }
        }

        if (savedParams.Count == 0) savedParams.Add(new PerlinParams());

        perlinParams = savedParams;
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();;

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
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
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
