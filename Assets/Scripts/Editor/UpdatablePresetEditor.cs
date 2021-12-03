using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatablePreset), true)]
public class UpdatablePresetEditor : Editor {

    public override void OnInspectorGUI()
    {
        UpdatablePreset data = (UpdatablePreset) target;

        if(DrawDefaultInspector())
        {
            if(data.autoUpdate)
            {
                data.UpdatePreset();
            }
        }

        if(GUILayout.Button("Update"))
        {
            data.UpdatePreset();
        }
    }
}
