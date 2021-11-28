using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField]
    private Terrain _terrain;

    public void SetSize(int length)
    {
        _terrain.terrainData.size = new Vector3(length,
                                                _terrain.terrainData.size.y,
                                                length);
    }
}
