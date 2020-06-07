using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (SettingsMenu))]
public class SettingMenuEditor : Editor
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

        SettingsMenu myScript = (SettingsMenu)target;
        if (GUILayout.Button("Compute settings values"))
        {
            myScript.ComputeSettings();
        }

        if (GUILayout.Button("Display all saved settings values"))
        {
            myScript.DisplaySettingsValues();
        }

        //if (GUILayout.Button("Assign saved settings values"))
        //{
        //    myScript.AssignSavedValuesInSettings();
        //}

        if (GUILayout.Button("Link Settings values to Game"))
        {
            myScript.ModifyPlayerPrefsValues();
        }

    }
}
