using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单 - 程序化构建UI
/// 包含: 标题、开始游戏、操作说明、退出按钮
/// </summary>
public class MainMenu : MonoBehaviour
{
    private Canvas canvas;
    private GameObject controlOverlay;

    void Start()
    {
        BuildMenu();
    }

    void BuildMenu()
    {
        // 创建Canvas
        CreateCanvas();

        // 背景
        CreateBackground();

        // 标题
        CreateTitle();

        // 副标题
        CreateSubtitle();

        // 按钮区域
        CreateStartButton();
        CreateControlsButton();
        CreateQuitButton();

        // 操作说明覆盖层
        CreateControlOverlay();

        // 版本号
        CreateVersionText();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("MenuCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreateBackground()
    {
        GameObject bg = CreateUIElement("Background", canvas.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = HexToColor("#2D3A1B");
        bgImage.rectTransform.anchorMin = Vector2.zero;
        bgImage.rectTransform.anchorMax = Vector2.one;
        bgImage.rectTransform.sizeDelta = Vector2.zero;
    }

    void CreateTitle()
    {
        GameObject titleObj = CreateUIElement("Title", canvas.transform);
        Text title = titleObj.AddComponent<Text>();
        title.text = "\u6BBC\u706D\u56E2\u7ADE2";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 64;
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        title.rectTransform.anchorMin = new Vector2(0.2f, 0.7f);
        title.rectTransform.anchorMax = new Vector2(0.8f, 0.85f);
        title.rectTransform.sizeDelta = Vector2.zero;
    }

    void CreateSubtitle()
    {
        GameObject subtitleObj = CreateUIElement("Subtitle", canvas.transform);
        Text subtitle = subtitleObj.AddComponent<Text>();
        subtitle.text = "4 v 4 \u56E2\u961F\u6BBC\u706D";
        subtitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitle.fontSize = 28;
        subtitle.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        subtitle.alignment = TextAnchor.MiddleCenter;
        subtitle.rectTransform.anchorMin = new Vector2(0.2f, 0.62f);
        subtitle.rectTransform.anchorMax = new Vector2(0.8f, 0.7f);
        subtitle.rectTransform.sizeDelta = Vector2.zero;
    }

    void CreateStartButton()
    {
        GameObject btnObj = CreateButton("StartButton", canvas.transform,
            "START GAME", HexToColor("#27AE60"), new Vector2(0.5f, 0.48f), new Vector2(300, 80));
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransition.LoadScene("TeamSelect");
        });
    }

    void CreateControlsButton()
    {
        GameObject btnObj = CreateButton("ControlsButton", canvas.transform,
            "CONTROLS", new Color(0.3f, 0.3f, 0.3f, 1f), new Vector2(0.5f, 0.34f), new Vector2(300, 60));
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            controlOverlay.SetActive(true);
        });
    }

    void CreateQuitButton()
    {
        GameObject btnObj = CreateButton("QuitButton", canvas.transform,
            "QUIT", new Color(0.6f, 0.15f, 0.15f, 1f), new Vector2(0.5f, 0.22f), new Vector2(300, 60));
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }

    void CreateControlOverlay()
    {
        controlOverlay = CreateUIElement("ControlOverlay", canvas.transform);
        controlOverlay.SetActive(false);

        // 半透明背景
        Image overlayBg = controlOverlay.AddComponent<Image>();
        overlayBg.color = new Color(0, 0, 0, 0.85f);
        overlayBg.rectTransform.anchorMin = Vector2.zero;
        overlayBg.rectTransform.anchorMax = Vector2.one;
        overlayBg.rectTransform.sizeDelta = Vector2.zero;

        // 操作说明内容面板
        GameObject panel = CreateUIElement("ControlPanel", controlOverlay.transform);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        panelBg.rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
        panelBg.rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
        panelBg.rectTransform.sizeDelta = Vector2.zero;

        // 操作说明文字
        GameObject textObj = CreateUIElement("ControlText", panel.transform);
        Text controls = textObj.AddComponent<Text>();
        controls.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        controls.fontSize = 24;
        controls.color = Color.white;
        controls.alignment = TextAnchor.MiddleCenter;
        controls.lineSpacing = 1.5f;
        controls.text =
            "--- CONTROLS ---\n\n" +
            "WASD        Move\n" +
            "Mouse       Aim\n" +
            "LMB         Shoot\n" +
            "R           Reload\n" +
            "1-4         Weapons\n" +
            "G           Grenade\n" +
            "7/8/9       Medkits";
        controls.rectTransform.anchorMin = new Vector2(0.1f, 0.15f);
        controls.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        controls.rectTransform.sizeDelta = Vector2.zero;

        // 关闭按钮
        GameObject closeBtn = CreateButton("CloseButton", panel.transform,
            "CLOSE", new Color(0.3f, 0.3f, 0.3f, 1f), new Vector2(0.5f, 0.08f), new Vector2(200, 50));
        closeBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            controlOverlay.SetActive(false);
        });
    }

    void CreateVersionText()
    {
        GameObject versionObj = CreateUIElement("Version", canvas.transform);
        Text version = versionObj.AddComponent<Text>();
        version.text = "v1.0";
        version.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        version.fontSize = 16;
        version.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        version.alignment = TextAnchor.LowerRight;
        version.rectTransform.anchorMin = new Vector2(0.85f, 0.02f);
        version.rectTransform.anchorMax = new Vector2(0.98f, 0.06f);
        version.rectTransform.sizeDelta = Vector2.zero;
    }

    GameObject CreateButton(string name, Transform parent, string labelText, Color bgColor, Vector2 anchorCenter, Vector2 size)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = bgColor;
        btnObj.AddComponent<Button>();

        btnImage.rectTransform.anchorMin = new Vector2(anchorCenter.x - 0.01f, anchorCenter.y - 0.01f);
        btnImage.rectTransform.anchorMax = new Vector2(anchorCenter.x + 0.01f, anchorCenter.y + 0.01f);
        btnImage.rectTransform.sizeDelta = size;

        // 按钮文字
        GameObject labelObj = CreateUIElement("Label", btnObj.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;

        return btnObj;
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        return obj;
    }

    Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        return new Color(r, g, b, 1f);
    }
}
