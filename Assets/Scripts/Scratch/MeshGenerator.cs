using UnityEngine;

public static class MeshGenerator
{
    static bool isOuterVertex(int x, int y, int outerSize)
    {
        return x == 0 || y == 0 || x == outerSize - 1 || y == outerSize - 1;
    }
    public static MeshData GenerateFromHeightMap(float[,] heightMap,
                                                 AnimationCurve heightCurve,
                                                 int levelOfDetail,
                                                 float maxHeight = 1)
    {
        int meshIncrement = Mathf.RoundToInt(Mathf.Pow(2, levelOfDetail));

        int outerMeshSize = heightMap.GetLength(0);
        int innerMeshSize = outerMeshSize - 2 * meshIncrement;
        int unsimplifiedInnerMeshSize = outerMeshSize - 2;

        float halfWidth = (unsimplifiedInnerMeshSize - 1) / -2f;
        float halfHeight = (unsimplifiedInnerMeshSize - 1) / 2f;

        int verticesPerLine = (innerMeshSize - 1) / meshIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        AnimationCurve copyHeightCurve = new AnimationCurve(heightCurve.keys); // For threading

        var vertexIndicesMap = new int[outerMeshSize, outerMeshSize];
        int outerMeshVertexIndex = -1;
        int innerMeshVertexIndex = 0;

        for (int y = 0; y < outerMeshSize; y += meshIncrement)
        {
            for (int x = 0; x < outerMeshSize; x += meshIncrement)
            {
                vertexIndicesMap[x, y] = isOuterVertex(x, y, outerMeshSize) ?
                    outerMeshVertexIndex-- :
                    innerMeshVertexIndex++;
            }
        }

        for (int y = 0; y < outerMeshSize; y += meshIncrement)
        {
            for (int x = 0; x < outerMeshSize; x += meshIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 normalized = new Vector2((x - meshIncrement) / (float)innerMeshSize, (y - meshIncrement) / (float)innerMeshSize);
                float height = copyHeightCurve.Evaluate(heightMap[x, y]) * maxHeight;
                Vector3 pos = new Vector3(halfWidth + (normalized.x * unsimplifiedInnerMeshSize),
                                          height,
                                          halfHeight - (normalized.y * unsimplifiedInnerMeshSize));

                meshData.AddVertex(pos, normalized, vertexIndex);

                //REVIEW: split this into 3 for loops and remove check
                if (x < outerMeshSize - 1 && y < outerMeshSize - 1)
                {
                    int squareTopLeftIndex = vertexIndicesMap[x, y];
                    int squareTopRightIndex = vertexIndicesMap[x + meshIncrement, y];
                    int squareBottomLeftIndex = vertexIndicesMap[x, y + meshIncrement];
                    int squareBottomRightIndex = vertexIndicesMap[x + meshIncrement, y + meshIncrement];

                    //REVIEW: test with your mind
                    meshData.AddTriangle(squareTopLeftIndex, squareBottomRightIndex, squareBottomLeftIndex);
                    meshData.AddTriangle(squareBottomRightIndex, squareTopLeftIndex, squareTopRightIndex);
                }
            }
        }

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;

    Vector3[] outerVertices;
    int[] outerTriangles;

    int triangleIndex = 0;
    int outerTriangleIndex = 0;

    public MeshData(int squareLength)
    {
        vertices = new Vector3[squareLength * squareLength];
        triangles = new int[(squareLength - 1) * (squareLength - 1) * 6];
        uv = new Vector2[squareLength * squareLength];

        outerVertices = new Vector3[squareLength * 4 + 4];
        outerTriangles = new int[squareLength * 4 * 6];
    }

    public void AddVertex(Vector3 pos, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            outerVertices[-vertexIndex - 1] = pos;
            return;
        }

        vertices[vertexIndex] = pos;
        this.uv[vertexIndex] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            outerTriangles[outerTriangleIndex] = a;
            outerTriangles[outerTriangleIndex + 1] = b;
            outerTriangles[outerTriangleIndex + 2] = c;
            outerTriangleIndex += 3;
            return;
        }

        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    Vector3 SurfaceNormalFromIndices(int a, int b, int c)
    {
        Vector3 pointA = a < 0 ? outerVertices[-a-1] : vertices[a];
        Vector3 pointB = b < 0 ? outerVertices[-b-1] : vertices[b];
        Vector3 pointC = c < 0 ? outerVertices[-c-1] : vertices[c];

        Vector3 edgeAB = pointB - pointA;
        Vector3 edgeAC = pointC - pointA;

        return Vector3.Cross(edgeAB, edgeAC).normalized;
    }

    Vector3[] CalculateNormals()
    {
        var vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; ++i)
        {
            int triangleIndex = i * 3;
            int vertexIndexA = triangles[triangleIndex];
            int vertexIndexB = triangles[triangleIndex + 1];
            int vertexIndexC = triangles[triangleIndex + 2];

            var normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += normal;
            vertexNormals[vertexIndexB] += normal;
            vertexNormals[vertexIndexC] += normal;
        }

        int outerTriangleCount = outerTriangles.Length / 3;
        for (int i = 0; i < outerTriangleCount; ++i)
        {
            int triangleIndex = i * 3;
            int vertexIndexA = outerTriangles[triangleIndex];
            int vertexIndexB = outerTriangles[triangleIndex + 1];
            int vertexIndexC = outerTriangles[triangleIndex + 2];

            var normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            if (vertexIndexA >= 0)
                vertexNormals[vertexIndexA] += normal;

            if (vertexIndexB >= 0)
                vertexNormals[vertexIndexB] += normal;

            if (vertexIndexC >= 0)
                vertexNormals[vertexIndexC] += normal;
        }

        for (int i = 0; i < vertexNormals.Length; ++i)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    public Mesh MakeMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = CalculateNormals();

        return mesh;
    }

}
