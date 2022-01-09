using UnityEngine;
using System.Collections.Generic;

public class Erosion
{
    float[,] heightMap;
    ThermalParams thermalParam;

    public Erosion(float[,] heightMap, ThermalParams thermalParam)
    {
        this.heightMap = heightMap;
        this.thermalParam = thermalParam;
    }

    public void Thermal()
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        for (int y = 3 ; y < height - 3; ++y)
        {
            for(int x = 3; x < width - 3; ++x)
            {
                List<Vector2Int> neighbours = Math.GenerateNeighbours(x, y,
                                                                      width, height);
                foreach(Vector2Int neighbour in neighbours)
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
}
