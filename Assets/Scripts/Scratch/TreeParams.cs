using UnityEngine;

[CreateAssetMenu()]
public class TreeParams : UpdatableScriptableObject
{
    public GameObject gobject;
    
    public float minHeight = 0f;
    public float maxHeight = 64f;

    [Range(0, 90)]
    public float minSlope = 0;

    [Range(0, 90)]
    public float maxSlope = 90;
    
    public float minScale = 0.5f;
    public float maxScale = 1.0f;

    public float baseScale = 0f;
    
    public float minScatter = 1f;
    public float maxScatter = 1f;

    [Min(1)]
    public int xSpacing = 1;

    [Min(1)]
    public int ySpacing = 1;
    
    public float yMinOffset = 0f;
    public float yMaxOffset = 0f;
    
    public Color color1 = Color.white;
    public Color color2 = Color.white;

    [Range(-1, 1)]
    public float color2Bias = 0f;

    [Range(0, 360)]
    public int minRotation = 0;

    [Range(0, 360)]
    public int maxRotation = 360;

    [Range(0, 1)]
    public float density = 0.5f;

    public int maxCount = 100;
}
