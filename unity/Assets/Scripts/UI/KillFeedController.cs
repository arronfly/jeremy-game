using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 击杀播报控制器 - 管理击杀信息列表，自动淡出和移除
/// </summary>
public class KillFeedController : MonoBehaviour
{
    private RectTransform container;
    private List<KillFeedEntry> activeEntries = new List<KillFeedEntry>();
    private const int MaxEntries = 5;
    private const float EntryLifetime = 5f;
    private const float FadeDuration = 1f;
    private const float EntryHeight = 24f;
    private const float EntrySpacing = 4f;

    private static readonly Color RedTeamColor = new Color(0.9f, 0.3f, 0.3f, 1f);
    private static readonly Color BlueTeamColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    private static readonly Color DefaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    private struct KillFeedEntry
    {
        public GameObject root;
        public CanvasGroup group;
        public float remainingTime;
    }

    /// <summary>
    /// 初始化控制器
    /// </summary>
    public void Initialize(RectTransform parent)
    {
        container = parent;
    }

    /// <summary>
    /// 添加击杀播报条目
    /// </summary>
    public void AddEntry(string killer, string victim, string weapon, string team)
    {
        if (container == null) return;

        // 创建条目
        GameObject entryObj = new GameObject("KillFeedEntry");
        entryObj.transform.SetParent(container, false);

        RectTransform entryRect = entryObj.AddComponent<RectTransform>();
        entryRect.anchorMin = new Vector2(1, 1);
        entryRect.anchorMax = new Vector2(1, 1);
        entryRect.pivot = new Vector2(1, 1);
        entryRect.sizeDelta = new Vector2(260, EntryHeight);

        CanvasGroup group = entryObj.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        // 背景
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(entryRect, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.4f);

        // 文字组合
        string fullText = killer + " [" + weapon + "] " + victim;
        Text entryText = CreateTextObj("Text", entryRect, fullText);
        entryText.fontSize = 12;
        entryText.alignment = TextAnchor.MiddleRight;

        // 设置击杀者队伍颜色
        Color killerColor = (team == "red") ? RedTeamColor : BlueTeamColor;
        entryText.color = killerColor;

        // 记录条目
        KillFeedEntry entry = new KillFeedEntry
        {
            root = entryObj,
            group = group,
            remainingTime = EntryLifetime
        };
        activeEntries.Add(entry);

        // 开始淡出
        StartCoroutine(FadeOutEntry(entry));

        // 移除超出的旧条目
        while (activeEntries.Count > MaxEntries)
        {
            RemoveOldestEntry();
        }

        // 重新排列条目位置
        RepositionEntries();
    }

    private void RepositionEntries()
    {
        for (int i = 0; i < activeEntries.Count; i++)
        {
            if (activeEntries[i].root != null)
            {
                RectTransform rt = activeEntries[i].root.GetComponent<RectTransform>();
                float yOffset = -i * (EntryHeight + EntrySpacing);
                rt.anchoredPosition = new Vector2(0, yOffset);
            }
        }
    }

    private void RemoveOldestEntry()
    {
        if (activeEntries.Count == 0) return;

        KillFeedEntry oldest = activeEntries[0];
        activeEntries.RemoveAt(0);

        if (oldest.root != null)
        {
            Destroy(oldest.root);
        }
    }

    private IEnumerator FadeOutEntry(KillFeedEntry entry)
    {
        // 等待显示时间
        yield return new WaitForSeconds(EntryLifetime - FadeDuration);

        // 渐变淡出
        if (entry.group != null)
        {
            float elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                if (entry.group != null)
                {
                    entry.group.alpha = 1f - (elapsed / FadeDuration);
                }
                yield return null;
            }
        }

        // 移除
        activeEntries.Remove(entry);
        if (entry.root != null)
        {
            Destroy(entry.root);
        }
        RepositionEntries();
    }

    private Text CreateTextObj(string name, RectTransform parent, string content)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = new Vector2(5, 0);
        rect.offsetMax = new Vector2(-5, 0);

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = content;
        text.raycastTarget = false;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }
}