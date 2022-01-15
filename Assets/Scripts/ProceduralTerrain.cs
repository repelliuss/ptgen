using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class ProceduralTerrain : MonoBehaviour
{
    public HeightMapParams heightMapParam;
    public TreeParams[] vegetationParam;
    public FoliageParams[] quadFoliageParam;
    public FoliageParams[] foliageParam;
    public WaterParams waterParam;

    public const int createAtOnce = 25;
    public const float createInterval = 0.1f;

    [Range(0, 7)]
    public int lod;
    public Material material;
    public bool rotateQuadFoliage;

    [SerializeField]
    TexturePreset texturePreset;

    public static int worldSeed = 0;

    static float[,] previewHeightMap;
    static HeightMapNeighboursData previewNeighboursData;
    static GameObject previewObject;
    static GameObject previewWater;

    Queue<Action> heightMapLine = new Queue<Action>();
    Queue<Action> meshLine = new Queue<Action>();
    Queue<Action> treeLine = new Queue<Action>();
    Queue<Action> shorelineLine = new Queue<Action>();
    Queue<Action> foliageLine = new Queue<Action>();

    public void RequestHeightMap(Action<HeightMapMaker> callback, Vector2 center)
    {
        new Thread(() => MakeHeightMap(callback, center)).Start();
    }

    public void RequestSquareMesh(Action<SquareMesh> callback, float[,] heightMap,
                                  int lod)
    {
        new Thread(() => MakeMesh(callback, heightMap, lod)).Start();
    }

    public void RequestTrees(Action<Forest> callback, float[,] heightMap,
                             Vector2 center, Transform parent,
                             int seed)
    {
        new Thread(() => MakeTrees(callback, heightMap, center, parent, seed)).Start();
    }

    public void RequestFoliage(Action<Foliage> callback, float[,] heightMap,
                             Vector2 center, Transform parent, int seed)
    {
        new Thread(() => MakeFoliage(callback, heightMap, center, parent,
                                     seed, foliageParam)).Start();
    }

    public void RequestQuadFoliage(Action<Foliage> callback, float[,] heightMap,
                                   Vector2 center, Transform parent, int seed)
    {
        new Thread(() => MakeFoliage(callback, heightMap, center, parent,
                                     seed, quadFoliageParam)).Start();
    }

    public void RequestShoreLine(Action<Shoreline> callback, float[,] heightMap,
                                 HeightMapNeighboursData neighbourData,
                                 Vector2 center, Transform parent)
    {
        GameObject shoreLine = new GameObject();
        shoreLine.transform.name = "Shore Line";
        shoreLine.transform.parent = parent;
        new Thread(() => MakeShoreLine(callback, heightMap, neighbourData,
                                       center, parent,
                                       shoreLine)).Start();
    }

    public void MakePreviewTerrain()
    {
        HeightMapMaker map = new HeightMapMaker(heightMapParam);
        map.Make(Vector2.zero);
        previewHeightMap = map.data;
        previewNeighboursData = map.ndata;
        Mesh mesh = SquareMesh.FromHeightMap(previewHeightMap, lod).Generate();

        if (previewObject == null)
        {
            previewObject = GameObject.FindGameObjectWithTag("Preview");
            if(previewObject == null) {
                previewObject = new GameObject("Preview Procedural Terrain");
                previewObject.tag = "Preview";
            }
        }

        MeshFilter meshFilter;
        if (!previewObject.TryGetComponent<MeshFilter>(out meshFilter))
        {
            meshFilter = previewObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;
        meshFilter.transform.localScale = Vector3.one * heightMapParam.uniformScale;

        SetTexturesToMaterial();
        MeshRenderer meshRenderer;
        if (!previewObject.TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            meshRenderer = previewObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = material;
    }

    void PreviewForest()
    {
        if(previewObject == null || previewHeightMap == null) MakePreviewTerrain();

        Vector2 center = new Vector2(previewObject.transform.position.x,
                                     previewObject.transform.position.z);

        if(previewObject.transform.childCount > 0)
        DestroyImmediate(previewObject.transform.GetChild(0).gameObject);

        GameObject forest = new GameObject();
        forest.transform.parent = previewObject.transform;
        forest.transform.name = "Forest";

        Forest v = new Forest(vegetationParam, center,
                              forest.transform, 58, heightMapParam.uniformScale);
        v.Bake(previewHeightMap, heightMapParam.GetMaxHeight());
        v.Plant();
    }

    void PreviewFoliageHelper(FoliageParams[] param, string name)
    {
        if(previewObject == null || previewHeightMap == null) MakePreviewTerrain();

        Vector2 center = new Vector2(previewObject.transform.position.x,
                                     previewObject.transform.position.z);

        if(previewObject.transform.childCount > 0)
        DestroyImmediate(previewObject.transform.GetChild(0).gameObject);

        GameObject quadFoliage = new GameObject();
        quadFoliage.transform.parent = previewObject.transform;
        quadFoliage.transform.name = name;

        Foliage v = new Foliage(param, center,
                                quadFoliage.transform, 58,
                                heightMapParam.uniformScale);
        v.Bake(previewHeightMap, heightMapParam.GetMaxHeight());
        v.Plant();
    }

    void PreviewQuadFoliage()
    {
        PreviewFoliageHelper(quadFoliageParam, "Quad Foliage");
    }

    void PreviewFoliage()
    {
        PreviewFoliageHelper(foliageParam, "Foliage");
    }

    public void PreviewWater()
    {
        if(previewObject == null || previewHeightMap == null) MakePreviewTerrain();

        if(previewWater == null)
        {
            previewWater = GameObject.FindWithTag("Water");
            if(previewWater == null)
            {
                previewWater = GameObject.Instantiate(waterParam.gobject);
            }
        }

        previewWater.transform.parent = previewObject.transform;
        previewWater.transform.tag = "Water";
        Vector3 waterPos = new Vector3(previewObject.transform.position.x,
                                       waterParam.waterLevel,
                                       previewObject.transform.position.z);
        previewWater.transform.position = waterPos;
        int chunkSize = HeightMapParams.size - 3;
        Vector3 waterChunkScale = new Vector3(chunkSize / waterParam.waterTileSize.x * 2,
                                              1,
                                              chunkSize / waterParam.waterTileSize.y * 2);
        previewWater.transform.localScale = waterChunkScale;

        GameObject shoreObject = new GameObject();
        shoreObject.transform.name = "Shore Line";
        shoreObject.transform.parent = previewObject.transform;
        Shoreline shoreLine = new Shoreline(previewHeightMap, previewNeighboursData,
                                            heightMapParam.uniformScale,
                                            waterParam.waterLevel,
                                            waterParam.material,
                                            Vector2.zero, previewObject.transform,
                                            shoreObject);
        shoreLine.BakeQuads();
        shoreLine.PlantQuads();
        shoreLine.PlantShoreLine();
        shoreLine.DestroyQuadsImmediate();
    }

    void Start()
    {
        SetTexturesToMaterial();
        heightMapParam.landOffset.x = worldSeed;
        heightMapParam.landOffset.y = worldSeed;
        UnityEngine.Random.InitState(worldSeed);
        GameObject previewObject = GameObject.FindGameObjectWithTag("Preview");
        if (previewObject != null) previewObject.SetActive(false);
    }

    void Update()
    {
        while (heightMapLine.Count > 0)
        {
            Action callback = heightMapLine.Dequeue();
            callback();
        }
        while (treeLine.Count > 0)
        {
            Action callback = treeLine.Dequeue();
            callback();
        }
        while (foliageLine.Count > 0)
        {
            Action callback = foliageLine.Dequeue();
            callback();
        }
        while (meshLine.Count > 0)
        {
            Action callback = meshLine.Dequeue();
            callback();
        }
        while (shorelineLine.Count > 0)
        {
            Action callback = shorelineLine.Dequeue();
            callback();
        }
    }

    void OnValidate()
    {
        if (heightMapParam)
        {
            heightMapParam.onChange -= MakePreviewTerrain;
            heightMapParam.onChange += MakePreviewTerrain;

            foreach(NoiseParams param in heightMapParam.noises)
            {
                param.onChange -= MakePreviewTerrain;
                param.onChange += MakePreviewTerrain;
            }

            if(heightMapParam.thermalParam)
            {
                heightMapParam.thermalParam.onChange -= MakePreviewTerrain;
                heightMapParam.thermalParam.onChange += MakePreviewTerrain;
            }

            if(heightMapParam.windParam)
            {
                heightMapParam.windParam.onChange -= MakePreviewTerrain;
                heightMapParam.windParam.onChange += MakePreviewTerrain;
            }

            if(heightMapParam.falloffParam)
            {
                heightMapParam.falloffParam.onChange -= MakePreviewTerrain;
                heightMapParam.falloffParam.onChange += MakePreviewTerrain;
            }
        }

        if (texturePreset)
        {
            texturePreset.onChange -= SetTexturesToMaterial;
            texturePreset.onChange += SetTexturesToMaterial;
        }

        foreach(TreeParams param in vegetationParam)
        {
            param.onChange -= PreviewForest;
            param.onChange += PreviewForest;
        }

        foreach(FoliageParams param in quadFoliageParam)
        {
            param.onChange -= PreviewQuadFoliage;
            param.onChange += PreviewQuadFoliage;
        }

        foreach(FoliageParams param in foliageParam)
        {
            param.onChange -= PreviewFoliage;
            param.onChange += PreviewFoliage;
        }

        if(waterParam)
        {
            waterParam.onChange -= PreviewWater;
            waterParam.onChange += PreviewWater;
        }
    }

    void SetTexturesToMaterial()
    {
        texturePreset.SetHeights(material, heightMapParam.GetMinHeight(),
                                 heightMapParam.GetMaxHeight());
        texturePreset.ApplyToMaterial(material);
    }

    void MakeHeightMap(Action<HeightMapMaker> callback, Vector2 center)
    {
        HeightMapMaker map = new HeightMapMaker(heightMapParam);
        map.Make(center);

        lock (heightMapLine)
        {
            heightMapLine.Enqueue(() => callback(map));
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

    void MakeTrees(Action<Forest> callback, float[,] heightMap,
           Vector2 center, Transform parent,
           int seed)
    {
        Forest veg = new Forest(vegetationParam, center, parent, seed,
                                heightMapParam.uniformScale);
        veg.Bake(heightMap, heightMapParam.GetMaxHeight());

        lock (treeLine)
        {
            treeLine.Enqueue(() => callback(veg));
        }
    }

    void MakeShoreLine(Action<Shoreline> callback, float[,] heightMap,
                       HeightMapNeighboursData neighbourData,
                       Vector2 center, Transform parent, GameObject shoreLine)
    {
        Shoreline sh = new Shoreline(heightMap, neighbourData,
                                     heightMapParam.uniformScale,
                                     waterParam.waterLevel,
                                     waterParam.material, center, parent,
                                     shoreLine);
        sh.BakeQuads();

        lock (shorelineLine)
        {
            shorelineLine.Enqueue(() => callback(sh));
        }
    }

    void MakeFoliage(Action<Foliage> callback, float[,] heightMap,
                     Vector2 center, Transform parent, int seed,
                     FoliageParams[] param)
    {
        Foliage foliage = new Foliage(param, center, parent, seed,
                                      heightMapParam.uniformScale);

        foliage.Bake(heightMap, heightMapParam.GetMaxHeight());

        lock (foliageLine)
        {
            foliageLine.Enqueue(() => callback(foliage));
        }
    }
}
