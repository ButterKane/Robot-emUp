using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// IngredientDrawer
[CustomPropertyDrawer(typeof(BothInputs))]
public class BothInputsDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var InputNameRect = new Rect(position.x, position.y, 150, position.height);
        var P1InputRect = new Rect(position.x + InputNameRect.width + 5, position.y, (position.width - InputNameRect.width)/2, position.height);
        var P2InputRect = new Rect(P1InputRect.x + P1InputRect.width, position.y, (position.width - InputNameRect.width) / 2, position.height);
        

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(InputNameRect, property.FindPropertyRelative("InputName"), GUIContent.none);
        EditorGUI.PropertyField(P1InputRect, property.FindPropertyRelative("P1Input"), GUIContent.none);
        EditorGUI.PropertyField(P2InputRect, property.FindPropertyRelative("P2Input"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
