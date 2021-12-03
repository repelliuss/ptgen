using UnityEngine;

[CreateAssetMenu()]
public class LandPreset : UpdatablePreset
{
    [Min(1)]
    public float height = 64;

    //BUG: doesn't work
    [Min(1)]
    public float scale;

    public AnimationCurve heightCurve;

    public bool useFalloff;
}
