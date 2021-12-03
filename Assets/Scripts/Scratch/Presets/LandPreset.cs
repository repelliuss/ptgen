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

    public float GetMinHeight()
    {
        return scale * height * heightCurve.Evaluate(0);
    }

    public float GetMaxHeight()
    {
        return scale * height * heightCurve.Evaluate(1);
    }
}
