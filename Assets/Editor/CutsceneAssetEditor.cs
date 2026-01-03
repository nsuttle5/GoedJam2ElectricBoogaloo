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

            // Header line for the element (shot title)
            float headerH = EditorGUIUtility.singleLineHeight + 4f;

            // Draw the struct normally underneath
            float bodyH = EditorGUI.GetPropertyHeight(element, includeChildren: true);

            // Add padding so next element never overlaps
            return headerH + bodyH + 10f;
        };

        _shotsList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = _shots.GetArrayElementAtIndex(index);

            // inset
            rect.x += 6f;
            rect.width -= 12f;
            rect.y += 2f;

            // Grab name + duration
            string shotName = GetString(element, "name");
            float dur = GetFloat(element, "duration");
            float end = GetCumulativeEndTime(index);

            if (string.IsNullOrWhiteSpace(shotName))
                shotName = $"Shot {index + 1}";

            // 1) Title line
            var titleRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(titleRect, $"{shotName}   |   End: {end:0.###}s   |   Dur: {dur:0.###}s", EditorStyles.boldLabel);

            // 2) Default struct drawing below the title
            float bodyY = titleRect.yMax + 4f;
            float bodyH = EditorGUI.GetPropertyHeight(element, includeChildren: true);
            var bodyRect = new Rect(rect.x, bodyY, rect.width, bodyH);

            EditorGUI.PropertyField(bodyRect, element, GUIContent.none, includeChildren: true);
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
        return GetFloat(element, "duration");
    }

    private float GetCumulativeEndTime(int index)
    {
        float sum = 0f;
        for (int i = 0; i <= index && i < _shots.arraySize; i++)
            sum += GetDuration(i);
        return sum;
    }

    private static float GetFloat(SerializedProperty element, string fieldName)
    {
        var p = element.FindPropertyRelative(fieldName);
        return p != null ? Mathf.Max(0f, p.floatValue) : 0f;
    }

    private static string GetString(SerializedProperty element, string fieldName)
    {
        var p = element.FindPropertyRelative(fieldName);
        return p != null ? p.stringValue : "";
    }
}
