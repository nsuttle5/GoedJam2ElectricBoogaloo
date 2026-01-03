using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScreenFader : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image image;

    public static ScreenFader EnsureExists()
    {
        var existing = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        if (existing != null) return existing;

        var go = new GameObject("ScreenFader");
        DontDestroyOnLoad(go);

        var fader = go.AddComponent<ScreenFader>();
        fader.BuildUI();
        return fader;
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var imgGO = new GameObject("Fade");
        imgGO.transform.SetParent(canvasGO.transform, false);
        image = imgGO.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(0, 0, 0, 0);

        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public Coroutine FadeTo(Color color, float duration, bool unscaled)
    {
        return StartCoroutine(FadeRoutine(color, duration, unscaled));
    }

    private IEnumerator FadeRoutine(Color target, float duration, bool unscaled)
    {
        if (image == null) yield break;

        Color start = image.color;
        if (duration <= 0f)
        {
            image.color = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            float dt = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float a = Mathf.Clamp01(t / duration);
            image.color = Color.Lerp(start, target, a);
            yield return null;
        }
        image.color = target;
    }
}
