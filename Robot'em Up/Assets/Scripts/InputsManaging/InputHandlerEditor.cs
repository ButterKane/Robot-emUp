using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(InputHandler))]
public class InputHandlerEditor : Editor
{
    InputHandler parent;

    private void OnEnable()
    {
        parent = (InputHandler)target;
    }

    public override void OnInspectorGUI()
    {
        foreach (var item in typeof(KeyBindingStruct).GetFields(System.Reflection.BindingFlags.Public))
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUI.color = Color.gray;
                    GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
                    i_headerStyle.alignment = TextAnchor.UpperLeft;
                    i_headerStyle.fontSize = 20;
                    i_headerStyle.fontStyle = FontStyle.Bold;
                    GUILayout.Box("Bidouille", i_headerStyle);
                    GUILayout.Space(10);
                    GUI.color = Color.white;
                    SerializedProperty m_button = serializedObject.FindProperty(item.ToString());
                    EditorGUILayout.PropertyField(m_button, new GUIContent("Bdouille ?"));
                }
            }
        }
    }
}

