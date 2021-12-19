using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(ProceduralTerrain))]
class TerrainChunker : MonoBehaviour
{
    public Transform player;
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

        UpdateChunks();
    }

    void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z) /
            terrain.param.uniformScale;
        if (!updateTreshold.Contains(playerPos))
        {
            UpdateChunks();
            updateTreshold = CalculateUpdateBounds();
        }

        foreach (Chunk chunk in activeChunks)
        {
            chunk.TryBakeCollider();
        }
    }

    Bounds CalculateUpdateBounds()
    {
        return new Bounds(new Vector2(player.position.x, player.position.z),
                          Vector2.one * updateBoundsSize);
    }

    void ClearActiveChunks()
    {
        foreach(Chunk chunk in activeChunks)
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

        public Chunk(Vector2 position, int chunkSize,
                     Material material,
                     TerrainChunker parent, LOD[] lods,
                     int colliderFromLOD)
        {
            chunk = new GameObject("Chunk " + position);
            chunk.transform.position = new Vector3(position.x, 0, position.y)
                * terrain.param.uniformScale;
            chunk.transform.localScale = Vector3.one * terrain.param.uniformScale;
            chunk.transform.parent = parent.transform;

            chunkBounds = new Bounds(position, Vector2.one * chunkSize);

            chunk.AddComponent<MeshRenderer>().material = material;
            meshFilter = chunk.AddComponent<MeshFilter>();
            meshCollider = chunk.AddComponent<MeshCollider>();

            this.lods = lods;
            lodMeshes = new LODMesh[lods.Length];
            for(int i = 0; i < lods.Length; ++i)
            {
                if(i == colliderFromLOD)
                {
                    lodMeshes[i] = new LODMesh(lods[i],
                                               () => {
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

            terrain.RequestHeightMap(OnHeightMapReceived, position);
        }

        public void UpdatePresence()
        {
            if(heightMap != null)
            {
                float distToPlayer = Mathf.Sqrt(chunkBounds.SqrDistance(playerPos));
                bool isVisible = distToPlayer <= maxViewDistance;

                if(isVisible)
                {
                    int lodIndex = 0;
                    int i = 0;

                    while(i < lods.Length - 1 &&
                          distToPlayer > lods[i].viewDistance)
                    {
                        ++i;
                        ++lodIndex;
                    }

                    if(lodIndex != prevLODIndex)
                    {
                       LODMesh mesh = lodMeshes[lodIndex];
                       if(!mesh.IsEmpty())
                       {
                           prevLODIndex = lodIndex;
                           meshFilter.mesh = mesh.GetMesh();
                       }
                       else {
                           mesh.RequestMesh(heightMap);
                       }
                    }

                    colliderMesh.RequestMesh(heightMap);

                    activeChunks.Add(this);
                }

                SetActive(isVisible);
            }
        }

        public void TryBakeCollider()
        {
            float distToPlayer = chunkBounds.SqrDistance(playerPos);
            if(distToPlayer < colliderDistanceTreshold * colliderDistanceTreshold)
            {
                if(!colliderMesh.IsEmpty())
                {
                    meshCollider.sharedMesh = colliderMesh.GetMesh();
                }
            }
        }

        public void SetActive(bool activity)
        {
            chunk.SetActive(activity);
        }

        void OnHeightMapReceived(float[,] heightMap)
        {
            this.heightMap = heightMap;
            UpdatePresence();
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
            if(IsEmpty())
            {
                return null;
            }

            return mesh;
        }

        public void RequestMesh(float[,] heightMap)
        {
            if(!isRequested) {
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

