using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SettingsMenuOrganizer))]
public class SettingsMenuOrganizerEditor : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
