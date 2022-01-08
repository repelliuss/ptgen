using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour {
    public Text fps;
    public float measureInterval = 0.1f;

    float timePassed = 0f;
    int accumulatedFrames = 0;
    int currentFPS;
    const string format = "{0} FPS";

    void Update()
    {
        timePassed += Time.deltaTime;
        ++accumulatedFrames;
        if(timePassed >= measureInterval)
        {
            currentFPS = (int)(accumulatedFrames / measureInterval);
            fps.text = string.Format(format, currentFPS);
            accumulatedFrames = 0;
            timePassed = 0f;
        }
    }
}
