using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainRenderer : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public void DrawFromHeightMap(LandData landData, MeshData meshData)
    {
        int width = landData.heightMap.GetLength(0);
        int height = landData.heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        //TODO: remove it from here
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        texture.SetPixels(landData.colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        meshRenderer.sharedMaterial.mainTexture = texture;
        meshFilter.sharedMesh = meshData.MakeMesh();
    }
}
