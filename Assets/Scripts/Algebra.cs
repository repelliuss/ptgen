using UnityEngine;

public class Algebra {
    public static float LinearDistance(float x, float y)
    {
        return Mathf.Abs(x-y);
    }

    public static Vector3 SlideInPlane(Vector3 vec, float delta)
    {
        return new Vector3(vec.x + delta, vec.y, vec.z + delta);
    }
}
