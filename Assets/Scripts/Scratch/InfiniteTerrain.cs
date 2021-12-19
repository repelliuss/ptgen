using UnityEngine;
using System.Collections.Generic;

public class InfiniteTerrain : MonoBehaviour
{
    public Transform player;
    //REVIEW: is this a good place
    public Material material;

    public LODInfo[] lodLevels;
    static float viewDistance;

    public int colliderLODIndex;

    public static Vector2 playerPos;
    Vector2 oldPlayerPos;

    const float playerMoveTresholdBeforeUpdate = 32f;
    const float sqrPlayerMoveTresholdBeforeUpdate = playerMoveTresholdBeforeUpdate * playerMoveTresholdBeforeUpdate;
    const float colliderTreshold = 5;

    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunks;
    static List<TerrainChunk> lastActiveChunks;

    //REVIEW: static
    static ProceduralLand proceduralLand;

    void Start()
    {
        viewDistance = lodLevels[lodLevels.Length - 1].visibilityThreshold;
        chunkSize = ProceduralLand.chunkSize - 1;
        chunksVisible = Mathf.RoundToInt(viewDistance / chunkSize);
        chunks = new Dictionary<Vector2, TerrainChunk>();
        lastActiveChunks = new List<TerrainChunk>();

        proceduralLand = FindObjectOfType<ProceduralLand>();

        UpdateVisibleChunks();
    }

    void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z) / proceduralLand.landPreset.scale;

        if (oldPlayerPos != playerPos)
        {
            foreach (TerrainChunk chunk in lastActiveChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((oldPlayerPos - playerPos).sqrMagnitude > sqrPlayerMoveTresholdBeforeUpdate)
        {
            oldPlayerPos = playerPos;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        foreach (TerrainChunk chunk in lastActiveChunks)
        {
            chunk.SetActive(false);
        }
        lastActiveChunks.Clear();

        int playerOnChunkCoordX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int playerOnChunkCoordY = Mathf.RoundToInt(playerPos.y / chunkSize);

        for (int yOffset = -chunksVisible; yOffset <= chunksVisible; ++yOffset)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; ++xOffset)
            {
                Vector2 curChunkCoord = new Vector2(playerOnChunkCoordX + xOffset,
                                                    playerOnChunkCoordY + yOffset);
                TerrainChunk curChunk;

                if (chunks.TryGetValue(curChunkCoord, out curChunk))
                {
                    curChunk.TryActivate();
                }
                else
                {
                    chunks.Add(curChunkCoord, new TerrainChunk(curChunkCoord, chunkSize, lodLevels, transform, material, colliderLODIndex));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 pos;
        GameObject land;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] lodLevels;
        LODMesh[] lodMeshes;

        int colliderLODIndex;

        LandData landData;
        bool isLandDataReceived;
        int curLODIndex = -1;

        bool hasCollider = false;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] lodLevels, Transform parent, Material material, int colliderLODIndex)
        {
            this.colliderLODIndex = colliderLODIndex;
            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 pos3 = new Vector3(pos.x, 0, pos.y);

            land = new GameObject("Terrain Chunk " + coord);
            land.transform.position = pos3 * proceduralLand.landPreset.scale;
            land.transform.localScale = Vector3.one * proceduralLand.landPreset.scale;
            land.transform.parent = parent;

            meshRenderer = land.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = land.AddComponent<MeshFilter>();
            meshCollider = land.AddComponent<MeshCollider>();

            this.lodLevels = lodLevels;
            lodMeshes = new LODMesh[lodLevels.Length];
            for (int i = 0; i < lodLevels.Length; ++i)
            {
                if (i == colliderLODIndex)
                {
                    lodMeshes[i] = new LODMesh(lodLevels[i].lod, () =>
                                               {
                                                   UpdateCollisionMesh();
                                                   return TryActivate();
                                               });
                }
                else
                {
                    lodMeshes[i] = new LODMesh(lodLevels[i].lod, TryActivate);
                }
            }

            SetActive(false);

            proceduralLand.RequestLandData(OnLandDataReceived, pos);
        }

        public bool TryActivate()
        {
            if (isLandDataReceived)
            {
                float distToPlayer = Mathf.Sqrt(bounds.SqrDistance(playerPos));
                bool isVisible = distToPlayer <= viewDistance;

                if (isVisible)
                {
                    int lodIndex = 0;
                    int i = 0;
                    while (i < lodLevels.Length - 1 &&
                          distToPlayer > lodLevels[i].visibilityThreshold)
                    {
                        ++i;
                        ++lodIndex;
                    }

                    if (lodIndex != curLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            curLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.isRequested)
                        {
                            lodMesh.RequestMesh(landData);
                        }
                    }

                    lastActiveChunks.Add(this);
                }

                SetActive(isVisible);

                return isVisible;
            }

            return land.activeSelf;
        }

        public void UpdateCollisionMesh()
        {
            if (!hasCollider)
            {
                float distToPlayer = bounds.SqrDistance(playerPos);

                if (distToPlayer < lodLevels[colliderLODIndex].GetSqrVisibleTreshold())
                {
                    if (!lodMeshes[colliderLODIndex].isRequested)
                    {
                        lodMeshes[colliderLODIndex].RequestMesh(landData);
                    }
                }

                if (distToPlayer < colliderTreshold * colliderTreshold)
                {
                    if (lodMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasCollider = true;
                    }
                }
            }
        }

        public void SetActive(bool val)
        {
            land.SetActive(val);
        }

        void OnLandDataReceived(LandData landData)
        {
            this.landData = landData;
            isLandDataReceived = true;

            TryActivate();
        }
    }

    class LODMesh
    {
        public Mesh mesh = null;
        public bool isRequested;
        public bool hasMesh;
        int lod;

        System.Func<bool> activationCallback;

        public LODMesh(int lod, System.Func<bool> activationCallback)
        {
            this.lod = lod;
            this.activationCallback = activationCallback;
        }

        public void RequestMesh(LandData landData)
        {
            isRequested = true;
            proceduralLand.RequestMeshData(OnMeshDataReceived, landData, lod);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.MakeMesh();
            hasMesh = true;
            activationCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibilityThreshold;

        public float GetSqrVisibleTreshold()
        {
            return visibilityThreshold * visibilityThreshold;
        }
    }
}
