using UnityEngine;

public class TerrainRenderer : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Gradient gradient;

    public void DrawFromHeightMap(float[,] noiseMap, MeshData meshData)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                colorMap[y * width + x] = gradient.Evaluate(noiseMap[x, y]);
            }
        }

        texture.SetPixels(colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        meshRenderer.sharedMaterial.mainTexture = texture;
        meshFilter.sharedMesh = meshData.MakeMesh();
    }
}
