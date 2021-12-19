using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableScriptableObject), true)]
public class UpdatableScriptableObjectEditor : Editor {

    public override void OnInspectorGUI()
    {
        UpdatableScriptableObject data = (UpdatableScriptableObject) target;

        if(DrawDefaultInspector())
        {
            if(data.autoUpdate)
            {
                data.Update();
            }
        }

        if(GUILayout.Button("Update"))
        {
            data.Update();
        }
    }
}
