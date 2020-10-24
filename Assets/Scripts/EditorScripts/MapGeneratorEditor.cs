using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var noiseMapGenerator = (MapGenerator)target;
        bool valueChanged = DrawDefaultInspector();
        bool buttonClicked = GUILayout.Button("Generate map");

        if ((noiseMapGenerator.AutoUpdate && valueChanged) || buttonClicked)
        {
            noiseMapGenerator.Generate();
        }
    }
}
