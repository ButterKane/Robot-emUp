using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(InputHandler))]
#if UNITY_EDITOR
public class InputHandlerEditor : Editor
{
    InputHandler parent;

    private void OnEnable()
    {
        parent = (InputHandler)target;
    }

    private void GetBindings()
    {

    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();


        GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
        i_headerStyle.alignment = TextAnchor.MiddleCenter;
        i_headerStyle.fontSize = 20;
        i_headerStyle.fontStyle = FontStyle.Bold;

        GUIStyle i_buttonStyle = new GUIStyle(EditorStyles.miniButton);
        i_buttonStyle.alignment = TextAnchor.MiddleCenter;
        i_buttonStyle.fontSize = 10;
        i_buttonStyle.fontStyle = FontStyle.Normal;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUI.color = Color.gray;
            GUILayout.Box("Mapped Inputs", i_headerStyle);
            GUILayout.Space(10);
            GUI.color = Color.white;
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_action = serializedObject.FindProperty("parent.bindingP1.dunk");
                EditorGUILayout.PropertyField(m_action, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_action = serializedObject.FindProperty("parent.bindingP1.throwBall");
                EditorGUILayout.PropertyField(m_action, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_action = serializedObject.FindProperty("parent.bindingP1.interact");
                EditorGUILayout.PropertyField(m_action, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_action = serializedObject.FindProperty("parent.bindingP1.grapple");
                EditorGUILayout.PropertyField(m_action, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_action = serializedObject.FindProperty("parent.bindingP1.detectBall");
                EditorGUILayout.PropertyField(m_action, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();
            //for (int i = 0; i < m_p1Bindings.arraySize; i++)
            //{
            //    GUILayout.Box("Event N°" + i + ": ", i_buttonStyle);
            //}
            //foreach (var item in typeof(KeyBindingStruct).GetFields(System.Reflection.BindingFlags.Public))
            //{

            //}




        }
        EditorGUILayout.EndVertical();

        DrawDefaultInspector();
    }
}
#endif

