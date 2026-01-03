using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cutscenes/Cutscene Asset", fileName = "Cutscene_")]
public sealed class CutsceneAsset : ScriptableObject
{
    [Header("Startup")]
    [Tooltip("All scenes to load additively before playing this cutscene.")]
    public List<string> additiveScenesToLoad = new();

    [Header("Shots")]
    public List<CutsceneShot> shots = new();

    [Header("End Behavior")]
    [Tooltip("If set, load this scene in Single mode at the end.")]
    public string nextSceneSingleLoad = "";

    public bool preloadNextSceneDuringLastShot = true;
    [Min(0f)] public float preloadLeadSeconds = 1.0f;

    [Serializable]
    public struct CutsceneShot
    {
        [Header("Shot Name")]
        public string name;

        [Header("Camera")]
        public string vCamID;

        [Min(0f)]
        public float duration;

        [Header("Blend Override (applies for transition INTO this shot)")]
        public bool overrideBlend;
        [Min(0f)] public float blendTime;
        public BlendStyle blendStyle;

        [Header("Fade (optional)")]
        public bool fadeIn;
        [Min(0f)] public float fadeInTime;

        public bool fadeOut;
        [Min(0f)] public float fadeOutTime;

        public Color fadeColor;
    }

    public enum BlendStyle
    {
        EaseInOut,
        EaseIn,
        EaseOut,
        Linear,
        Cut
    }
}
