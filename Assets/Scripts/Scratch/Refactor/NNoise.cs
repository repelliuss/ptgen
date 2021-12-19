using UnityEngine;

public static class NNoise
{
    public static float PrimaryNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }
}
