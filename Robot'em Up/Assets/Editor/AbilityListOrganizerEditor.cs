using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbilityListOrganizer))]
public class AbilityListOrganizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AbilityListOrganizer myScript = (AbilityListOrganizer)target;
        if (GUILayout.Button("Organize Abilities"))
        {
            myScript.OrganizeAbilities();
        }
    }
}
