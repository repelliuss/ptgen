using UnityEngine;
using System;
using System.Collections.Generic;

public class Shoreline
{

    float[,] heightMap;
    int width;
    int height;
    float halfWidth;
    float halfHeight;
    float waterLevel;
    Transform parent;
    Vector2 center;

    GameObject shoreLine;
    Material shoreLineMaterial;

    List<Action> readyQuads;

    List<MeshFilter> quadFilters;

    public Shoreline(float[,] heightMap, float waterLevel,
                     Material shoreLineMaterial,
                     Vector2 center, Transform parent,
                     GameObject shoreLine)
    {
        this.heightMap = heightMap;
        this.width = heightMap.GetLength(0);
        this.height = heightMap.GetLength(1);
        this.halfWidth = width / 2f;
        this.halfHeight = height / 2f;
        this.waterLevel = waterLevel;
        this.parent = parent;
        this.center = center;
        this.shoreLine = shoreLine;
        this.shoreLineMaterial = shoreLineMaterial;
        this.quadFilters = new List<MeshFilter>();
        this.readyQuads = new List<Action>();
    }

    public void Destroy()
    {
        GameObject.Destroy(shoreLine);
    }

    public void AddQuad(Vector3 position, Vector3 rotateTo)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadFilters.Add(quad.GetComponent<MeshFilter>());
        quad.transform.localScale = Vector3.one * 5.0f;
        quad.transform.parent = shoreLine.transform;
        quad.transform.position = position;
        quad.transform.LookAt(rotateTo);
        quad.transform.Rotate(90, 0, 0);
    }

    public void PlantQuads()
    {
        foreach(Action action in readyQuads)
        {
            action();
        }
    }

    public void BakeQuads()
    {
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if (heightMap[x, y] < waterLevel)
                {
                    List<Vector2Int> neighbours = GenerateNeighbours(x, y);

                    foreach (Vector2Int n in neighbours)
                    {
                        if (heightMap[n.x, n.y] > waterLevel)
                        {
                            float effectiveX = (x - halfWidth) + center.x;
                            float effectiveY = (y - halfHeight) + center.y;
                            Vector3 position = new Vector3(effectiveX, waterLevel, effectiveY);
                            Vector3 rotateTo = new Vector3((n.y - halfWidth) + center.x,
                                                           waterLevel,
                                                           (n.y - halfHeight) + center.y);
                            readyQuads.Add(() => AddQuad(position, rotateTo));
                        }
                    }
                }
            }
        }
    }

    public void PlantShoreLine()
    {
        CombineInstance[] combine = new CombineInstance[quadFilters.Count];
        for(int i = 0; i < combine.Length; ++i)
        {
            combine[i].mesh = quadFilters[i].sharedMesh;
            combine[i].transform = quadFilters[i].transform.localToWorldMatrix;
            quadFilters[i].gameObject.SetActive(false);
        }

        shoreLine.AddComponent<WaveAnimation>();
        MeshFilter shoreLineFilter = shoreLine.AddComponent<MeshFilter>();
        shoreLineFilter.mesh = new Mesh();
        shoreLineFilter.sharedMesh.CombineMeshes(combine);

        MeshRenderer renderer = shoreLine.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = shoreLineMaterial;
    }

    public void DestroyQuads()
    {
        foreach(MeshFilter m in quadFilters)
        {
            GameObject.Destroy(m.gameObject);
        }
    }

    public void DestroyQuadsImmediate()
    {
        foreach(MeshFilter m in quadFilters)
        {
            GameObject.DestroyImmediate(m.gameObject);
        }
    }

    public void SetActive(bool activity)
    {
        if(shoreLine != null) shoreLine.SetActive(activity);
    }

    void AddNeighbour(List<Vector2Int> neighbours, int x, int y)
    {
        neighbours.Add(new Vector2Int(x, y));
    }

    List<Vector2Int> GenerateNeighbours(int x, int y)
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
