using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SettingsMenuOrganizer))]
public class SettingsMenuOrganizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SettingsMenuOrganizer myScript = (SettingsMenuOrganizer)target;
        if (GUILayout.Button("Get Quarantinable Settings"))
        {
            myScript.GetQuarantinableSettings();
        }

        if (GUILayout.Button("Organize Settings"))
        {
            myScript.OrganizeEverything();
        }
    }
}
