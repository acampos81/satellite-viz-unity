using UnityEngine;
using UnityEditor;
using EphemerisDemo.Sim;

[CustomEditor(typeof(EarthObject))]
public class EarthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Draw Earth Lines"))
        {
            ((EarthObject)serializedObject.targetObject).DrawEarthLines();
        }
    }
}
