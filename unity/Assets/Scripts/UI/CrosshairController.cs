using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 准心控制器 - 管理4条准心线的位置和扩散动画
/// </summary>
public class CrosshairController : MonoBehaviour
{
    private Image[] lines;       // 上、下、左、右 4条线
    private float currentSpread;
    private float targetSpread;
    private float defaultSpread = 5f;
    private float returnSpeed = 15f;

    private readonly Vector2 lineSize = new Vector2(3, 20);

    /// <summary>
    /// 初始化准心线段引用
    /// </summary>
    public void Initialize(Image[] crosshairLines)
    {
        lines = crosshairLines;
        currentSpread = defaultSpread;
        targetSpread = defaultSpread;

        // 设置基础尺寸
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
            {
                RectTransform rt = lines[i].rectTransform;
                rt.sizeDelta = lineSize;
            }
        }
    }

    /// <summary>
    /// 设置准心扩散值，射击时调用
    /// </summary>
    public void SetSpread(float spread)
    {
        targetSpread = Mathf.Max(spread, defaultSpread);
    }

    private void Update()
    {
        // 平滑回缩到默认扩散
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * returnSpeed);
        targetSpread = Mathf.Lerp(targetSpread, defaultSpread, Time.deltaTime * returnSpeed);

        if (lines == null) return;

        float halfGap = currentSpread;

        // 上
        SetLinePosition(0, new Vector2(0, halfGap + lineSize.y * 0.5f));
        // 下
        SetLinePosition(1, new Vector2(0, -(halfGap + lineSize.y * 0.5f)));
        // 左
        SetLinePosition(2, new Vector2(-(halfGap + lineSize.y * 0.5f), 0));
        // 右
        SetLinePosition(3, new Vector2(halfGap + lineSize.y * 0.5f, 0));

        // 横向线段旋转90度
        if (lines[2] != null) lines[2].rectTransform.rotation = Quaternion.Euler(0, 0, 90);
        if (lines[3] != null) lines[3].rectTransform.rotation = Quaternion.Euler(0, 0, 90);
    }

    private void SetLinePosition(int index, Vector2 anchoredPos)
    {
        if (index < lines.Length && lines[index] != null)
        {
            lines[index].rectTransform.anchoredPosition = anchoredPos;
        }
    }
}