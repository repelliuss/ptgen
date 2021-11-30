using UnityEngine;
using System.Collections.Generic;

public class InfiniteTerrain : MonoBehaviour
{
    public const float viewDistance = 300;
    public Transform player;

    public static Vector2 playerPos;

    int chunkSize;
    int chunksVisible;

    Dictionary<Vector2, TerrainChunk> chunks;
    List<TerrainChunk> lastActiveChunks;

    //REVIEW: static
    static ProceduralLand proceduralLand;

    void Start()
    {
        chunkSize = ProceduralLand.chunkSize - 1;
        chunksVisible = Mathf.RoundToInt(viewDistance / chunkSize);
        chunks = new Dictionary<Vector2, TerrainChunk>();
        lastActiveChunks = new List<TerrainChunk>();

        proceduralLand = FindObjectOfType<ProceduralLand>();
    }

    void Update()
    {
        playerPos = new Vector2(player.position.x, player.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        foreach(TerrainChunk chunk in lastActiveChunks)
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

                if(chunks.TryGetValue(curChunkCoord, out curChunk))
                {
                    if(curChunk.TryActivate())
                    {
                        lastActiveChunks.Add(curChunk);
                    }
                }
                else {
                    chunks.Add(curChunkCoord, new TerrainChunk(curChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public readonly struct TerrainChunk
    {
        readonly Vector2 pos;
        readonly GameObject land;
        readonly Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            pos = coord * size;
            bounds = new Bounds(pos, Vector2.one * size);
            Vector3 pos3 = new Vector3(pos.x, 0, pos.y);

            land = GameObject.CreatePrimitive(PrimitiveType.Plane);
            land.transform.position = pos3;
            land.transform.localScale = Vector3.one * size / 10f;
            land.transform.parent = parent;
            SetActive(false);

            // proceduralLand.RequestLandData(OnLandDataReceived);
        }

        public bool TryActivate()
        {
            float distToPlayer= Mathf.Sqrt(bounds.SqrDistance(playerPos));
            bool isVisible = distToPlayer <= viewDistance;

            SetActive(isVisible);

            return isVisible;
        }

        public void SetActive(bool val)
        {
            land.SetActive(val);
        }

        void OnLandDataReceived(LandData landData)
        {
            Debug.Log("Map data received");
        }
    }
}
