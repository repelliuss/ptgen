using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TexturePreset : UpdatableScriptableObject
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    const int maxLayerCount = 10;

    public Layer[] layers = new Layer[maxLayerCount];

    float lastMinHeight;
    float lastMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("tints", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("startHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("blendStrengths", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("tintStrengths", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("textureScales", layers.Select(x => x.textureScale).ToArray());

        var textures = PrepareLayerTextures(layers.Select(x => x.texture).ToArray());
        material.SetTexture("textures", textures);

        UpdateMeshHeights(material, lastMinHeight, lastMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        lastMinHeight = minHeight;
        lastMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    Texture2DArray PrepareLayerTextures(Texture2D[] textures)
    {
        var textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

        for (int i = 0; i < textures.Length; ++i) {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        textureArray.Apply();

        return textureArray;
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public float textureScale;
        public Color tint;
        [Range(0,1)]
        public float tintStrength;
        [Range(0,1)]
        public float startHeight;
        [Range(0,1)]
        public float blendStrength;
    }
}
