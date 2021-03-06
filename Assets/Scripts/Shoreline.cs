using UnityEngine;
using System;
using System.Collections.Generic;

public class Shoreline
{

    float[,] heightMap;
    HeightMapNeighboursData neighbourData;
    int width;
    int height;
    float halfWidth;
    float halfHeight;
    float waterLevel;
    float uniformScale;
    Transform parent;
    Vector2 center;

    GameObject shoreLine;
    Material shoreLineMaterial;

    List<Action> readyQuads;
    List<MeshFilter> quadFilters;
    float foamScale;

    public Shoreline(float[,] heightMap, HeightMapNeighboursData ndata,
                     float uniformScale,
                     float waterLevel,
                     Material shoreLineMaterial,
                     Vector2 center, Transform parent,
                     GameObject shoreLine,
                     float foamScale)
    {
        this.heightMap = heightMap;
        this.neighbourData = ndata;
        this.width = heightMap.GetLength(0);
        this.height = heightMap.GetLength(1);
        this.halfWidth = width / 2f;
        this.halfHeight = height / 2f;
        this.waterLevel = waterLevel;
        this.parent = parent;
        this.center = center / uniformScale;
        this.shoreLine = shoreLine;
        this.shoreLineMaterial = shoreLineMaterial;
        this.quadFilters = new List<MeshFilter>();
        this.readyQuads = new List<Action>();
        this.uniformScale = uniformScale;
        this.foamScale = foamScale;
    }

    public void Destroy()
    {
        GameObject.Destroy(shoreLine);
    }

    public void AddQuad(Vector3 position, Vector3 rotateTo)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadFilters.Add(quad.GetComponent<MeshFilter>());
        quad.transform.localScale = Vector3.one * 5.0f * uniformScale;
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
                    foreach (Vector2Int n in neighbourData.neighbours[x, y])
                    {
                        if (heightMap[n.x, n.y] > waterLevel)
                        {
                            float effectiveX = (x - halfWidth) + center.x;
                            float effectiveY = (y - halfHeight) + center.y;
                            Vector3 position = new Vector3(effectiveX, waterLevel, effectiveY) * uniformScale;
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
}
