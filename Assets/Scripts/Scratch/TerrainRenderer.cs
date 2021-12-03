using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainRenderer : MonoBehaviour
{
    MeshFilter meshFilter;

    public void DrawFromHeightMap(LandData landData, MeshData meshData)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = meshData.MakeMesh();
        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<ProceduralLand>().landPreset.scale;
    }
}
