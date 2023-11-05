using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty levelId = serializedObject.FindProperty("LevelId");
        SerializedProperty bottleCount = serializedObject.FindProperty("BottleCount");
        SerializedProperty bottlePosition = serializedObject.FindProperty("BottlePosition");
        SerializedProperty colorCount = serializedObject.FindProperty("colorCount");

        EditorGUILayout.PropertyField(levelId);
        EditorGUILayout.PropertyField(bottleCount);

        // Display the BottlePosition array
        EditorGUILayout.PropertyField(bottlePosition, true);

        // Ensure that BottlePosition array size always matches BottleCount
        if (bottlePosition.arraySize != bottleCount.intValue)
        {
            bottlePosition.arraySize = bottleCount.intValue;
        }

        // Use a custom slider or field for ColorCount with the constraint
        int newColorCount = EditorGUILayout.IntSlider("ColorCount", colorCount.intValue, 1, bottleCount.intValue - 1);
        colorCount.intValue = newColorCount;

        serializedObject.ApplyModifiedProperties();
    }
}
