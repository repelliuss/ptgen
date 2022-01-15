using UnityEngine;

[CreateAssetMenu()]
public class WaterParams : UpdatableScriptableObject
{
    public GameObject gobject;
    public float waterLevel = 3f;
    public Vector2 waterTileSize;
    public Material material;
    public bool makeShoreline = true;
}
