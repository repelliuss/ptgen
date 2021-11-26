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

    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiPeaks;
    SerializedProperty voronoiType;

    SerializedProperty MPDHeightMin;
    SerializedProperty MPDHeightMax;
    SerializedProperty MPDHeightDampenerPower;
    SerializedProperty MPDRoughness;

    SerializedProperty smoothAmount;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    GUITableState vegMapTable;
    SerializedProperty vegetation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;

    GUITableState detailMapTable;
    SerializedProperty detail;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;

    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty shoreLineMaterial;

    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMPD = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showHeights = false;
    bool showVeg = false;
    bool showDetail = false;
    bool showWater = false;

    Texture2D hmTexture;

    Vector2 scrollPos;

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

        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiPeaks = serializedObject.FindProperty("voronoiPeaks");
        voronoiType = serializedObject.FindProperty("voronoiType");

        MPDHeightMin = serializedObject.FindProperty("MPDheightMin");
        MPDHeightMax = serializedObject.FindProperty("MPDheightMax");
        MPDHeightDampenerPower = serializedObject.FindProperty("MPDheightDampenerPower");
        MPDRoughness = serializedObject.FindProperty("MPDroughness");

        smoothAmount = serializedObject.FindProperty("smoothAmount");

        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");

        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);

        vegMapTable = new GUITableState("vegMapTable");
        vegetation = serializedObject.FindProperty("vegetation");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");

        detailMapTable = new GUITableState("detailMapTable");
        detail = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");

        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreLineMaterial = serializedObject.FindProperty("shoreLineMaterial");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;

        Rect r = EditorGUILayout.BeginVertical();
        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

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

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.IntSlider(voronoiPeaks, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);
            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            EditorGUILayout.PropertyField(MPDHeightMin, new GUIContent("Height min"));
            EditorGUILayout.PropertyField(MPDHeightMax, new GUIContent("Height max"));
            EditorGUILayout.PropertyField(MPDHeightDampenerPower, new GUIContent("Dampener Power"));
            EditorGUILayout.PropertyField(MPDRoughness, new GUIContent("Roughness"));
            if (GUILayout.Button("Apply"))
            {
                terrain.MidPointDisplacement();
            }
        }

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            /*EditorGUILayout.Slider(splatOffset, 0, 0.1f, new GUIContent("Offset"));
            EditorGUILayout.Slider(splatNoiseXScale, 0.001f, 1, new GUIContent("Noise X Scale"));
            EditorGUILayout.Slider(splatNoiseYScale, 0.001f, 1, new GUIContent("Noise Y Scale"));
            EditorGUILayout.Slider(splatNoiseScaler, 0, 1, new GUIContent("Noise Scaler"));*/
            splatMapTable = GUITableLayout.DrawTable(splatMapTable,
                                            serializedObject.FindProperty("splatHeights"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply SplatMaps"))
            {
                terrain.SplatMaps();
            }
        }

        showVeg = EditorGUILayout.Foldout(showVeg, "Vegetation");
        if (showVeg)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Trees Spacing"));
            vegMapTable = GUITableLayout.DrawTable(vegMapTable,
                                        serializedObject.FindProperty("vegetation"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewVegetation();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Vegetation"))
            {
                terrain.PlantVegetation();
            }
        }

        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if (showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);

            if (GUILayout.Button("Add Water"))
            {
                terrain.AddWater();
            }

            EditorGUILayout.PropertyField(shoreLineMaterial);
            if (GUILayout.Button("Add Shoreline"))
            {
                terrain.DrawShoreline();
            }
        }

        showDetail = EditorGUILayout.Foldout(showDetail, "Details");
        if (showDetail) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Detail", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Maximum Details"));
            EditorGUILayout.IntSlider(detailSpacing, 1, 20, new GUIContent("Detail Spacing"));
            detailMapTable = GUITableLayout.DrawTable(detailMapTable,
                serializedObject.FindProperty("details"));

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+")) {
                terrain.AddNewDetails();
            }
            if (GUILayout.Button("-")) {
                terrain.RemoveDetails();
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Details")) {
                terrain.AddDetails();
            }
        }

        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth Terrain");
        if(showSmooth)
        {
            EditorGUILayout.IntSlider(smoothAmount, 1, 10, new GUIContent("smoothAmount"));
            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }

        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Flatten"))
        {
            terrain.FlattenTerrain();
        }

        showHeights = EditorGUILayout.Foldout(showHeights, "Height Map");
        if (showHeights)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0,
                                                                    terrain.terrainData.heightmapResolution,
                                                                    terrain.terrainData.heightmapResolution);
                for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
                {
                    for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
                    {
                        hmTexture.SetPixel(x, y, new Color(heightMap[x, y],
                                                           heightMap[x, y],
                                                           heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
