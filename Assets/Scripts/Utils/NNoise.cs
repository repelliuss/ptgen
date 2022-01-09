using UnityEngine;

public static class NNoise
{
    public static float PrimaryNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }

    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}
