using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Foliage
{
    const float placementOffset = 5f;

    readonly FoliageParams[] parameters;
    readonly Vector2 center;
    readonly Transform parent;

    readonly int seed;
    readonly float maxSteepness;

    readonly float uniformScale;

    bool isPlanted;
    List<Action> readyFoliage;

    public Foliage(FoliageParams[] parameters, Vector2 center,
                   Transform parent, int seed, float uniformScale)
    {
        this.parameters = parameters;
        this.center = center;
        this.parent = parent;
        this.seed = seed;
        this.uniformScale = uniformScale;

        float[,] maxSlopeMap = new float[,] { { 0, 1, }, { 1, 1 } };
        this.maxSteepness = Math.GetSteepness(maxSlopeMap, 0, 1, 0, 0, 2, 2);

        this.readyFoliage = new List<Action>();
    }

    public bool IsPlanted()
    {
        return isPlanted;
    }

    public void Plant()
    {
        foreach (Action action in readyFoliage)
        {
            action();
        }
    }

    public IEnumerator PlantDeferred()
    {
        int count = 0;
        foreach (Action action in readyFoliage)
        {
            action();
            ++count;
            if(count >= ProceduralTerrain.createAtOnce)
            {
                count = 0;
                yield return new WaitForSeconds(ProceduralTerrain.createInterval);
            }
        }
    }

    public void Bake(float[,] heightMap, float maxHeight, bool force = false)
    {
        if (isPlanted && !force) return;

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float halfWidth = width / 2;
        float halfHeight = height / 2;
        int placementOffsetAsInt = Mathf.CeilToInt(placementOffset);
        SysRand random = new SysRand(seed);

        isPlanted = true;

        for (int i = 0; i < parameters.Length; ++i)
        {
            FoliageParams param = parameters[i];
            GameObjectMaker maker = new GameObjectMaker(param.gobject);
            float maxSlope = Mathf.Lerp(0, maxSteepness, param.maxSlope / 90);
            float minSlope = Mathf.Lerp(0, maxSteepness, param.minSlope / 90);
            // + 1 is because anchor of a tree is not at its perfect center
            // so when rotated, it looks out of chunk
            int innerHeight = height - placementOffsetAsInt - 1 - Mathf.CeilToInt(param.ySpacing);
            int innerWidth = width - placementOffsetAsInt - 1 - Mathf.CeilToInt(param.xSpacing);
            int foliageCount = 0;
            for (float y = placementOffsetAsInt + 1; y < innerHeight; y += param.ySpacing)
            {
                for (float x = placementOffsetAsInt + 1; x < innerWidth; x += param.xSpacing)
                {
                    if (random.NextFloat() > param.density) continue;

                    float effectiveX = (x - halfWidth) + center.x;
                    float effectiveY = (y - halfHeight) + center.y;

                    float offsetX = random.Range(-placementOffset, placementOffset);
                    float offsetY = random.Range(-placementOffset, placementOffset);

                    effectiveX += offsetX;
                    effectiveY += offsetY;

                    float noise = NNoise.Map(NNoise.PrimaryNoise(x * param.feather,
                                                                y * param.feather),
                                            0, 1, 0.5f, 1);

                    float overlap = param.overlap * noise;
                    float heightLow = param.minHeight * noise - overlap;
                    float heightHigh = param.maxHeight * noise + overlap;

                    float h = Math.InterpolateHeight(Mathf.RoundToInt(x),
                                                     Mathf.RoundToInt(y),
                                                     offsetX, offsetY, heightMap);

                    float steepness = Math.GetSteepness(heightMap,
                                                        h,
                                                        maxHeight,
                                                        Mathf.RoundToInt(x + offsetX),
                                                        Mathf.RoundToInt(y + offsetY),
                                                        width,
                                                        height);

                    if (h >= heightLow && h <= heightHigh &&
                       steepness >= minSlope && steepness <= maxSlope)
                    {
                        float widthScale = random.Range(param.widthScale.x,
                                                    param.widthScale.y);
                        float yScale = random.Range(param.heightScale.x,
                                                    param.heightScale.y);
                        Vector3 scale3 = new Vector3(widthScale + param.baseWidthScale,
                                                     yScale + param.baseHeightScale,
                                                     widthScale + param.baseWidthScale);

                        int yRotation = random.Range(param.minRotation,
                                                     param.maxRotation);

                        Vector3 position = new Vector3(effectiveX,
                                                       h,
                                                       effectiveY) * uniformScale;

                        readyFoliage.Add(
                            () => maker.Make(position, scale3,
                                             yRotation, parent,
                                             param.color1, param.color2,
                                             param.color2Bias));

                        if (++foliageCount >= param.maxCount) goto NEXT_PARAM;

                        float xScatter = random.Range(param.minScatter,
                                                      param.maxScatter);
                        x += xScatter;

                        float yScatter = random.Range(param.minScatter,
                                                      param.maxScatter);
                        y += yScatter;
                        if (y >= innerHeight) return;
                    }
                }
            }
            NEXT_PARAM:
            continue;
        }
    }
}
