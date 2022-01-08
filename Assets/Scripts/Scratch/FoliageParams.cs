using UnityEngine;

[CreateAssetMenu()]
public class FoliageParams : UpdatableScriptableObject
{
    public GameObject gobject;

    public float minHeight = 0f;
    public float maxHeight = 64f;

    [Range(0, 90)]
    public float minSlope = 0;

    [Range(0, 90)]
    public float maxSlope = 90;

    public Vector2 widthScale;
    public Vector2 heightScale;

    public float baseWidthScale = 0f;
    public float baseHeightScale = 0f;

    public float minScatter = 0f;
    public float maxScatter = 0f;

    [Min(1)]
    public float xSpacing = 1f;

    [Min(1)]
    public float ySpacing = 1f;

    public Color color1 = Color.white;
    public Color color2 = Color.white;

    [Range(-1, 1)]
    public float color2Bias = 0f;

    [Range(0, 360)]
    public int minRotation = 0;

    [Range(0, 360)]
    public int maxRotation = 360;

    public float overlap = 0.01f;
    public float feather = 0.05f;

    [Range(0, 1)]
    public float density = 0.5f;

    public int maxCount = 100;
}
