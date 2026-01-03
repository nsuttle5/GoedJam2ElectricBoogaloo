using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CutsceneAsset))]
public sealed class CutsceneAssetEditor : Editor
{
    private ReorderableList _shotsList;

    private SerializedProperty _additiveScenes;
    private SerializedProperty _shots;
    private SerializedProperty _nextScene;
    private SerializedProperty _preloadDuringLast;
    private SerializedProperty _preloadLead;

    private void OnEnable()
    {
        _additiveScenes = serializedObject.FindProperty("additiveScenesToLoad");
        _shots = serializedObject.FindProperty("shots");
        _nextScene = serializedObject.FindProperty("nextSceneSingleLoad");
        _preloadDuringLast = serializedObject.FindProperty("preloadNextSceneDuringLastShot");
        _preloadLead = serializedObject.FindProperty("preloadLeadSeconds");

        _shotsList = new ReorderableList(serializedObject, _shots, true, true, true, true);

        _shotsList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Shots");
        };

        _shotsList.elementHeightCallback = index =>
        {
            var element = _shots.GetArrayElementAtIndex(index);

            // One extra line for the "End Time" label + spacing.
            float labelH = EditorGUIUtility.singleLineHeight + 4f;

            // Unity-computed height for the whole struct (includes color picker, foldouts, etc.)
            float propH = EditorGUI.GetPropertyHeight(element, includeChildren: true);

            // Bottom padding to prevent overlap with the reorder controls
            float pad = 10f;

            return labelH + propH + pad;
        };

        _shotsList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = _shots.GetArrayElementAtIndex(index);

            float duration = GetDuration(index);
            float cumulative = GetCumulativeEndTime(index);

            // Inset horizontally a bit so it doesn't hug the list border
            rect.x += 4f;
            rect.width -= 8f;

            // 1) End-time label
            var labelRect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, $"End Time: {cumulative:0.###}s   (Shot {index + 1} dur {duration:0.###}s)");

            // 2) Property block below label
            float propY = labelRect.yMax + 2f;
            var propRect = new Rect(rect.x, propY, rect.width, EditorGUI.GetPropertyHeight(element, true));
            EditorGUI.PropertyField(propRect, element, GUIContent.none, includeChildren: true);
        };

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Startup Loading", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_additiveScenes, true);

        EditorGUILayout.Space(8);
        _shotsList.DoLayoutList();

        EditorGUILayout.Space(8);
        DrawTotals();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("End Behavior", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_nextScene);
        EditorGUILayout.PropertyField(_preloadDuringLast);

        using (new EditorGUI.DisabledScope(!_preloadDuringLast.boolValue))
        {
            EditorGUILayout.PropertyField(_preloadLead);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTotals()
    {
        float total = 0f;
        for (int i = 0; i < _shots.arraySize; i++)
            total += GetDuration(i);

        EditorGUILayout.HelpBox($"Total Cutscene Length: {total:0.###} seconds", MessageType.Info);
    }

    private float GetDuration(int index)
    {
        if (index < 0 || index >= _shots.arraySize) return 0f;
        var element = _shots.GetArrayElementAtIndex(index);
        var durProp = element.FindPropertyRelative("duration");
        return durProp != null ? Mathf.Max(0f, durProp.floatValue) : 0f;
    }

    private float GetCumulativeEndTime(int index)
    {
        float sum = 0f;
        for (int i = 0; i <= index && i < _shots.arraySize; i++)
            sum += GetDuration(i);
        return sum;
    }
}
