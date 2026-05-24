using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 回合公告覆盖控制器 - 显示回合开始/结束等公告文字
/// 淡入 -> 保持 -> 淡出
/// </summary>
public class RoundOverlayController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Text announcementText;
    private Coroutine activeCoroutine;

    /// <summary>
    /// 初始化控制器
    /// </summary>
    public void Initialize(CanvasGroup group, Text text)
    {
        canvasGroup = group;
        announcementText = text;

        // 初始状态：隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 显示公告文字，持续指定时间后自动淡出
    /// </summary>
    public void Show(string text, float duration)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(ShowSequence(text, duration));
    }

    private IEnumerator ShowSequence(string text, float duration)
    {
        // 设置文字
        if (announcementText != null)
        {
            announcementText.text = text;
        }

        // 根据文字内容设置颜色
        if (announcementText != null)
        {
            if (text.Contains("RED"))
                announcementText.color = new Color(1f, 0.35f, 0.35f, 1f);
            else if (text.Contains("BLUE"))
                announcementText.color = new Color(0.35f, 0.65f, 1f, 1f);
            else if (text.Contains("SUDDEN"))
                announcementText.color = new Color(1f, 0.85f, 0.2f, 1f);
            else
                announcementText.color = Color.white;
        }

        // 淡入
        yield return Fade(0f, 1f, 0.5f);

        // 保持显示
        yield return new WaitForSeconds(duration - 1f);

        // 淡出
        yield return Fade(1f, 0f, 0.5f);

        activeCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}