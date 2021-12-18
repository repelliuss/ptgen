using UnityEngine;
using UnityEngine.Assertions;

public class GradientHeightMapMaker
{
    GradientHeightMapParams param;
    float maxHeight;
    Vector2[] octaveOffsets;

    public GradientHeightMapMaker(GradientHeightMapParams param)
    {
        this.param = param;
        this.maxHeight = param.CalculateMaxHeight();
        this.octaveOffsets = param.CalculateOctaveOffsets();
    }

    public float[,] Make()
    {
        int size = GradientHeightMapParams.size;
        float[,] heightMap = new float[size, size];

        Assert.IsTrue(param.noiseScale > 0.0f, "scale factor is not positive");

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                float height = FBM(x, y);
                height = Normalize(height);
                height = param.heightCurve.Evaluate(height);
                height *= param.heightScale;
                heightMap[x, y] = height;

            }
        }

        return heightMap;
    }

    float FBM(float x, float y)
    {
        float height = 0;
        float frequency = 1;
        float amplitude = 1;

        float halfSize = GradientHeightMapParams.size / 2;

        for (int i = 0; i < param.octaveCount; ++i)
        {
            float effectiveX = CalculateEffectiveX(x, halfSize, i, frequency);
            float effectiveY = CalculateEffectiveY(y, halfSize, i, frequency);

            float noise = NNoise.PrimaryNoise(effectiveX, effectiveY);
            noise = (noise * 2 - 1) * amplitude;

            height += noise;

            amplitude *= param.persistance;
            frequency *= param.lacunarity;
        }

        return height;
    }

    float CalculateEffectiveX(float x, float halfSize, int octaveIndex,
                              float frequency)
    {
        return (x
                - halfSize
                + octaveOffsets[octaveIndex].x
                + param.landOffset.x)
            / param.noiseScale
            * frequency;
    }

    float CalculateEffectiveY(float y, float halfSize, int octaveIndex,
                                     float frequency)
    {
        return (y
                - halfSize
                + octaveOffsets[octaveIndex].y
                - param.landOffset.y)
            / param.noiseScale
            * frequency;
    }

    /// unfold value and normalize with a constant
    /// (value + 1) / 2f * maxValue * constant
    /// here constant is 2f
    float Normalize(float height)
    {
        return (height + 1) / maxHeight;
    }
}
