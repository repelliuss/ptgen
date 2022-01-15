using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour {
    public Text fps;
    public float measureInterval = 0.1f;

    float timePassed = 0f;
    int accumulatedFrames = 0;
    int currentFPS;
    const string format = "{0} FPS";

    float min, max, avg;

    void Start()
    {
        avg = 0;
        min = float.MaxValue;
        max = float.MinValue;
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        ++accumulatedFrames;
        if(timePassed >= measureInterval)
        {
            currentFPS = (int)(accumulatedFrames / measureInterval);
            if(min > currentFPS) min = currentFPS;
            else if(max < currentFPS) max = currentFPS;
            avg += currentFPS;
            avg /= 2f;
            fps.text = string.Format(format, currentFPS);
            accumulatedFrames = 0;
            timePassed = 0f;
        }
    }

    public void ShowDebug()
    {
        Debug.Log("Min: " + min);
        Debug.Log("Max: " + max);
        Debug.Log("Avg: " + avg);
    }
}
