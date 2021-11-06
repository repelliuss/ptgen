using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{

    SerializedProperty randomHeightRange;

    SerializedProperty heightMapScale;
    SerializedProperty heightMapTexture;

    SerializedProperty flattenBeforeApply;

    SerializedProperty perlinXScale;
    SerializedProperty perlinZScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinZOffset;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinLacunarity;
    SerializedProperty perlinHeightScale;

    GUITableState perlinParamsTable;
    SerializedProperty perlinParams;

    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;

    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");

        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapTexture = serializedObject.FindProperty("heightMapTexture");

        flattenBeforeApply = serializedObject.FindProperty("flattenBeforeApply");

        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinZScale = serializedObject.FindProperty("perlinZScale");
        perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        perlinZOffset = serializedObject.FindProperty("perlinZOffset");

        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinLacunarity = serializedObject.FindProperty("perlinLacunarity");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        perlinParamsTable = new GUITableState("perlinParamsTable");
        perlinParams = serializedObject.FindProperty("perlinParams");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        EditorGUILayout.PropertyField(flattenBeforeApply);

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set heights between random values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);

            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load heights from...");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapTexture);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 0.03f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinZScale, 0, 0.03f, new GUIContent("Z Scale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinZOffset, 0, 10000, new GUIContent("Z Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinLacunarity, 0.1f, 10, new GUIContent("Lacunarity"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 3, new GUIContent("Height Scale"));

            if (GUILayout.Button("Apply"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if(showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noises", EditorStyles.boldLabel);
            perlinParamsTable = GUITableLayout.DrawTable(perlinParamsTable, perlinParams);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply"))
            {
                terrain.MultiplePerlin();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Flatten"))
        {
            terrain.FlattenTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
