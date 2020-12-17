using UnityEngine;
using UnityEditor;
using TerrainGeneration;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var mapGenerator = (MapGenerator)target;
        bool valueChanged = DrawDefaultInspector();
        bool buttonClicked = GUILayout.Button("Generate preview");

        if (!Application.isPlaying && ((mapGenerator.autoUpdatePreview && valueChanged) || buttonClicked))
        {
            mapGenerator.GeneratePreview();
        }
    }
}