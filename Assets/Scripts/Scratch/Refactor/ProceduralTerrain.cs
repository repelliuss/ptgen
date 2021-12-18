using UnityEngine;

public class ProceduralTerrain : MonoBehaviour
{
    public GradientHeightMapParams param;

    void Start()
    {
        var mapMaker = new GradientHeightMapMaker(param);
        var map = mapMaker.Make();
    }
}
