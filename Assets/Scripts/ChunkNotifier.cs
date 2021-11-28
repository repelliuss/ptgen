using UnityEngine;
using UnityEngine.Assertions;

public class ChunkNotifier : MonoBehaviour
{
    private GameObject _terrain;
    private Chunker _chunker;

    public void Start()
    {
        _terrain = GameObject.FindGameObjectWithTag("ChunkedTerrain");
        _chunker = _terrain?.GetComponent<Chunker>();
        Assert.IsNotNull(_chunker, "No ChunkedTerrain or it doesn't have a Chunker");
    }

    void Update()
    {
        if (_chunker.UpdatePipeline(transform.position))
        {
            _chunker.GeneratePipeline();
        }
    }
}
