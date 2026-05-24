using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键运行脚本 - 放在场景中自动完成所有设置
/// 按Play即可运行游戏，无需手动配置
/// </summary>
public class AutoSetup : MonoBehaviour
{
    [Header("一步到位模式")]
    [Tooltip("勾选后按Play自动创建完整游戏场景")]
    public bool autoSetupOnPlay = true;

    [Header("预制体 - 如果留空将自动创建")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject bulletPrefab;

    void Awake()
    {
        if (autoSetupOnPlay && !Application.isPlaying)
        {
            // 编辑器模式下直接设置
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = true;
            #endif
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            SetupCompleteGame();
        }
    }

    public void SetupCompleteGame()
    {
        Debug.Log("========================================");
        Debug.Log("    Jeremy Game - 一键游戏设置");
        Debug.Log("========================================");

        // 1. 设置相机
        SetupCamera();

        // 2. 创建游戏管理
        SetupGameManager();

        // 3. 创建回合管理
        SetupRoundManager();

        // 4. 创建HUD
        SetupHUDCanvas();

        // 5. 生成玩家
        SpawnAllPlayers();

        // 6. 设置边界
        SetupWorldBounds();

        // 7. 创建子弹管理
        SetupBulletManager();

        Debug.Log("========================================");
        Debug.Log("    游戏设置完成! 开始战斗!");
        Debug.Log("========================================");
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
        }
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        Debug.Log("[1/7] 相机设置完成");
    }

    void SetupGameManager()
    {
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
        GameManager.Instance = gmObj.GetComponent<GameManager>();
        Debug.Log("[2/7] 游戏管理器创建完成");
    }

    void SetupRoundManager()
    {
        GameObject rmObj = GameObject.Find("RoundManager");
        if (rmObj == null)
        {
            rmObj = new GameObject("RoundManager");
            rmObj.AddComponent<RoundManager>();
        }
        Debug.Log("[3/7] 回合管理器创建完成");
    }

    void SetupHUDCanvas()
    {
        // 检查是否已存在HUD
        if (GameObject.Find("GameHUD") != null)
        {
            Debug.Log("[4/7] HUD已存在");
            return;
        }

        GameObject canvasObj = new GameObject("GameHUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // 背景面板
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvasObj.transform);
        RectTransform topRect = topBar.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.anchoredPosition = Vector2.zero;
        topRect.sizeDelta = new Vector2(0, 60);

        UnityEngine.UI.Image topBg = topBar.AddComponent<UnityEngine.UI.Image>();
        topBg.color = new Color(0, 0, 0, 0.5f);

        // 红队分数
        GameObject redScore = CreateText("RedScore", "红队: 0", new Vector2(30, -30),
            TextAnchor.UpperLeft, Color.red, 24);
        redScore.transform.SetParent(topBar.transform);

        // 蓝队分数
        GameObject blueScore = CreateText("BlueScore", "蓝队: 0", new Vector2(-30, -30),
            TextAnchor.UpperRight, Color.blue, 24);
        blueScore.transform.SetParent(topBar.transform);

        // 回合信息
        GameObject roundText = CreateText("RoundText", "第 1 回合 / 7", new Vector2(0, -30),
            TextAnchor.UpperCenter, Color.white, 24);
        roundText.transform.SetParent(topBar.transform);

        // 底部状态栏
        GameObject bottomBar = new GameObject("BottomBar");
        bottomBar.transform.SetParent(canvasObj.transform);
        RectTransform bottomRect = bottomBar.AddComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(0, 0);
        bottomRect.anchoredPosition = new Vector2(100, 40);
        bottomRect.sizeDelta = new Vector2(200, 40);

        // 生命值条
        GameObject healthBarBg = new GameObject("HealthBarBg");
        healthBarBg.transform.SetParent(bottomBar.transform);
        RectTransform healthBgRect = healthBarBg.AddComponent<RectTransform>();
        healthBgRect.anchorMin = Vector2.zero;
        healthBgRect.anchorMax = Vector2.one;
        healthBgRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image healthBgImg = healthBarBg.AddComponent<UnityEngine.UI.Image>();
        healthBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject healthBarFill = new GameObject("HealthFill");
        healthBarFill.transform.SetParent(healthBarBg.transform);
        RectTransform healthFillRect = healthBarFill.AddComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = new Vector2(0.5f, 1);
        healthFillRect.anchoredPosition = Vector2.zero;
        healthFillRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image healthFillImg = healthBarFill.AddComponent<UnityEngine.UI.Image>();
        healthFillImg.color = Color.green;

        // 生命值文字
        GameObject healthText = CreateText("HP", "100/100", new Vector2(100, 0),
            TextAnchor.MiddleCenter, Color.white, 16);
        healthText.transform.SetParent(bottomBar.transform);

        // 弹药信息
        GameObject ammoBar = new GameObject("AmmoBar");
        ammoBar.transform.SetParent(canvasObj.transform);
        RectTransform ammoRect = ammoBar.AddComponent<RectTransform>();
        ammoRect.anchorMin = new Vector2(1, 0);
        ammoRect.anchorMax = new Vector2(1, 0);
        ammoRect.anchoredPosition = new Vector2(-100, 40);
        ammoRect.sizeDelta = new Vector2(200, 40);

        GameObject ammoText = CreateText("Ammo", "30 / 90", new Vector2(0, 0),
            TextAnchor.MiddleCenter, Color.yellow, 20);
        ammoText.transform.SetParent(ammoBar.transform);

        // 控制说明
        GameObject controls = new GameObject("Controls");
        controls.transform.SetParent(canvasObj.transform);
        RectTransform ctrlRect = controls.AddComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(0, 0);
        ctrlRect.anchorMax = new Vector2(1, 0);
        ctrlRect.anchoredPosition = new Vector2(0, 20);
        ctrlRect.sizeDelta = new Vector2(0, 30);

        GameObject ctrlText = CreateText("CtrlInfo",
            "WASD移动 | 鼠标瞄准 | 左键射击 | R换弹 | 1-4换武器 | G投掷 | 7-9药品 | Tab测试",
            new Vector2(0, 0), TextAnchor.MiddleCenter, Color.gray, 14);
        ctrlText.transform.SetParent(controls.transform);

        Debug.Log("[4/7] HUD界面创建完成");
    }

    GameObject CreateText(string name, string content, Vector2 pos, TextAnchor alignment, Color color, int fontSize)
    {
        GameObject go = new GameObject(name);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(200, 40);

        UnityEngine.UI.Text txt = go.AddComponent<UnityEngine.UI.Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = color;
        txt.fontStyle = FontStyle.Bold;

        return go;
    }

    void SpawnAllPlayers()
    {
        // 生成点
        Vector2[] redSpawns = new Vector2[] {
            new Vector2(-6, 2), new Vector2(-6, 0),
            new Vector2(-6, -2), new Vector2(-4, 1)
        };
        Vector2[] blueSpawns = new Vector2[] {
            new Vector2(6, -2), new Vector2(6, 0),
            new Vector2(6, 2), new Vector2(4, -1)
        };

        // 创建玩家
        for (int i = 0; i < 4; i++)
        {
            CreatePlayer(redSpawns[i], "red");
            CreateBot(blueSpawns[i], "blue");
        }

        Debug.Log("[5/7] 玩家生成完成 - 红队4人 vs 蓝队4人");
    }

    void CreatePlayer(Vector2 pos, string team)
    {
        GameObject player = CreateBasicCharacter();
        player.name = team == "red" ? "Player" : "Bot";
        player.tag = team == "red" ? "Player" : "Bot";
        player.transform.position = pos;

        // 添加玩家组件
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<WeaponSystem>();
        player.AddComponent<GrenadeSystem>();
        player.AddComponent<MedkitSystem>();
        player.AddComponent<InputManager>();
        player.AddComponent<PlayerVisual>();

        // 设置颜色
        PlayerVisual visual = player.GetComponent<PlayerVisual>();
        visual.SetTeamColor(team == "red" ? Color.red : Color.blue);

        // 设置刚体
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void CreateBot(Vector2 pos, string team)
    {
        GameObject bot = CreateBasicCharacter();
        bot.name = "Bot_" + UnityEngine.Random.Range(1, 100);
        bot.tag = "Bot";
        bot.transform.position = pos;

        // 添加Bot组件
        bot.AddComponent<PlayerController>();
        bot.AddComponent<PlayerHealth>();
        bot.AddComponent<BotAIAggressive>();
        bot.AddComponent<WeaponSystem>();
        bot.AddComponent<PlayerVisual>();

        // 设置颜色
        PlayerVisual visual = bot.GetComponent<PlayerVisual>();
        visual.SetTeamColor(Color.blue);

        // 设置刚体
        Rigidbody2D rb = bot.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    GameObject CreateBasicCharacter()
    {
        GameObject go = new GameObject();
        go.AddComponent<Rigidbody2D>();
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        return go;
    }

    void SetupWorldBounds()
    {
        GameObject bounds = new GameObject("WorldBounds");
        BoxCollider2D col = bounds.AddComponent<BoxCollider2D>();
        col.size = new Vector2(16, 9);
        col.isTrigger = true;
        bounds.AddComponent<WorldBounds>();
        Debug.Log("[6/7] 世界边界设置完成");
    }

    void SetupBulletManager()
    {
        GameObject bulletMgr = GameObject.Find("BulletManager");
        if (bulletMgr == null)
        {
            bulletMgr = new GameObject("BulletManager");
            bulletMgr.AddComponent<BulletManager>();
        }
        Debug.Log("[7/7] 子弹管理器设置完成");
    }
}