//// Assets/Editor/CutsceneShotDrawer.cs
//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(CutsceneAsset.CutsceneShot))]
//public sealed class CutsceneShotDrawer : PropertyDrawer
//{
//    const float PAD = 2f;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // Force defaults every draw so they can’t be changed / persist.
//        var unscaledTimeProp = property.FindPropertyRelative("unscaledTime");
//        if (unscaledTimeProp != null) unscaledTimeProp.boolValue = false;

//        var priorityOverrideProp = property.FindPropertyRelative("priorityOverride");
//        if (priorityOverrideProp != null) priorityOverrideProp.intValue = 0;

//        // Draw
//        EditorGUI.BeginProperty(position, label, property);

//        float line = EditorGUIUtility.singleLineHeight;
//        float space = EditorGUIUtility.standardVerticalSpacing;

//        // Foldout header
//        Rect r = new Rect(position.x, position.y, position.width, line);
//        property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label, true);

//        if (!property.isExpanded)
//        {
//            EditorGUI.EndProperty();
//            return;
//        }

//        EditorGUI.indentLevel++;

//        r.y += line + space;

//        // Helpers
//        Rect NextLine()
//        {
//            var rr = new Rect(r.x, r.y, r.width, line);
//            r.y += line + space;
//            return rr;
//        }

//        // --- Camera
//        EditorGUI.LabelField(NextLine(), "Camera", EditorStyles.boldLabel);
//        EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative("vcamId"));
//        EditorGUI.PropertyField(NextLine(), property.FindPropertyRelative("duration"));

//        // (unscaledTime hidden + forced false)

//        // --- Priority (optional)
//        // (priorityOverride hidden + forced 0)

//        // --- Blend Override
//        EditorGUI.LabelField(NextLine(), "Blend Override (optional)", EditorStyles.boldLabel);

//        var overrideBlend = property.FindPropertyRelative("overrideBlend");
//        var blendTime = property.FindPropertyRelative("blendTime");
//        var blendStyle = property.FindPropertyRelative("blendStyle");

//        EditorGUI.PropertyField(NextLine(), overrideBlend);
//        if (overrideBlend != null && overrideBlend.boolValue)
//        {
//            EditorGUI.PropertyField(NextLine(), blendTime);
//            EditorGUI.PropertyField(NextLine(), blendStyle);
//        }

//        // --- Fade To Black
//        EditorGUI.LabelField(NextLine(), "Fade To Black (optional)", EditorStyles.boldLabel);

//        var fadeIn = property.FindPropertyRelative("fadeIn");
//        var fadeInTime = property.FindPropertyRelative("fadeInTime");
//        var fadeOut = property.FindPropertyRelative("fadeOut");
//        var fadeOutTime = property.FindPropertyRelative("fadeOutTime");
//        var fadeColor = property.FindPropertyRelative("fadeColor");

//        EditorGUI.PropertyField(NextLine(), fadeIn);
//        if (fadeIn != null && fadeIn.boolValue)
//            EditorGUI.PropertyField(NextLine(), fadeInTime);

//        EditorGUI.PropertyField(NextLine(), fadeOut);
//        if (fadeOut != null && fadeOut.boolValue)
//            EditorGUI.PropertyField(NextLine(), fadeOutTime);

//        EditorGUI.PropertyField(NextLine(), fadeColor);

//        EditorGUI.indentLevel--;
//        EditorGUI.EndProperty();
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        float line = EditorGUIUtility.singleLineHeight;
//        float space = EditorGUIUtility.standardVerticalSpacing;

//        // 1 line for foldout always
//        if (!property.isExpanded) return line;

//        int lines = 0;

//        // Camera header + vcamId + duration
//        lines += 3;

//        // Blend header + overrideBlend + (optional blendTime + blendStyle)
//        lines += 2;
//        var overrideBlend = property.FindPropertyRelative("overrideBlend");
//        if (overrideBlend != null && overrideBlend.boolValue) lines += 2;

//        // Fade header + fadeIn + (optional fadeInTime) + fadeOut + (optional fadeOutTime) + fadeColor
//        lines += 3; // header + fadeIn + fadeOut
//        var fadeIn = property.FindPropertyRelative("fadeIn");
//        if (fadeIn != null && fadeIn.boolValue) lines += 1;
//        var fadeOut = property.FindPropertyRelative("fadeOut");
//        if (fadeOut != null && fadeOut.boolValue) lines += 1;
//        lines += 1; // fadeColor

//        // Total height = foldout line + expanded lines
//        float totalLines = 1 + lines;
//        return totalLines * line + (totalLines - 1) * space + PAD;
//    }
//}
//#endif
