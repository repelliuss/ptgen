using UnityEngine;
using System.Collections.Generic;

public class Chunker : MonoBehaviour
{
    public int chunkCount = 3;
    public int chunkLength = 32;
    public Chunk chunk;

    public int marginCount = 1;

    private int _marginDist;

    private Vector3 _terrainStart;
    private Vector3 _terrainEnd;

    private Queue<(int, int)> pipeline;

    void Start()
    {
        chunk.SetSize(chunkLength);

        _terrainStart = transform.position;
        _terrainEnd = Algebra.SlideInPlane(_terrainStart, chunkLength * marginCount);

        if (marginCount < 1)
            marginCount = 1;

        _marginDist = marginCount * chunkLength;

        Instantiate(chunk, transform.position, transform.rotation);
    }

    private int CheckMarginHorizontally(Vector3 pos)
    {
        if (Algebra.LinearDistance(pos.z, _terrainStart.z) < _marginDist)
            return -chunkLength;

        if (Algebra.LinearDistance(pos.z, _terrainEnd.z) < _marginDist)
            return chunkLength;

        return 0;
    }

    public int CheckMarginVertically(Vector3 pos)
    {
        if (Algebra.LinearDistance(pos.x, _terrainStart.x) < _marginDist)
            return -chunkLength;

        if (Algebra.LinearDistance(pos.x, _terrainEnd.x) < _marginDist)
            return chunkLength;

        return 0;
    }

    public bool UpdatePipeline(Vector3 pos)
    {
        int x = CheckMarginVertically(pos);
        int z = CheckMarginHorizontally(pos);

        pipeline.Enqueue((x, z));

        return x != 0 || z != 0;
    }

    public void GeneratePipeline()
    {
        Debug.Log("Generating Pipeline");
    }
}
