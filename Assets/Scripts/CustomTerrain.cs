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

    public float perlinXScale = 0.01f;
    public float perlinZScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinZOffset = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinLacunarity = 2;
    public float perlinHeightScale = 0.09f;

    public Terrain terrain;
    public TerrainData terrainData;

    public void Perlin()
    {
        float[,] heightMap;

        heightMap = new float[terrainData.heightmapResolution,
                              terrainData.heightmapResolution];

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] = Noise.fBM((x + perlinXOffset) * perlinXScale, (z + perlinZOffset) * perlinZScale,
                                             perlinOctaves, perlinPersistance, perlinLacunarity) * perlinHeightScale;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap;

        heightMap = new float[terrainData.heightmapResolution,
                              terrainData.heightmapResolution];

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] = UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
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

        heightMap = new float[terrainData.heightmapResolution,
                              terrainData.heightmapResolution];

        for (int x = 0; x < terrainData.heightmapResolution; ++x)
        {
            for (int z = 0; z < terrainData.heightmapResolution; ++z)
            {
                heightMap[x, z] = heightMapTexture.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    // Start is called before the first frame update
    void Start()
    {

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

    // Update is called once per frame
    void Update()
    {

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
