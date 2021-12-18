using UnityEngine;
using System;

public class UpdatableScriptableObject : ScriptableObject
{
    public event Action onChange;
    public bool autoUpdate;

    public void Update()
    {
        onChange?.Invoke();
    }
}
