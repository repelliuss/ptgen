using UnityEngine;

public class SquareMesh
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] normals;

    Vector3[] surroundingVertices;
    int[] surroundingTriangles;

    int triangleIndex = 0;
    int surroundingTriangleIndex = 0;

    public SquareMesh(int size)
    {
        vertices = new Vector3[size*size];
        triangles = new int[(size-1)*(size-1)*2*3];
        uvs = new Vector2[size*size];
        normals = new Vector3[size*size];

        surroundingVertices = new Vector3[size*4+4];
        surroundingTriangles = new int[size*4*2*3];
    }

    public Mesh Generate()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    public void SetVertex(int index, Vector3 vertex, Vector2 uv)
    {
        if(index < 0)
        {
            surroundingVertices[-index - 1] = vertex;
            return;
        }

        vertices[index] = vertex;
        uvs[index] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            surroundingTriangles[surroundingTriangleIndex++] = a;
            surroundingTriangles[surroundingTriangleIndex++] = b;
            surroundingTriangles[surroundingTriangleIndex++] = c;
            return;
        }

        triangles[triangleIndex++] = a;
        triangles[triangleIndex++] = b;
        triangles[triangleIndex++] = c;
    }

    public void BakeNormals()
    {
        int triangleCount = triangles.Length / 3;
        int triangleIndex = 0;

        for (int i = 0; i < triangleCount; ++i)
        {
            int a = triangles[triangleIndex++];
            int b = triangles[triangleIndex++];
            int c = triangles[triangleIndex++];
            Vector3 normal = SurfaceNormalFromIndices(a, b, c);

            normals[a] += normal;
            normals[b] += normal;
            normals[c] += normal;
        }

        int surroundingTriangleCount = surroundingTriangles.Length / 3;
        triangleIndex = 0;

        for (int i = 0; i < surroundingTriangleCount; ++i)
        {
            int a = surroundingTriangles[triangleIndex++];
            int b = surroundingTriangles[triangleIndex++];
            int c = surroundingTriangles[triangleIndex++];
            var normal = SurfaceNormalFromIndices(a, b, c);

            if (a >= 0)
                normals[a] += normal;

            if (b >= 0)
                normals[b] += normal;

            if (c >= 0)
                normals[c] += normal;
        }

        for (int i = 0; i < normals.Length; ++i)
        {
            normals[i].Normalize();
        }
    }

    static public SquareMesh FromHeightMap(float[,] heightMap,
                                           int lod)
    {
        int meshIncrement = MeshIncrementFromLOD(lod);
        int surroundingSize = heightMap.GetLength(0);
        int size = surroundingSize - meshIncrement * 2;
        int unsimplifiedSize = surroundingSize - 2;
        float halfSize = (unsimplifiedSize - 1) / 2f;
        int verticesPerLine = (size - 1) / meshIncrement + 1;
        SquareMesh mesh = new SquareMesh(verticesPerLine);

        int[,] vertexIndicesMap = new int[surroundingSize, surroundingSize];
        int surroundingIndex = -1;
        int index = 0;
        for (int y = 0; y < surroundingSize; y += meshIncrement)
        {
            for (int x = 0; x < surroundingSize; x += meshIncrement)
            {
                vertexIndicesMap[x, y] = IsSurroundingVertex(x, y, surroundingSize) ?
                    surroundingIndex-- :
                    index++;
            }
        }

        for (int y = 0; y < surroundingSize; y += meshIncrement)
        {
            for (int x = 0; x < surroundingSize; x += meshIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 normalized = new Vector2((x - meshIncrement) / (float)size,
                                                 (y - meshIncrement) / (float)size);
                Vector3 vertex = new Vector3((normalized.x * unsimplifiedSize) - halfSize,
                                             heightMap[x, y],
                                             (normalized.y * unsimplifiedSize) - halfSize);

                mesh.SetVertex(vertexIndex, vertex, normalized);

                if (x < surroundingSize - 1 && y < surroundingSize - 1)
                {
                    //[T]op, [B]ottom, [L]eft, [R]ight
                    int TL = vertexIndicesMap[x, y];
                    int TR = vertexIndicesMap[x + meshIncrement, y];
                    int BL = vertexIndicesMap[x, y + meshIncrement];
                    int BR = vertexIndicesMap[x + meshIncrement, y + meshIncrement];

                    //counter-clockwise
                    mesh.AddTriangle(TL, BR, TR);
                    mesh.AddTriangle(TL, BL, BR);
                }
            }
        }

        mesh.BakeNormals();

        return mesh;
    }

    Vector3 SurfaceNormalFromIndices(int a, int b, int c)
    {
        Vector3 vertexA = a < 0 ? surroundingVertices[-a-1] : vertices[a];
        Vector3 vertexB = b < 0 ? surroundingVertices[-b-1] : vertices[b];
        Vector3 vertexC = c < 0 ? surroundingVertices[-c-1] : vertices[c];

        Vector3 edgeAB = vertexB - vertexA;
        Vector3 edgeAC = vertexC - vertexA;

        return Vector3.Cross(edgeAB, edgeAC).normalized;
    }

    static int MeshIncrementFromLOD(int lod)
    {
        return Mathf.RoundToInt(Mathf.Pow(2, lod));
    }

    static bool IsSurroundingVertex(int x, int y, int size)
    {
        return x == 0 || y == 0 || x == size - 1 || y == size - 1;
    }
}
