using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单 - ESC键切换暂停状态
/// 包含: 继续、音量调节、退出到主菜单
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private Canvas canvas;
    private GameObject pausePanel;
    private bool isPaused = false;

    void Start()
    {
        BuildPauseUI();
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void BuildPauseUI()
    {
        CreateCanvas();
        CreatePausePanel();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("PauseCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreatePausePanel()
    {
        // 半透明黑色背景
        pausePanel = CreateUIElement("PausePanel", canvas.transform);
        Image panelBg = pausePanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.75f);
        panelBg.raycastTarget = true;
        RectTransform panelRt = pausePanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        // PAUSED 标题
        GameObject titleObj = CreateUIElement("PausedTitle", pausePanel.transform);
        Text title = titleObj.AddComponent<Text>();
        title.text = "PAUSED";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 56;
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        title.rectTransform.anchorMin = new Vector2(0.2f, 0.72f);
        title.rectTransform.anchorMax = new Vector2(0.8f, 0.85f);
        title.rectTransform.sizeDelta = Vector2.zero;

        // RESUME 按钮
        CreateMenuButton("ResumeBtn", pausePanel.transform, "RESUME",
            HexToColor("#27AE60"), new Vector2(0.5f, 0.55f), new Vector2(300, 65),
            () => { TogglePause(); });

        // 音量滑块区域
        CreateVolumeSlider();

        // QUIT TO MENU 按钮
        CreateMenuButton("QuitBtn", pausePanel.transform, "QUIT TO MENU",
            new Color(0.6f, 0.15f, 0.15f, 1f), new Vector2(0.5f, 0.18f), new Vector2(300, 65),
            () =>
            {
                TogglePause();
                SceneTransition.LoadScene("MainMenu");
            });
    }

    void CreateVolumeSlider()
    {
        // 音量标签
        GameObject volumeLabel = CreateUIElement("VolumeLabel", pausePanel.transform);
        Text labelText = volumeLabel.AddComponent<Text>();
        labelText.text = "VOLUME";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 22;
        labelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.rectTransform.anchorMin = new Vector2(0.3f, 0.42f);
        labelText.rectTransform.anchorMax = new Vector2(0.7f, 0.48f);
        labelText.rectTransform.sizeDelta = Vector2.zero;

        // 滑块背景
        GameObject sliderBg = CreateUIElement("SliderBg", pausePanel.transform);
        Image bgImage = sliderBg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        sliderBg.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0.34f);
        sliderBg.GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 0.38f);
        sliderBg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 滑块
        GameObject sliderObj = CreateUIElement("VolumeSlider", sliderBg.transform);
        Slider slider = sliderObj.AddComponent<Slider>();
        sliderObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        sliderObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        sliderObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 滑块填充区域
        GameObject fillArea = CreateUIElement("FillArea", sliderObj.transform);
        fillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
        fillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = HexToColor("#27AE60");
        fill.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fill.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 滑块把手
        GameObject handleArea = CreateUIElement("HandleSlideArea", sliderObj.transform);
        handleArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        handleArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
        handleArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        GameObject handle = CreateUIElement("Handle", handleArea.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 30);

        // 配置滑块
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        slider.onValueChanged.AddListener((float value) =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
                AudioManager.Instance.SetSFXVolume(value);
            }
        });
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
    }

    void CreateMenuButton(string name, Transform parent, string labelText, Color bgColor,
        Vector2 anchorCenter, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = bgColor;
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        btnImage.rectTransform.anchorMin = new Vector2(anchorCenter.x - 0.01f, anchorCenter.y - 0.01f);
        btnImage.rectTransform.anchorMax = new Vector2(anchorCenter.x + 0.01f, anchorCenter.y + 0.01f);
        btnImage.rectTransform.sizeDelta = size;

        GameObject labelObj = CreateUIElement("Label", btnObj.transform);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 26;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;
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

    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}
