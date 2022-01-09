using UnityEngine;

public class Erosion
{
    float[,] heightMap;
    HeightMapNeighboursData neighbourData;
    int width;
    int height;

    public Erosion(float[,] heightMap, HeightMapNeighboursData neighbourData)
    {
        this.heightMap = heightMap;
        this.neighbourData = neighbourData;
        this.width = heightMap.GetLength(0);
        this.height = heightMap.GetLength(1);
    }

    public void Thermal(ErosionParams thermalParam)
    {
        for (int y = 3 ; y < height - 3; ++y)
        {
            for(int x = 3; x < width - 3; ++x)
            {
                foreach(Vector2Int neighbour in neighbourData.neighbours[x, y])
                {
                    float curHeight = heightMap[x, y];
                    float neighbourHeight = heightMap[neighbour.x, neighbour.y];
                    if(curHeight > neighbourHeight + thermalParam.strength)
                    {
                        heightMap[x, y] -= curHeight * thermalParam.power;
                        heightMap[neighbour.x, neighbour.y] += curHeight * thermalParam.power;
                    }
                }
            }
        }
    }

    public void Wind(ErosionParams windParam)
    {
        for (int y = 10; y < height - 25; y += 10)
        {
            for (int x = 10; x < width - 10; ++x)
            {
                int noise = (int)(NNoise.PrimaryNoise(x * 0.06f, y * 0.06f)
                                  * 20 * windParam.strength);
                int nx = x;
                int digy = y + noise;
                int ny = y + 5 + noise;

                if(ny < height)
                {
                    heightMap[x, digy] -= windParam.power;
                    heightMap[nx, ny] += windParam.power;
                }
            }
        }
    }

    public void GenerateFalloffMap(FalloffParams param)
    {
        for (int i = 0; i < height; ++i) {
            for (int j = 0; j < width; ++j) {
                float x = (i/(float)width) * 2 - 1;
                float y = (j/(float)height) * 2 - 1;

                float val = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                heightMap[i, j] =  heightMap[i, j] - Evaluate(val, param) * param.scale;
            }
        }
    }

    float Evaluate(float val, FalloffParams param)
    {
        float a = param.a;
        float b = param.b;

        return Mathf.Pow(val, a) / (Mathf.Pow(val, a) + Mathf.Pow(b - b * val, a));
    }
}
