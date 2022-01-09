using UnityEngine;

//TODO: rename file
public class GradientHeightMapMaker
{
    GradientHeightMapParams param;
    float maxHeight;
    Vector2[][] octaveOffsets;
    AnimationCurve heightCurve;

    public GradientHeightMapMaker(GradientHeightMapParams param)
    {
        this.param = param;
        this.maxHeight = param.CalculateMaxFBMValue();
        this.octaveOffsets = param.CalculateOctaveOffsets();
        this.heightCurve = new AnimationCurve(param.heightCurve.keys); // For threading
    }

    public float[,] Make(Vector2 center)
    {
        int size = GradientHeightMapParams.size;
        float[,] heightMap = new float[size, size];

        //TODO: remove these
        float max = float.MinValue;
        float min = float.MaxValue;

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                float height = FBM(x, y, center);
                height = Normalize(height, maxHeight);
                height = heightCurve.Evaluate(height);
                height *= param.heightScale;
                heightMap[x, y] = height;
                if(max < height)
                max = height;
                if(min > height)
                min = height;
            }
        }

        if(param.thermalParam != null)
        {
            Erosion erosion = new Erosion(heightMap, param.thermalParam);
            erosion.Thermal();
        }

        return heightMap;
    }

    float FBM(float x, float y, Vector2 center)
    {
        float halfSize = GradientHeightMapParams.size / 2;
        NoiseParams[] noises = param.noises;
        float height = 0;

        for (int j = 0; j < noises.Length; ++j)
        {
            float curHeight = 0;
            float frequency = 1;
            float amplitude = 1;

            for (int i = 0; i < noises[j].octaveCount; ++i)
            {
                float effectiveX = CalculateEffectiveX(x, halfSize, j, i, frequency, center);
                float effectiveY = CalculateEffectiveY(y, halfSize, j, i, frequency, center);

                float noise = NNoise.PrimaryNoise(effectiveX, effectiveY);
                noise = (noise * 2 - 1) * amplitude;

                curHeight += noise;

                amplitude *= noises[j].persistance;
                frequency *= noises[j].lacunarity;
            }

            height += curHeight;
        }

        return height / noises.Length;
    }

    float CalculateEffectiveX(float x, float halfSize,
                              int noiseIndex, int octaveIndex,
                              float frequency, Vector2 center)
    {
        return (x
                - halfSize
                + octaveOffsets[noiseIndex][octaveIndex].x
                + param.landOffset.x
                + center.x)
            / param.noises[noiseIndex].noiseScale
            * frequency;
    }

    float CalculateEffectiveY(float y, float halfSize,
                              int noiseIndex, int octaveIndex,
                              float frequency, Vector2 center)
    {
        return (y
                - halfSize
                + octaveOffsets[noiseIndex][octaveIndex].y
                + param.landOffset.y
                + center.y)
            / (param.noises[noiseIndex].noiseScale)
            * frequency;
    }

    /// unfold value and normalize with a constant
    /// (value + 1) / 2f * maxValue * constant
    /// here constant is 2f
    public static float Normalize(float height, float maxHeight)
    {
        // REVIEW: check
        // return (height + maxHeight) / (maxHeight * 2f);
        return (height + 1) / maxHeight;
    }
}
