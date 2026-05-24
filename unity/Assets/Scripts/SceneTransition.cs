using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 场景过渡工具 - 淡入淡出黑屏过渡
/// 单例模式，跨场景不销毁
/// </summary>
public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;

    private Canvas canvas;
    private Image fadeOverlay;
    private Text loadingText;
    private bool isTransitioning = false;

    void Awake()
    {
        // 单例模式
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        SetupTransitionUI();
    }

    void SetupTransitionUI()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 全屏黑色遮罩
        GameObject overlayObj = new GameObject("FadeOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        fadeOverlay = overlayObj.AddComponent<Image>();
        fadeOverlay.color = new Color(0, 0, 0, 0f);
        fadeOverlay.raycastTarget = true;
        RectTransform rt = overlayObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // Loading文字
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(canvasObj.transform, false);
        loadingText = textObj.AddComponent<Text>();
        loadingText.text = "LOADING...";
        loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadingText.fontSize = 36;
        loadingText.color = Color.white;
        loadingText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.3f, 0.45f);
        textRt.anchorMax = new Vector2(0.7f, 0.55f);
        textRt.sizeDelta = Vector2.zero;
        loadingText.enabled = false;
    }

    /// <summary>
    /// 淡出到黑屏然后加载场景
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.TransitionCoroutine(sceneName));
        }
    }

    IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // 禁用交互
        fadeOverlay.raycastTarget = true;

        // 淡出 (0 -> 1)
        loadingText.enabled = true;
        yield return Fade(0f, 1f, 0.5f);

        // 加载场景
        SceneManager.LoadScene(sceneName);

        // 淡入 (1 -> 0)
        yield return Fade(1f, 0f, 0.5f);

        loadingText.enabled = false;
        fadeOverlay.raycastTarget = false;
        isTransitioning = false;
    }

    IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, toAlpha);
    }
}
