using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Forest
{
    const float placementOffset = 5f;

    readonly TreeParams[] parameters;
    readonly Vector2 center;
    readonly Transform parent;

    readonly int seed;
    readonly float maxSteepness;

    bool isPlanted;
    List<Action> readyTrees;

    public Forest(TreeParams[] parameters, Vector2 center,
                      Transform parent, int seed)
    {
        this.parameters = parameters;
        this.center = center;
        this.parent = parent;
        this.seed = seed;

        float[,] maxSlopeMap = new float[,] { { 0, 1, }, { 1, 1 } };
        this.maxSteepness = Math.GetSteepness(maxSlopeMap, 0, 1, 0, 0, 2, 2);

        this.readyTrees = new List<Action>();
    }

    public bool IsPlanted()
    {
        return isPlanted;
    }

    float RandomInRange(System.Random random, float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    public void Plant()
    {
        foreach(Action action in readyTrees)
        {
            action();
        }
    }

    public IEnumerator PlantDeferred()
    {
        int count = 0;
        foreach (Action action in readyTrees)
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
            TreeParams param = parameters[i];
            GameObjectMaker maker = new GameObjectMaker(param.gobject);
            float maxSlope = Mathf.Lerp(0, maxSteepness, param.maxSlope / 90);
            float minSlope = Mathf.Lerp(0, maxSteepness, param.minSlope / 90);
            // + 1 is because anchor of a tree is not at its perfect center
            // so when rotated, it looks out of chunk
            int innerHeight = height - placementOffsetAsInt - 1 - Mathf.CeilToInt(param.ySpacing);
            int innerWidth = width - placementOffsetAsInt - 1 - Mathf.CeilToInt(param.xSpacing);
            int treeCount = 0;
            for (int y = placementOffsetAsInt + 1; y < innerHeight; y += param.ySpacing)
            {
                for (int x = placementOffsetAsInt + 1; x < innerWidth; x += param.xSpacing)
                {
                    if (random.NextFloat() > param.density) continue;

                    float effectiveX = (x - halfWidth) + center.x;
                    float effectiveY = (y - halfHeight) + center.y;

                    float offsetX = random.Range(-placementOffset, placementOffset);
                    float offsetY = random.Range(-placementOffset, placementOffset);

                    effectiveX += offsetX;
                    effectiveY += offsetY;

                    float h = Math.InterpolateHeight(x, y, offsetX, offsetY, heightMap);

                    float steepness = Math.GetSteepness(heightMap,
                           h,
                           maxHeight,
                           Mathf.RoundToInt(x + offsetX),
                           Mathf.RoundToInt(y + offsetY),
                           width,
                           height);

                    h += random.Range(param.yMinOffset, param.yMaxOffset);

                    if (h >= param.minHeight && h <= param.maxHeight &&
                       steepness >= minSlope && steepness <= param.maxSlope)
                    {
                        float scale = random.Range(param.minScale, param.maxScale) +
                            param.baseScale;
                        Vector3 scale3 = new Vector3(scale, scale, scale);
                        int yRotation = random.Range(param.minRotation,
                                                     param.maxRotation);
                        Vector3 position = new Vector3(effectiveX,
                                                       h + scale * 0.5f,
                                                       effectiveY);

                        readyTrees.Add(
                            () => maker.Make(position, scale3,
                                             yRotation, parent,
                                             param.color1, param.color2,
                                             param.color2Bias));

                        if (++treeCount >= param.maxCount) goto NEXT_PARAM;

                        float xScatter = random.Range(param.minScatter,
                                                      param.maxScatter);
                        x += (int)(scale + xScatter);

                        float yScatter = random.Range(param.minScatter,
                                                       param.maxScatter);
                        y += (int)(scale + yScatter);
                        if (y >= innerHeight) return;
                    }

                }
            }
            NEXT_PARAM:
            continue;
        }
    }
}
