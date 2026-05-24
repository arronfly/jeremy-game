using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 队伍选择界面 - 选择红蓝队伍和难度
/// 通过静态变量传递选择到GamePlay场景
/// </summary>
public class TeamSelect : MonoBehaviour
{
    public static string selectedTeam = "red";
    public static string selectedDifficulty = "normal";

    private Canvas canvas;
    private GameObject redPanel;
    private GameObject bluePanel;
    private Image redBorder;
    private Image blueBorder;
    private GameObject easyBtn;
    private GameObject normalBtn;
    private GameObject hardBtn;

    void Start()
    {
        BuildUI();
        UpdateSelectionDisplay();
    }

    void BuildUI()
    {
        CreateCanvas();
        CreateBackground();
        CreateTitle();

        // 队伍选择面板
        CreateRedTeamPanel();
        CreateBlueTeamPanel();

        // 难度选择
        CreateDifficultySection();

        // 底部按钮
        CreateDeployButton();
        CreateBackButton();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("TeamSelectCanvas");
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
        title.text = "SELECT YOUR TEAM";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 48;
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        title.rectTransform.anchorMin = new Vector2(0.15f, 0.82f);
        title.rectTransform.anchorMax = new Vector2(0.85f, 0.92f);
        title.rectTransform.sizeDelta = Vector2.zero;
    }

    void CreateRedTeamPanel()
    {
        redPanel = CreateUIElement("RedTeamPanel", canvas.transform);
        Image panelBg = redPanel.AddComponent<Image>();
        panelBg.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
        redPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.08f, 0.4f);
        redPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.46f, 0.75f);
        redPanel.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 红色边框
        GameObject borderObj = CreateUIElement("RedBorder", redPanel.transform);
        redBorder = borderObj.AddComponent<Image>();
        redBorder.color = Color.red;
        borderObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        borderObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        borderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-8, -8);

        // 内部填充
        GameObject inner = CreateUIElement("Inner", borderObj.transform);
        Image innerBg = inner.AddComponent<Image>();
        innerBg.color = new Color(0.15f, 0.08f, 0.08f, 1f);
        inner.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        inner.GetComponent<RectTransform>().anchorMax = Vector2.one;
        inner.GetComponent<RectTransform>().sizeDelta = new Vector2(-6, -6);

        // 标签
        GameObject labelObj = CreateUIElement("RedLabel", inner.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = "RED TEAM";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 32;
        label.color = new Color(1f, 0.4f, 0.4f, 1f);
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = new Vector2(0.1f, 0.6f);
        label.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        label.rectTransform.sizeDelta = Vector2.zero;

        // 选择按钮
        CreatePanelButton("RedSelectBtn", inner.transform, "SELECT", new Vector2(0.5f, 0.2f), () =>
        {
            selectedTeam = "red";
            UpdateSelectionDisplay();
        });
    }

    void CreateBlueTeamPanel()
    {
        bluePanel = CreateUIElement("BlueTeamPanel", canvas.transform);
        Image panelBg = bluePanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        bluePanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.54f, 0.4f);
        bluePanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.92f, 0.75f);
        bluePanel.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 蓝色边框
        GameObject borderObj = CreateUIElement("BlueBorder", bluePanel.transform);
        blueBorder = borderObj.AddComponent<Image>();
        blueBorder.color = Color.blue;
        borderObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        borderObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        borderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-8, -8);

        // 内部填充
        GameObject inner = CreateUIElement("Inner", borderObj.transform);
        Image innerBg = inner.AddComponent<Image>();
        innerBg.color = new Color(0.08f, 0.08f, 0.15f, 1f);
        inner.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        inner.GetComponent<RectTransform>().anchorMax = Vector2.one;
        inner.GetComponent<RectTransform>().sizeDelta = new Vector2(-6, -6);

        // 标签
        GameObject labelObj = CreateUIElement("BlueLabel", inner.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = "BLUE TEAM";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 32;
        label.color = new Color(0.4f, 0.4f, 1f, 1f);
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = new Vector2(0.1f, 0.6f);
        label.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        label.rectTransform.sizeDelta = Vector2.zero;

        // 选择按钮
        CreatePanelButton("BlueSelectBtn", inner.transform, "SELECT", new Vector2(0.5f, 0.2f), () =>
        {
            selectedTeam = "blue";
            UpdateSelectionDisplay();
        });
    }

    void CreateDifficultySection()
    {
        // 难度标题
        GameObject diffTitle = CreateUIElement("DiffTitle", canvas.transform);
        Text diffText = diffTitle.AddComponent<Text>();
        diffText.text = "DIFFICULTY";
        diffText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        diffText.fontSize = 28;
        diffText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        diffText.alignment = TextAnchor.MiddleCenter;
        diffText.rectTransform.anchorMin = new Vector2(0.3f, 0.3f);
        diffText.rectTransform.anchorMax = new Vector2(0.7f, 0.38f);
        diffText.rectTransform.sizeDelta = Vector2.zero;

        // Easy
        easyBtn = CreateDiffButton("EasyBtn", canvas.transform, "EASY", new Vector2(0.2f, 0.22f), () =>
        {
            selectedDifficulty = "easy";
            UpdateDiffButtons();
        });

        // Normal
        normalBtn = CreateDiffButton("NormalBtn", canvas.transform, "NORMAL", new Vector2(0.5f, 0.22f), () =>
        {
            selectedDifficulty = "normal";
            UpdateDiffButtons();
        });

        // Hard
        hardBtn = CreateDiffButton("HardBtn", canvas.transform, "HARD", new Vector2(0.8f, 0.22f), () =>
        {
            selectedDifficulty = "hard";
            UpdateDiffButtons();
        });
    }

    void CreateDeployButton()
    {
        GameObject btn = CreateLargeButton("DeployBtn", canvas.transform,
            "DEPLOY", HexToColor("#27AE60"), new Vector2(0.5f, 0.1f), new Vector2(350, 70));
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            DifficultyManager.SetDifficulty(selectedDifficulty);
            SceneTransition.LoadScene("GamePlay");
        });
    }

    void CreateBackButton()
    {
        GameObject btn = CreateLargeButton("BackBtn", canvas.transform,
            "BACK", new Color(0.3f, 0.3f, 0.3f, 1f), new Vector2(0.5f, 0.03f), new Vector2(200, 50));
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransition.LoadScene("MainMenu");
        });
    }

    void UpdateSelectionDisplay()
    {
        bool isRed = selectedTeam == "red";

        if (redBorder != null)
            redBorder.color = isRed ? new Color(1f, 0.3f, 0.3f, 1f) : new Color(0.4f, 0.15f, 0.15f, 0.5f);
        if (blueBorder != null)
            blueBorder.color = !isRed ? new Color(0.3f, 0.3f, 1f, 1f) : new Color(0.15f, 0.15f, 0.4f, 0.5f);

        UpdateDiffButtons();
    }

    void UpdateDiffButtons()
    {
        SetButtonHighlight(easyBtn, selectedDifficulty == "easy");
        SetButtonHighlight(normalBtn, selectedDifficulty == "normal");
        SetButtonHighlight(hardBtn, selectedDifficulty == "hard");
    }

    void SetButtonHighlight(GameObject btn, bool selected)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = selected ? HexToColor("#27AE60") : new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    GameObject CreatePanelButton(string name, Transform parent, string labelText, Vector2 anchorCenter, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.35f, 0.35f, 0.35f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        btnImage.rectTransform.anchorMin = new Vector2(anchorCenter.x - 0.15f, anchorCenter.y - 0.08f);
        btnImage.rectTransform.anchorMax = new Vector2(anchorCenter.x + 0.15f, anchorCenter.y + 0.08f);
        btnImage.rectTransform.sizeDelta = Vector2.zero;

        GameObject labelObj = CreateUIElement("Label", btnObj.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 22;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;

        return btnObj;
    }

    GameObject CreateDiffButton(string name, Transform parent, string labelText, Vector2 anchorCenter, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        btnImage.rectTransform.anchorMin = new Vector2(anchorCenter.x - 0.1f, anchorCenter.y - 0.04f);
        btnImage.rectTransform.anchorMax = new Vector2(anchorCenter.x + 0.1f, anchorCenter.y + 0.04f);
        btnImage.rectTransform.sizeDelta = Vector2.zero;

        GameObject labelObj = CreateUIElement("Label", btnObj.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 20;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;

        return btnObj;
    }

    GameObject CreateLargeButton(string name, Transform parent, string labelText, Color bgColor, Vector2 anchorCenter, Vector2 size)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = bgColor;
        btnObj.AddComponent<Button>();

        btnImage.rectTransform.anchorMin = new Vector2(anchorCenter.x - 0.01f, anchorCenter.y - 0.01f);
        btnImage.rectTransform.anchorMax = new Vector2(anchorCenter.x + 0.01f, anchorCenter.y + 0.01f);
        btnImage.rectTransform.sizeDelta = size;

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
