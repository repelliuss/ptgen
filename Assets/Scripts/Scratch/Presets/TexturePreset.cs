using UnityEngine;

[CreateAssetMenu()]
public class TexturePreset : UpdatablePreset
{
    public Color[] colors;
    [Range(0,1)]
    public float[] colorStartHeights;

    float lastMinHeight;
    float lastMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("colorCount", colors.Length);
        material.SetColorArray("colors", colors);
        material.SetFloatArray("colorStartHeights", colorStartHeights);

        UpdateMeshHeights(material, lastMinHeight, lastMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        lastMinHeight = minHeight;
        lastMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }
}
