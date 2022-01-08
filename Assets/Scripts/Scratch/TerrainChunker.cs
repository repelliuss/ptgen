using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(ProceduralTerrain))]
class TerrainChunker : MonoBehaviour
{
    public Transform player;
    public Transform freePlayer;
    public int colliderFromLOD;
    public LOD[] lods;

    const float colliderDistanceTreshold = 5;

    static Vector2 playerPos;
    static ProceduralTerrain terrain;
    static int maxViewDistance;
    static List<Chunk> activeChunks;

    Material material;

    Bounds updateTreshold;
    float updateBoundsSize;

    Dictionary<Vector2, Chunk> chunks;
    int chunkSize;
    int chunkPerLine;

    public float forestChunkDistance;
    public float waterChunkDistance;
    public float foliageChunkDistance;
    public float quadFoliageChunkDistance;

    static float forestChunkDistSqr;
    static float waterChunkDistSqr;
    static float foliageChunkDistSqr;
    static float quadFoliageChunkDistSqr;

    static Vector3 waterChunkScale;

    static Camera mainCam;

    void Start()
    {
        terrain = FindObjectOfType<ProceduralTerrain>();
        activeChunks = new List<Chunk>();
        chunks = new Dictionary<Vector2, Chunk>();

        maxViewDistance = lods[lods.Length - 1].viewDistance;
        chunkSize = GradientHeightMapParams.size - 3;
        chunkPerLine = Mathf.RoundToInt(maxViewDistance / chunkSize);

        updateBoundsSize = chunkSize - (chunkSize / 6f);
        updateTreshold = CalculateUpdateBounds();

        material = terrain.material;

        forestChunkDistSqr = forestChunkDistance *
            forestChunkDistance;
        waterChunkDistSqr = waterChunkDistance *
            waterChunkDistance;
        waterChunkScale = new Vector3(chunkSize / terrain.waterParam.waterTileSize.x * 2,
                                      1,
                                      chunkSize / terrain.waterParam.waterTileSize.y * 2);
        foliageChunkDistSqr = foliageChunkDistance *
            foliageChunkDistance;
        quadFoliageChunkDistSqr = quadFoliageChunkDistance *
            quadFoliageChunkDistance;

        mainCam = Camera.main;

        UpdateChunks();
    }

    void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z) /
            terrain.heightMapParam.uniformScale;
        if (!updateTreshold.Contains(playerPos))
        {
            UpdateChunks();

        }

        foreach (Chunk chunk in activeChunks)
        {
            chunk.TryBakeData();
        }
    }

    void LateUpdate()
    {
        if(terrain.rotateQuadFoliage)
        {
            foreach (Chunk chunk in activeChunks)
            {
                chunk.RotateQuads();
            }
        }
    }

    Bounds CalculateUpdateBounds()
    {
        return new Bounds(new Vector2(player.position.x, player.position.z),
                          Vector2.one * updateBoundsSize);
    }

    void ClearActiveChunks()
    {
        foreach (Chunk chunk in activeChunks)
        {
            chunk.SetActive(false);
        }
        activeChunks.Clear();
    }

    void UpdateChunks()
    {
        ClearActiveChunks();

        int playerChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int playerChunkY = Mathf.RoundToInt(playerPos.y / chunkSize);

        for (int yOffset = -chunkPerLine; yOffset <= chunkPerLine; ++yOffset)
        {
            for (int xOffset = -chunkPerLine; xOffset <= chunkPerLine; ++xOffset)
            {
                Vector2 curChunkCoord =
                    new Vector2((playerChunkX + xOffset) * chunkSize,
                                (playerChunkY + yOffset) * chunkSize);
                Chunk curChunk;

                if (chunks.TryGetValue(curChunkCoord, out curChunk))
                {
                    curChunk.UpdatePresence();
                }
                else
                {
                    chunks.Add(curChunkCoord, new Chunk(curChunkCoord, chunkSize,
                                                        material,
                                                        this, lods, colliderFromLOD));
                }
            }
        }
    }

    class Instantiator : MonoBehaviour
    {
        public void OnForestReceived(Forest forest)
        {
            StartCoroutine(forest.PlantDeferred());
        }

        public void OnQuadFoliageReceived(Foliage quadFoliage)
        {
            StartCoroutine(quadFoliage.PlantDeferred());
        }

        public void OnFoliageReceived(Foliage foliage)
        {
            StartCoroutine(foliage.PlantDeferred());
        }
    }

    class Chunk
    {
        GameObject chunk;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LOD[] lods;
        int prevLODIndex = -1;
        LODMesh[] lodMeshes;
        LODMesh colliderMesh;
        float[,] heightMap;
        Bounds chunkBounds;

        GameObject forest;
        GameObject water;
        GameObject foliage;
        GameObject quadFoliage;
        Shoreline shoreLine;

        bool shoreLineRequested;

        static GameObject instantiatorObject;
        static Instantiator instantiator;

        public Chunk(Vector2 position, int chunkSize,
                     Material material,
                     TerrainChunker parent, LOD[] lods,
                     int colliderFromLOD)
        {
            chunk = new GameObject("Chunk " + position);
            chunk.transform.position = new Vector3(position.x, 0, position.y)
                * terrain.heightMapParam.uniformScale;
            chunk.transform.localScale = Vector3.one * terrain.heightMapParam.uniformScale;
            chunk.transform.parent = parent.transform;

            chunkBounds = new Bounds(position, Vector2.one * chunkSize);

            chunk.AddComponent<MeshRenderer>().material = material;
            meshFilter = chunk.AddComponent<MeshFilter>();
            meshCollider = chunk.AddComponent<MeshCollider>();

            this.lods = lods;
            lodMeshes = new LODMesh[lods.Length];
            for (int i = 0; i < lods.Length; ++i)
            {
                if (i == colliderFromLOD)
                {
                    lodMeshes[i] = new LODMesh(lods[i],
                                               () =>
                                               {
                                                   TryBakeCollider();
                                                   UpdatePresence();
                                               });
                    colliderMesh = lodMeshes[i];
                }
                else
                {
                    lodMeshes[i] = new LODMesh(lods[i], UpdatePresence);
                }
            }

            SetActive(false);

            if(instantiatorObject == null)
            {
                instantiatorObject = new GameObject();
                instantiatorObject.transform.name = "Instantiator";
                instantiator = instantiatorObject.AddComponent<Instantiator>();
            }

            terrain.RequestHeightMap(OnHeightMapReceived, position);
        }

        public bool IsActive()
        {
            return chunk.activeSelf;
        }

        public void Destroy()
        {
            GameObject.Destroy(chunk);
        }

        public Vector2 GetCoord()
        {
            return new Vector2(chunk.transform.position.x, chunk.transform.position.z);
        }

        public void UpdatePresence()
        {
            if (heightMap != null)
            {
                float distToPlayer = Mathf.Sqrt(chunkBounds.SqrDistance(playerPos));
                bool isVisible = distToPlayer <= maxViewDistance;

                if (isVisible)
                {
                    int lodIndex = 0;
                    int i = 0;

                    while (i < lods.Length - 1 &&
                          distToPlayer > lods[i].viewDistance)
                    {
                        ++i;
                        ++lodIndex;
                    }

                    if (lodIndex != prevLODIndex)
                    {
                        LODMesh mesh = lodMeshes[lodIndex];
                        if (!mesh.IsEmpty())
                        {
                            prevLODIndex = lodIndex;
                            meshFilter.mesh = mesh.GetMesh();
                        }
                        else
                        {
                            mesh.RequestMesh(heightMap);
                        }
                    }

                    colliderMesh.RequestMesh(heightMap);

                    activeChunks.Add(this);
                }

                SetActive(isVisible);
            }
        }

        public void TryBakeData()
        {
            TryBakeCollider();
            TryBakeTrees();
            TryAddFoliage();
            TryAddQuadFoliage();
            TryAddWater();
            TryAddShoreLine();
        }

        public void TryAddWater()
        {
            Assert.IsNotNull(terrain.waterParam.gobject, "No terrain water assigned");

            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < waterChunkDistSqr)
            {
                if (water == null)
                {
                    water = GameObject.Instantiate(terrain.waterParam.gobject);
                    water.transform.parent = chunk.transform;
                    Vector3 waterPos = new Vector3(chunk.transform.position.x,
                                                   terrain.waterParam.waterLevel,
                                                   chunk.transform.position.z);
                    water.transform.position = waterPos;
                    water.transform.localScale = waterChunkScale;
                }
                else
                {
                    water.SetActive(true);
                }
            }
            else if (water != null)
            {
                GameObject.Destroy(water);
            }
        }

        public void TryAddShoreLine()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < waterChunkDistSqr)
            {
                if (!shoreLineRequested)
                {
                    shoreLineRequested = true;
                    terrain.RequestShoreLine(OnShoreLineReceived,
                                             heightMap,
                                             new Vector2(chunk.transform.position.x,
                                                         chunk.transform.position.z),
                                             chunk.transform);
                }
                else if (shoreLine != null)
                {
                    shoreLine.SetActive(true);
                }
            }
            else if (shoreLine != null)
            {
                shoreLineRequested = false;
                shoreLine.Destroy();
            }
        }

        public void TryAddQuadFoliage()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < quadFoliageChunkDistSqr)
            {
                if (quadFoliage == null)
                {
                    Vector2 position = new Vector2(chunk.transform.position.x,
                                                   chunk.transform.position.z);
                    quadFoliage = new GameObject();
                    quadFoliage.transform.parent = chunk.transform;
                    quadFoliage.transform.name = "Quad Foliage";

                    terrain.RequestQuadFoliage(instantiator.OnQuadFoliageReceived,
                                               heightMap,
                                               position,
                                               quadFoliage.transform,
                                               UnityEngine.Random.Range(0, 9999999));
                }
                else
                {
                    quadFoliage.SetActive(true);
                }
            }
            else
            {
                if (quadFoliage != null) quadFoliage.SetActive(false);
            }
        }

        public void TryAddFoliage()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < foliageChunkDistSqr)
            {
                if (foliage == null)
                {
                    Vector2 position = new Vector2(chunk.transform.position.x,
                                                   chunk.transform.position.z);
                    foliage = new GameObject();
                    foliage.transform.parent = chunk.transform;
                    foliage.transform.name = "Foliage";

                    terrain.RequestFoliage(instantiator.OnFoliageReceived,
                                           heightMap,
                                           position,
                                           foliage.transform,
                                           UnityEngine.Random.Range(0, 9999999));
                }
                else
                {
                    foliage.SetActive(true);
                }
            }
            else
            {
                if (foliage != null) foliage.SetActive(false);
            }
        }

        public void TryBakeTrees()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < forestChunkDistSqr)
            {
                if (forest == null)
                {
                    Vector2 position = new Vector2(chunk.transform.position.x,
                                                   chunk.transform.position.z);

                    forest = new GameObject();
                    forest.transform.parent = chunk.transform;
                    forest.transform.name = "Forest";

                    terrain.RequestTrees(instantiator.OnForestReceived,
                                         heightMap,
                                         position,
                                         forest.transform,
                                         UnityEngine.Random.Range(0, 9999999));
                }
                else {
                    forest.SetActive(true);
                }
            }
            else {
                if(forest != null) forest.SetActive(false);
            }
        }

        public void TryBakeCollider()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if (distToPlayer < colliderDistanceTreshold * colliderDistanceTreshold)
            {
                if (!colliderMesh.IsEmpty())
                {
                    meshCollider.sharedMesh = colliderMesh.GetMesh();
                }
            }
            else
            {
                meshCollider.sharedMesh = null;
            }
        }

        public void SetActive(bool activity)
        {
            chunk.SetActive(activity);
        }

        public void RotateQuads()
        {
            if(quadFoliage != null && quadFoliage.activeSelf)
            {
                foreach(Transform t in quadFoliage.transform)
                {
                    t.rotation = Quaternion.Euler(0f,
                                                  mainCam.transform.rotation.eulerAngles.y,
                                                  0f);
                }
            }
        }

        void OnHeightMapReceived(float[,] heightMap)
        {
            this.heightMap = heightMap;
            UpdatePresence();
        }

        void OnForestReceived(Forest forest)
        {
            forest.Plant();
        }

        void OnQuadFoliageReceived(Foliage quadFoliage)
        {
            quadFoliage.Plant();
        }

        void OnFoliageReceived(Foliage foliage)
        {
            foliage.Plant();
        }

        void OnShoreLineReceived(Shoreline shoreLine)
        {
            this.shoreLine = shoreLine;
            shoreLine.PlantQuads();
            shoreLine.PlantShoreLine();
            shoreLine.DestroyQuads();
        }
    }

    class LODMesh
    {
        Mesh mesh;
        public bool isRequested;
        public LOD lod;
        Action onReceive;

        bool hasMesh;

        public LODMesh(LOD lod, Action onReceive)
        {
            this.lod = lod;
            this.onReceive = onReceive;
        }

        public Mesh GetMesh()
        {
            if (IsEmpty())
            {
                return null;
            }

            return mesh;
        }

        public void RequestMesh(float[,] heightMap)
        {
            if (!isRequested)
            {
                isRequested = true;
                terrain.RequestSquareMesh(OnMeshReceived, heightMap, lod.level);
            }
        }

        public bool IsEmpty()
        {
            return !hasMesh;
        }

        void OnMeshReceived(SquareMesh squareMesh)
        {
            mesh = squareMesh.Generate();
            hasMesh = true;
            onReceive();
        }
    }
}

