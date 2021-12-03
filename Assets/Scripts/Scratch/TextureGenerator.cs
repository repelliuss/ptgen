using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D GenerateFromLandData(LandData data)
    {
        int size = data.heightMap.GetLength(0);
        var texture = new Texture2D(size, size);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }
}
