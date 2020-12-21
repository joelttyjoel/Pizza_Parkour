using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovementScript))]
public class MovementEditor : Editor
{
    SerializedProperty[] airAttributes;
    SerializedProperty[] groundAttributes;
    SerializedProperty[] jumpAttributes;

    Dictionary<string, SerializedProperty[]> attributes = new Dictionary<string, SerializedProperty[]>();
    bool[] notFolded;

    void OnEnable()
    {
        groundAttributes = new SerializedProperty[]
        {
            serializedObject.FindProperty("maxSpeed"),
            serializedObject.FindProperty("timeToMaxAccel"),
            serializedObject.FindProperty("maxAcceleration"),
            serializedObject.FindProperty("accelCurve"),

        };
        airAttributes = new SerializedProperty[]
        {
            serializedObject.FindProperty("airVelocity"),
            serializedObject.FindProperty("maxAirSpeed"),
            serializedObject.FindProperty("maxFallSpeed"),
            serializedObject.FindProperty("catchHeight"),
            serializedObject.FindProperty("bumpedHeadWidth"),
        };
        jumpAttributes = new SerializedProperty[]
        {
            serializedObject.FindProperty("jumpVelocity"),
            serializedObject.FindProperty("airJumpVelocity"),
            serializedObject.FindProperty("shortJumpMultiplier"),
            serializedObject.FindProperty("shortAirJumpMultiplier"),
            serializedObject.FindProperty("fallMultiplier"),
            serializedObject.FindProperty("coyoteTime"),
            serializedObject.FindProperty("jumpBufferTime"),
            serializedObject.FindProperty("apexHeight"),
            serializedObject.FindProperty("apexSpeed"),
            serializedObject.FindProperty("antiGravityApexMagnitude"),
            //serializedObject.FindProperty("stickyFeetFriction"),

        };

        attributes.Add("Ground Movement", groundAttributes);
        attributes.Add("Air Movement", airAttributes);
        attributes.Add("Jumping", jumpAttributes);

        notFolded = new bool[attributes.Count];
        for (int i = 0; i < notFolded.Length; i++)
            notFolded[i] = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        int i = 0;
        foreach (var key in attributes.Keys)
        {
            notFolded[i] = EditorGUILayout.Foldout(notFolded[i], key, true, EditorStyles.foldoutHeader);
            EditorGUI.indentLevel++;
            if (notFolded[i])
                foreach (var attribute in attributes[key])
                    EditorGUILayout.PropertyField(attribute, true);
            EditorGUILayout.Space(0, true);
            EditorGUI.indentLevel--;
            i++;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
