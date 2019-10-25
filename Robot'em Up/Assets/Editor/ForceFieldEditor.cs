using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PuzzleForceField))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PuzzleForceField myScript = (PuzzleForceField)target;
        if (GUILayout.Button("Refresh Material"))
        {
            myScript.ChangeState(myScript.isActivated, myScript.alsoBlockPlayer);
        }
    }
}