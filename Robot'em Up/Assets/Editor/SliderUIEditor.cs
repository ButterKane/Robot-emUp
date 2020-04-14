using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SliderUI))]
public class SliderUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SliderUI myScript = (SliderUI)target;

        if(GUILayout.Button("Assign min and max values to their texts"))
        {
            myScript.AttributeMinMaxValues();
        }
    }
}
