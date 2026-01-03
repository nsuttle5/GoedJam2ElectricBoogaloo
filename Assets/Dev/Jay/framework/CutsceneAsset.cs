using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cutscenes/Cutscene Asset", fileName = "Cutscene_")]
public sealed class CutsceneAsset : ScriptableObject
{
    [Header("Startup Loading")]
    [Tooltip("Scenes to load additively before playing this cutscene.")]
    public List<string> additiveScenesToLoad = new();

    [Header("Shots")]
    public List<CutsceneShot> shots = new();

    [Header("End Behavior")]
    [Tooltip("If set, we load this scene in Single mode at the end (optionally preloaded).")]
    public string nextSceneSingleLoad = "";

    public bool preloadNextSceneDuringLastShot = true;
    [Min(0f)] public float preloadLeadSeconds = 1.0f;

    [Serializable]
    public struct CutsceneShot
    {
        [Header("Camera")]
        public string vcamId;

        [Min(0f)]
        public float duration;

        [Tooltip("If true, uses unscaled time (ignores Time.timeScale).")]
        public bool unscaledTime;

        [Header("Priority (optional)")]
        [Tooltip("<= 0 means use manager's Active Priority.")]
        public int priorityOverride;

        [Header("Blend Override (optional, applied for the transition INTO this shot)")]
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
