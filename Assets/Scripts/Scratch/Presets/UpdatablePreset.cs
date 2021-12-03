using UnityEngine;
using System;

public class UpdatablePreset : ScriptableObject
{
    public event Action OnValueChange;
    public bool autoUpdate;

    public void UpdatePreset()
    {
        OnValueChange?.Invoke();
    }
}
