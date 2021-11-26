using UnityEngine;

public static class Noise
{
    public static float fBM(float x, float z, int octaveCount,
                            float persistance, float lacunarity)
    {
        float total = 0;
        float frequency = 1;
        float amp = 1;
        float maxVal = 0;

        for (int i = 0; i < octaveCount; ++i)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amp;
            maxVal += amp;
            amp *= persistance;
            frequency *= lacunarity;
        }

        return total / maxVal;
    }

    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}
