using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键场景设置 - 快速搭建游戏场景
/// 使用方法：直接拖到场景中或从菜单 GameObject > JeremyGame > QuickSetup 创建
/// </summary>
public class QuickSetup : MonoBehaviour
{
    [Header("预制体引用 - 需要手动拖入")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject bulletPrefab;

    [Header("生成点设置")]
    public Vector2[] redSpawnPoints = new Vector2[]
    {
        new Vector2(-6, 2),
        new Vector2(-6, 0),
        new Vector2(-6, -2),
        new Vector2(-4, 1)
    };

    public Vector2[] blueSpawnPoints = new Vector2[]
    {
        new Vector2(6, -2),
        new Vector2(6, 0),
        new Vector2(6, 2),
        new Vector2(4, -1)
    };

    [Header("游戏边界")]
    public float boundX = 8f;
    public float boundY = 4.5f;

    private void Start()
    {
        SetupGame();
    }

    public void SetupGame()
    {
        Debug.Log("=== Jeremy Game 快速场景设置 ===");

        // 1. 设置相机
        SetupCamera();

        // 2. 创建游戏管理器
        GameObject gameManager = CreateGameManager();

        // 3. 创建回合管理器
        GameObject roundManager = CreateRoundManager();

        // 4. 创建HUD
        GameObject hud = CreateHUD();

        // 5. 生成玩家
        CreatePlayers();

        // 6. 设置边界
        SetupBounds();

        Debug.Log("=== 场景设置完成! 按 Play 运行游戏 ===");
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
        }
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
    }

    GameObject CreateGameManager()
    {
        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        go.AddComponent<BulletManager>();

        // 设置预制体引用
        BulletManager bm = go.GetComponent<BulletManager>();
        if (bulletPrefab != null)
            bm.bulletPrefab = bulletPrefab;

        Debug.Log("GameManager 创建完成");
        return go;
    }

    GameObject CreateRoundManager()
    {
        GameObject go = new GameObject("RoundManager");
        RoundManager rm = go.AddComponent<RoundManager>();

        // 设置回合信息
        SerializedObject serialized = new SerializedObject(rm);
        serialized.FindProperty("totalRounds").intValue = 7;
        serialized.FindProperty("roundsToWin").intValue = 4;
        serialized.ApplyModifiedProperties();

        Debug.Log("RoundManager 创建完成");
        return go;
    }

    GameObject CreateHUD()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 创建面板背景
        GameObject panel = new GameObject("HUDPanel");
        panel.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;

        // 左上角 - 红队分数
        CreateUIElement("RedScore", panel.transform, new Vector2(20, -20),
            TextAnchor.UpperLeft, "红队: 0", new Color(0.9f, 0.3f, 0.3f));

        // 右上角 - 蓝队分数
        CreateUIElement("BlueScore", panel.transform, new Vector2(-20, -20),
            TextAnchor.UpperRight, "蓝队: 0", new Color(0.3f, 0.5f, 0.9f));

        // 中上 - 回合信息
        CreateUIElement("RoundInfo", panel.transform, new Vector2(0, -20),
            TextAnchor.UpperCenter, "第 1 回合/7", Color.white);

        // 左下角 - 生命值
        GameObject healthPanel = new GameObject("HealthPanel");
        healthPanel.transform.SetParent(panel.transform);
        RectTransform healthRect = healthPanel.AddComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0, 0);
        healthRect.anchorMax = new Vector2(0, 0);
        healthRect.anchoredPosition = new Vector2(20, 20);
        healthRect.sizeDelta = new Vector2(200, 30);

        UnityEngine.UI.Image healthBar = healthPanel.AddComponent<UnityEngine.UI.Image>();
        healthBar.color = Color.green;

        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(healthPanel.transform);
        RectTransform healthTextRect = healthTextObj.AddComponent<RectTransform>();
        healthTextRect.anchorMin = Vector2.zero;
        healthTextRect.anchorMax = Vector2.one;
        healthTextRect.anchoredPosition = Vector2.zero;
        healthTextRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Text healthText = healthTextObj.AddComponent<UnityEngine.UI.Text>();
        healthText.text = "100/100";
        healthText.fontSize = 16;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;

        Debug.Log("HUD 创建完成");
        return canvasObj;
    }

    GameObject CreateUIElement(string name, Transform parent, Vector2 anchoredPosition, TextAnchor alignment, string text, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(200, 30);

        UnityEngine.UI.Text txt = go.AddComponent<UnityEngine.UI.Text>();
        txt.text = text;
        txt.fontSize = 20;
        txt.alignment = alignment;
        txt.color = color;

        return go;
    }

    void CreatePlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player Prefab 未设置!");
            return;
        }

        // 生成红队玩家
        for (int i = 0; i < redSpawnPoints.Length; i++)
        {
            GameObject player = Instantiate(playerPrefab, redSpawnPoints[i], Quaternion.identity);
            player.name = "RedPlayer_" + (i + 1);
            player.tag = "Player";

            // 设置队伍颜色
            PlayerVisual visual = player.GetComponent<PlayerVisual>();
            if (visual != null)
            {
                visual.SetTeamColor(new Color(0.9f, 0.3f, 0.3f));
            }
        }

        // 生成蓝队Bot
        if (botPrefab == null)
        {
            Debug.LogWarning("Bot Prefab 未设置!");
            return;
        }

        for (int i = 0; i < blueSpawnPoints.Length; i++)
        {
            GameObject bot = Instantiate(botPrefab, blueSpawnPoints[i], Quaternion.identity);
            bot.name = "BlueBot_" + (i + 1);
            bot.tag = "Bot";

            // 设置队伍颜色
            PlayerVisual visual = bot.GetComponent<PlayerVisual>();
            if (visual != null)
            {
                visual.SetTeamColor(new Color(0.2f, 0.5f, 0.9f));
            }
        }

        Debug.Log("玩家生成完成: 红队 " + redSpawnPoints.Length + "人, 蓝队 " + blueSpawnPoints.Length + "人");
    }

    void SetupBounds()
    {
        // 创建边界对象
        GameObject bounds = new GameObject("WorldBounds");
        BoxCollider2D collider = bounds.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(boundX * 2, boundY * 2);
        collider.isTrigger = true;

        // 添加边界脚本
        bounds.AddComponent<WorldBounds>();

        Debug.Log("世界边界设置完成: " + (boundX * 2) + "x" + (boundY * 2));
    }
}

/// <summary>
/// 世界边界 - 限制玩家移动
/// </summary>
public class WorldBounds : MonoBehaviour
{
    private Vector2 bounds;

    void Start()
    {
        // 从GameManager获取边界
        if (GameManager.Instance != null)
        {
            bounds = GameManager.Instance.worldBounds;
        }
        else
        {
            bounds = new Vector2(16, 9);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 限制玩家在边界内
        Vector3 pos = other.transform.position;
        pos.x = Mathf.Clamp(pos.x, -bounds.x / 2, bounds.x / 2);
        pos.y = Mathf.Clamp(pos.y, -bounds.y / 2, bounds.y / 2);
        other.transform.position = pos;
    }
}