using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏结束界面 - 显示胜负结果和比分
/// 通过静态变量接收比赛结果
/// </summary>
public class EndScreen : MonoBehaviour
{
    public static string winnerTeam = "red";
    public static int[] finalScores = new int[] { 0, 0 };

    private Canvas canvas;

    void Start()
    {
        BuildUI();
    }

    void BuildUI()
    {
        CreateCanvas();
        CreateBackground();
        CreateResultText();
        CreateScoreDisplay();
        CreateStatsArea();
        CreatePlayAgainButton();
        CreateMainMenuButton();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("EndScreenCanvas");
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

    void CreateResultText()
    {
        bool playerWon = (winnerTeam == TeamSelect.selectedTeam);
        string resultText = playerWon ? "VICTORY!" : "DEFEAT!";
        Color resultColor = playerWon ? HexToColor("#27AE60") : new Color(0.9f, 0.2f, 0.2f, 1f);

        GameObject resultObj = CreateUIElement("ResultText", canvas.transform);
        Text result = resultObj.AddComponent<Text>();
        result.text = resultText;
        result.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        result.fontSize = 72;
        result.color = resultColor;
        result.alignment = TextAnchor.MiddleCenter;
        result.rectTransform.anchorMin = new Vector2(0.1f, 0.72f);
        result.rectTransform.anchorMax = new Vector2(0.9f, 0.88f);
        result.rectTransform.sizeDelta = Vector2.zero;
    }

    void CreateScoreDisplay()
    {
        int redScore = finalScores[0];
        int blueScore = finalScores[1];
        string scoreString = "RED  " + redScore + " - " + blueScore + "  BLUE";

        GameObject scoreObj = CreateUIElement("ScoreDisplay", canvas.transform);
        Text score = scoreObj.AddComponent<Text>();
        score.text = scoreString;
        score.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        score.fontSize = 48;
        score.alignment = TextAnchor.MiddleCenter;
        score.rectTransform.anchorMin = new Vector2(0.1f, 0.58f);
        score.rectTransform.anchorMax = new Vector2(0.9f, 0.7f);
        score.rectTransform.sizeDelta = Vector2.zero;

        // 使用富文本为队伍名称着色
        score.supportRichText = true;
        score.text = "<color=#FF4444>RED</color>  " + redScore + " - " + blueScore + "  <color=#4444FF>BLUE</color>";
    }

    void CreateStatsArea()
    {
        GameObject statsObj = CreateUIElement("StatsArea", canvas.transform);
        Text stats = statsObj.AddComponent<Text>();
        stats.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        stats.fontSize = 24;
        stats.color = new Color(0.75f, 0.75f, 0.75f, 1f);
        stats.alignment = TextAnchor.MiddleCenter;
        stats.lineSpacing = 1.8f;
        stats.rectTransform.anchorMin = new Vector2(0.2f, 0.38f);
        stats.rectTransform.anchorMax = new Vector2(0.8f, 0.55f);
        stats.rectTransform.sizeDelta = Vector2.zero;

        stats.supportRichText = true;
        stats.text =
            "--- MATCH STATS ---\n\n" +
            "<color=#FF4444>Red Team:</color>  " + finalScores[0] + " rounds won\n" +
            "<color=#4444FF>Blue Team:</color>  " + finalScores[1] + " rounds won";
    }

    void CreatePlayAgainButton()
    {
        GameObject btnObj = CreateButton("PlayAgainBtn", canvas.transform,
            "PLAY AGAIN", HexToColor("#27AE60"), new Vector2(0.5f, 0.22f), new Vector2(350, 70));
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransition.LoadScene("TeamSelect");
        });
    }

    void CreateMainMenuButton()
    {
        GameObject btnObj = CreateButton("MainMenuBtn", canvas.transform,
            "MAIN MENU", new Color(0.3f, 0.3f, 0.3f, 1f), new Vector2(0.5f, 0.1f), new Vector2(350, 70));
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransition.LoadScene("MainMenu");
        });
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
