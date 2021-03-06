using UnityEngine;

public class GameObjectMaker
{
    GameObject tree;

    public GameObjectMaker(GameObject gobject)
    {
        this.tree = gobject;
    }

    public void Make(Vector3 position, Vector3 scale,
                     int yRotation, Transform parent,
                     Color color1, Color color2, float color2bias)
    {
        GameObject instance = GameObject.Instantiate(tree);

        instance.transform.position = position;
        instance.transform.localScale = scale;
        instance.transform.Rotate(0, yRotation, 0);
        instance.transform.parent = parent;

        Renderer rend;
        if(instance.TryGetComponent<Renderer>(out rend))
        {
            SetMaterials(rend, color1, color2, color2bias);
        }
    }

    void SetMaterials(Renderer renderer, Color color1, Color color2,
                      float color2bias)
    {
        //TODO: make materials not shared
        Material[] materials = renderer.materials;

        foreach (Material mat in materials)
        {
            if(mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", Color.Lerp(color1,
                                                  color2,
                                                  color2bias + Random.Range(0f, 1f)));
            }
        }
    }
}
