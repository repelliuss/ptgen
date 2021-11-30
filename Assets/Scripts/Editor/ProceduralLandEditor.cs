using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralLand))]
public class ProceduralLandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralLand land = (ProceduralLand)target;

        if (DrawDefaultInspector())
        {
            if (land.autoUpdate)
            {
                land.DrawLand();
            }
        };

        if (GUILayout.Button("Generate"))
        {
            land.DrawLand();
        }
    }
}
