using UnityEngine;

public static class MeshGenerator

{
    public static MeshData GenerateFromHeightMap(float[,] heightMap,
                                                 AnimationCurve heightCurve,
                                                 float maxHeight = 1)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        MeshData meshData = new MeshData(width, height);

        float halfWidth = (width - 1) / 2f;
        float halfHeight = (height - 1) / 2f;

        int vertexIndex = 0;
        for (int y = 0; y < height - 1; ++y)
        {
            for (int x = 0; x < width - 1; ++x)
            {
                meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                             heightCurve.Evaluate(heightMap[x, y]) * maxHeight,
                                                             halfHeight - y);
                meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                ++vertexIndex;
            }

            ++vertexIndex;
        }

        vertexIndex = width - 1;
        for (int y = 0; y < height; ++y)
        {
            for (int x = width - 1; x < width; ++x)
            {
                meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                             heightCurve.Evaluate(heightMap[x, y]) * maxHeight,
                                                             halfHeight - y);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                vertexIndex += width;
            }
        }

        vertexIndex = width * (height - 1);
        for (int x = 0; x < width - 1; ++x)
        {
            meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                         heightCurve.Evaluate(heightMap[x, (height - 1)]) * maxHeight,
                                                         halfHeight- (height - 1));
            meshData.uv[vertexIndex] = new Vector2(x / (float)width, (height - 1) / (float)height);
            ++vertexIndex;
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    int triangleIndex = 0;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width-1)*(height-1) * 6];
        uv = new Vector2[width * height];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh MakeMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        return mesh;
    }
}
