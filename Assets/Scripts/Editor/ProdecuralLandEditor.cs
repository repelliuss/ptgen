using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProdecuralLand))]
public class ProdecuralLandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProdecuralLand land = (ProdecuralLand)target;

        if (DrawDefaultInspector())
        {
            if (land.autoUpdate)
            {
                land.Generate();
            }
        };

        if (GUILayout.Button("Generate"))
        {
            land.Generate();
        }
    }
}
