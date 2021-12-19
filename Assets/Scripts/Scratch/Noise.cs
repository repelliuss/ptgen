using UnityEngine;
using UnityEngine.Assertions;

public enum NormalizeMode { LOCAL, GLOBAL };

public static class Noise
{
    public static float OldFBM(float x, float y,
                            int octaves, float persistance, float lacunarity)
    {
        float height = 0;
        float frequency = 1;
        float amplitude = 1;

        for (int i = 0; i < octaves; ++i)
        {
            height += PrimaryNoise(x * frequency, y * frequency) * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return height;
    }

    static Vector2[] calculateOctaveOffsets(int octaves, Vector2 offset, int seed, float persistance, out float maxPossibleHeight)
    {

        System.Random prng = new System.Random(seed);
        var octaveOffsets = new Vector2[octaves];
        float amplitude = 1;
        maxPossibleHeight = 0;

        for (int i = 0; i < octaves; ++i)
        {
            //TODO: experiment with -10000, 10000 range
            octaveOffsets[i].x = prng.Next(-10000, 10000) + offset.x;
            octaveOffsets[i].y = prng.Next(-10000, 10000) - offset.y;

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        return octaveOffsets;
    }

    public static float FBM(float x, float y,
                            float scale, int octaves, float persistance, float lacunarity,
                            int seed, Vector2 offset, Vector2[] octaveOffsets)
    {
        float height = 0;

        float frequency = 1;
        float amplitude = 1;

        for (int i = 0; i < octaves; ++i)
        {
            height += (PrimaryNoise(((x + octaveOffsets[i].x) / scale) * frequency,
                                    ((y + octaveOffsets[i].y) / scale) * frequency) * 2 - 1) * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return height;
    }

    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }

    public static float[,] GeneratePerlinNoiseMap(int width, int height, float scale,
                                                  int octaves, float persistance, float lacunarity,
                                                  int seed, Vector2 offset, NormalizeMode mode)
    {
        float[,] map = new float[width, height];

        Assert.IsTrue(scale > 0.0f, "scale factor is not positive");

        float max = float.MinValue;
        float min = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        float maxPossibleHeight;
        var octaveOffsets = calculateOctaveOffsets(octaves, offset, seed, persistance, out maxPossibleHeight);

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                float noise = FBM(x - halfWidth, y - halfHeight, scale,
                                  octaves, persistance, lacunarity,
                                  seed, offset, octaveOffsets);
                map[x, y] = noise;

                if (noise > max)
                    max = noise;
                else if (noise < min)
                    min = noise;
            }
        }
        
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (mode == NormalizeMode.LOCAL)
                {
                    map[x, y] = Mathf.InverseLerp(min, max, map[x, y]);
                }
                else {
                    //REVIEW: 1.8f constant
                    float normalizedHeight = (map[x,y] + 1) / maxPossibleHeight;
                    map[x,y] = normalizedHeight;
                }
            }
        }

        return map;
    }

    public static float PrimaryNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }
}
