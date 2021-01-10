using Data.ScriptableObjects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var data = (UpdatableData)target;
        bool valueChanged = DrawDefaultInspector();
        GUILayout.Space(15);
        bool buttonClicked = GUILayout.Button("Update");

        if ((data.autoUpdate && valueChanged) || buttonClicked)
        {
            data.RaiseValuesUpdatedEvent();
        }
    }
}
