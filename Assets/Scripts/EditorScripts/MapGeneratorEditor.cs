using UnityEngine;
using UnityEditor;
using TerrainGeneration;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var terrainMapGenerator = (MapGenerator)target;
        bool valueChanged = DrawDefaultInspector();
        bool buttonClicked = GUILayout.Button("Generate mesh preview");

        if ((terrainMapGenerator.AutoUpdatePreview && valueChanged) || buttonClicked)
        {
            terrainMapGenerator.GeneratePreview();
        }
    }
}
