using UnityEngine;
using System.Collections.Generic;

public static class Math {
    public static Vector3 QuadLerp(Vector3 a, Vector3 b,
                                   Vector3 c, Vector3 d, float u, float v)
    {
        Vector3 abu = Vector3.Lerp(a, b, u);
        Vector3 dcu = Vector3.Lerp(d, c, u);
        return Vector3.Lerp(abu, dcu, v);
    }

    public static float InterpolateHeight(int x, int y, float offsetX,
                                          float offsetY, float[,] heightMap)
    {
        int floorOffsetX = Mathf.FloorToInt(offsetX);
        int floorOffsetY = Mathf.FloorToInt(offsetY);

        float u = offsetX - floorOffsetX;
        float v = offsetY - floorOffsetY;

        int anchorX = x + floorOffsetX;
        int anchorY = y + floorOffsetY;

        Vector3 a = new Vector3(anchorX, heightMap[anchorX, anchorY], anchorY);
        Vector3 b = new Vector3(anchorX + 1, heightMap[anchorX + 1, anchorY], anchorY);
        Vector3 c = new Vector3(anchorX + 1, heightMap[anchorX + 1, anchorY + 1], anchorY + 1);
        Vector3 d = new Vector3(anchorX, heightMap[anchorX, anchorY + 1], anchorY + 1);

        return QuadLerp(a, b, c, d, u, v).y;
    }

    public static float GetSteepness(float[,] heightmap,
                                     float curHeight, float maxHeight,
                                     int x, int y, int width, int height)
    {
        float h = curHeight / maxHeight;
        int nx = x + 1;
        int ny = y + 1;

        float dx = heightmap[nx, y] / maxHeight;
        float dy = heightmap[x, ny] / maxHeight;

        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    static void AddNeighbour(List<Vector2Int> neighbours, int x, int y)
    {
        neighbours.Add(new Vector2Int(x, y));
    }

    public static List<Vector2Int> GenerateNeighbours(int x, int y, int width, int height)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        if (x != 0)
        {
            if (y != 0) AddNeighbour(neighbours, x - 1, y - 1);
            AddNeighbour(neighbours, x - 1, y);
            if (y < height - 1) AddNeighbour(neighbours, x - 1, y + 1);
        }

        if (y != 0) AddNeighbour(neighbours, x, y - 1);
        if (y < height - 1) AddNeighbour(neighbours, x, y + 1);

        if (x < width - 1)
        {
            if (y != 0) AddNeighbour(neighbours, x + 1, y - 1);
            AddNeighbour(neighbours, x + 1, y);
            if (y < height - 1) AddNeighbour(neighbours, x + 1, y + 1);
        }

        return neighbours;
    }
}
