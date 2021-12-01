using UnityEngine;

public static class MeshGenerator

{
    public static MeshData GenerateFromHeightMap(float[,] heightMap,
                                                 AnimationCurve heightCurve,
                                                 int levelOfDetail,
                                                 float maxHeight = 1)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        int meshIncrement = Mathf.RoundToInt(Mathf.Pow(2, levelOfDetail));
        int verticesPerLine = (width - 1) / meshIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

        float halfWidth = (width - 1) / 2f;
        float halfHeight = (height - 1) / 2f;

        AnimationCurve copyHeightCurve = new AnimationCurve(heightCurve.keys); // For threading

        int vertexIndex = 0;
        for (int y = 0; y < height - 1; y += meshIncrement)
        {
            for (int x = 0; x < width - 1; x += meshIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                             copyHeightCurve.Evaluate(heightMap[x, y]) * maxHeight,
                                                             halfHeight - y);
                meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                ++vertexIndex;
            }

            ++vertexIndex;
        }

        vertexIndex = verticesPerLine - 1;
        for (int y = 0; y < height; y += meshIncrement)
        {
            for (int x = width - 1; x < width; x += meshIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                             heightCurve.Evaluate(heightMap[x, y]) * maxHeight,
                                                             halfHeight - y);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                vertexIndex += verticesPerLine;
            }
        }

        vertexIndex = verticesPerLine * (verticesPerLine - 1);
        for (int x = 0; x < width - 1; x += meshIncrement)
        {
            meshData.vertices[vertexIndex] = new Vector3(x - halfWidth,
                                                         heightCurve.Evaluate(heightMap[x, (height - 1)]) * maxHeight,
                                                         halfHeight - (height - 1));
            meshData.uv[vertexIndex] = new Vector2(x / (float)width, (height - 1) / (float)height);
            ++vertexIndex;
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    int triangleIndex = 0;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uv = new Vector2[width * height];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
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
