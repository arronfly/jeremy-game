using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏启动器 - GamePlay场景的唯一入口
/// 按顺序初始化所有系统并启动游戏
/// 将此脚本挂载到场景中一个空GameObject上即可运行
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("玩家设置")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject bulletPrefab;

    [Header("世界设置")]
    public float mapHalfWidth = 800f;
    public float mapHalfHeight = 600f;

    // === 管理器引用 ===
    private SpriteManager spriteManager;
    private AudioManager audioManager;
    private GarageMapBuilder mapBuilder;
    private GameManager gameManager;
    private RoundManager roundManager;
    private TeamManager teamManager;
    private BulletManager bulletManager;
    private HUDManager hudManager;
    private CameraFollow cameraFollow;
    private CameraShake cameraShake;
    private EffectManager effectManager;
    private PauseMenu pauseMenu;

    // === 运行时对象 ===
    private GameObject player;
    private GameObject[] allPlayers = new GameObject[8];

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("  歼灭团竞2 - 游戏启动中...");
        Debug.Log("========================================");

        // 按依赖顺序初始化
        InitializeManagers();
        BuildWorld();
        SpawnAllPlayers();
        SetupHUD();
        SetupCamera();
        StartFirstRound();

        Debug.Log("========================================");
        Debug.Log("  游戏就绪! 开始战斗!");
        Debug.Log("========================================");
    }

    // =========================================================================
    //  管理器初始化
    // =========================================================================

    void InitializeManagers()
    {
        // 1. SpriteManager (所有视觉资源的基础)
        GameObject spriteMgrObj = new GameObject("SpriteManager");
        spriteMgrObj.transform.SetParent(transform);
        spriteManager = spriteMgrObj.AddComponent<SpriteManager>();
        // SpriteManager.GenerateAllSprites() 在 Start() 自动调用
        // 等一帧确保 sprites 生成完毕
        Debug.Log("[Bootstrapper] SpriteManager 初始化");

        // 2. AudioManager (音效系统)
        GameObject audioMgrObj = new GameObject("AudioManager");
        audioMgrObj.transform.SetParent(transform);
        audioManager = audioMgrObj.AddComponent<AudioManager>();
        Debug.Log("[Bootstrapper] AudioManager 初始化");

        // 3. EffectManager (特效系统)
        GameObject effectMgrObj = new GameObject("EffectManager");
        effectMgrObj.transform.SetParent(transform);
        effectManager = effectMgrObj.AddComponent<EffectManager>();
        Debug.Log("[Bootstrapper] EffectManager 初始化");

        // 4. GameManager (全局状态)
        GameObject gameMgrObj = new GameObject("GameManager");
        gameMgrObj.transform.SetParent(transform);
        gameManager = gameMgrObj.AddComponent<GameManager>();
        Debug.Log("[Bootstrapper] GameManager 初始化");

        // 5. TeamManager (队伍管理)
        GameObject teamMgrObj = new GameObject("TeamManager");
        teamMgrObj.transform.SetParent(transform);
        teamManager = teamMgrObj.AddComponent<TeamManager>();
        Debug.Log("[Bootstrapper] TeamManager 初始化");

        // 6. BulletManager (对象池)
        GameObject bulletMgrObj = new GameObject("BulletManager");
        bulletMgrObj.transform.SetParent(transform);
        bulletManager = bulletMgrObj.AddComponent<BulletManager>();
        if (bulletPrefab != null)
        {
            bulletManager.bulletPrefab = bulletPrefab;
        }
        Debug.Log("[Bootstrapper] BulletManager 初始化");

        // 7. RoundManager (回合逻辑)
        GameObject roundMgrObj = new GameObject("RoundManager");
        roundMgrObj.transform.SetParent(transform);
        roundManager = roundMgrObj.AddComponent<RoundManager>();
        Debug.Log("[Bootstrapper] RoundManager 初始化");
    }

    // =========================================================================
    //  世界构建
    // =========================================================================

    void BuildWorld()
    {
        // 等待 SpriteManager 完成
        if (spriteManager != null && !spriteManager.IsReady())
        {
            Debug.LogWarning("[Bootstrapper] SpriteManager 未就绪, 使用默认 sprites");
        }

        // 构建地图
        GameObject mapObj = new GameObject("Map");
        mapBuilder = mapObj.AddComponent<GarageMapBuilder>();
        mapBuilder.BuildMap();
        Debug.Log("[Bootstrapper] 地图构建完成");

        // 设置游戏世界边界
        if (gameManager != null)
        {
            gameManager.worldBounds = new Vector2(mapHalfWidth * 2, mapHalfHeight * 2);
        }
    }

    // =========================================================================
    //  玩家生成
    // =========================================================================

    void SpawnAllPlayers()
    {
        string playerTeam = TeamSelect.selectedTeam;
        string enemyTeam = (playerTeam == "red") ? "blue" : "red";

        Vector2[] redSpawns = (mapBuilder != null) ? mapBuilder.redSpawnPoints : DefaultRedSpawns();
        Vector2[] blueSpawns = (mapBuilder != null) ? mapBuilder.blueSpawnPoints : DefaultBlueSpawns();

        bool playerIsRed = (playerTeam == "red");
        Vector2[] teamSpawns = playerIsRed ? redSpawns : blueSpawns;
        Vector2[] enemySpawns = playerIsRed ? blueSpawns : redSpawns;

        // 生成玩家 (队伍的第一个位置)
        player = SpawnPlayer(teamSpawns[0], playerTeam, true);
        allPlayers[0] = player;

        // 生成3个友方Bot
        for (int i = 1; i < 4; i++)
        {
            GameObject allyBot = SpawnBot(teamSpawns[i % teamSpawns.Length], playerTeam, false);
            allPlayers[i] = allyBot;
        }

        // 生成4个敌方Bot
        for (int i = 0; i < 4; i++)
        {
            GameObject enemyBot = SpawnBot(enemySpawns[i % enemySpawns.Length], enemyTeam, false);
            allPlayers[4 + i] = enemyBot;
        }

        // 设置 TeamManager
        if (teamManager != null)
        {
            teamManager.playerTeam = playerTeam;
            teamManager.enemyTeam = enemyTeam;

            // 分配队伍数组给 RoundManager
            if (roundManager != null)
            {
                if (playerIsRed)
                {
                    roundManager.redTeamPlayers = new GameObject[] { allPlayers[0], allPlayers[1], allPlayers[2], allPlayers[3] };
                    roundManager.blueTeamPlayers = new GameObject[] { allPlayers[4], allPlayers[5], allPlayers[6], allPlayers[7] };
                }
                else
                {
                    roundManager.blueTeamPlayers = new GameObject[] { allPlayers[0], allPlayers[1], allPlayers[2], allPlayers[3] };
                    roundManager.redTeamPlayers = new GameObject[] { allPlayers[4], allPlayers[5], allPlayers[6], allPlayers[7] };
                }
            }
        }

        Debug.Log("[Bootstrapper] 玩家生成完成 - " + playerTeam + "队 (玩家+3Bot vs 4Bot)");
    }

    GameObject SpawnPlayer(Vector2 position, string team, bool isHuman)
    {
        GameObject obj;
        if (playerPrefab != null)
        {
            obj = Instantiate(playerPrefab, position, Quaternion.identity);
        }
        else
        {
            obj = CreatePlayerObject(position);
        }

        obj.name = isHuman ? "Player" : team + "_Bot_" + Random.Range(1, 100);
        obj.tag = isHuman ? "Player" : "Bot";

        // 设置队伍颜色
        SetupTeamVisual(obj, team);

        // 设置武器系统
        SetupWeaponSystem(obj);

        // 设置Bot AI
        if (!isHuman)
        {
            SetupBotAI(obj);
        }

        return obj;
    }

    GameObject SpawnBot(Vector2 position, string team, bool isHuman)
    {
        return SpawnPlayer(position, team, isHuman);
    }

    /// <summary>
    /// 创建玩家对象 (无预制体时的后备方案)
    /// </summary>
    GameObject CreatePlayerObject(Vector2 position)
    {
        GameObject obj = new GameObject("Player");
        obj.transform.position = position;

        // 添加必要的组件
        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        obj.AddComponent<PlayerHealth>();
        obj.AddComponent<PlayerVisual>();
        obj.AddComponent<PlayerController>();

        return obj;
    }

    void SetupTeamVisual(GameObject obj, string team)
    {
        PlayerVisual visual = obj.GetComponent<PlayerVisual>();
        if (visual != null)
        {
            Color teamColor = (team == "red")
                ? new Color(0.9f, 0.25f, 0.25f, 1f)
                : new Color(0.2f, 0.55f, 0.9f, 1f);
            visual.SetTeamColor(teamColor);
        }
    }

    void SetupWeaponSystem(GameObject obj)
    {
        WeaponSystem weaponSys = obj.GetComponent<WeaponSystem>();
        if (weaponSys == null)
        {
            weaponSys = obj.AddComponent<WeaponSystem>();
        }

        // 设置弹药预制体
        if (bulletPrefab != null && weaponSys.bulletPrefabs != null)
        {
            for (int i = 0; i < weaponSys.bulletPrefabs.Length; i++)
            {
                weaponSys.bulletPrefabs[i] = bulletPrefab;
            }
        }
    }

    void SetupBotAI(GameObject obj)
    {
        // 优先使用 BotAIAggressive (状态机版本)
        BotAIAggressive ai = obj.AddComponent<BotAIAggressive>();

        // 设置子弹预制体
        if (bulletPrefab != null)
        {
            ai.bulletPrefab = bulletPrefab;
        }

        // 应用难度设置
        ai.accuracy = DifficultyManager.botAccuracy;
    }

    Vector2[] DefaultRedSpawns()
    {
        return new Vector2[]
        {
            new Vector2(-650, 300),
            new Vector2(-650, 100),
            new Vector2(-650, -100),
            new Vector2(-650, -300)
        };
    }

    Vector2[] DefaultBlueSpawns()
    {
        return new Vector2[]
        {
            new Vector2(650, 300),
            new Vector2(650, 100),
            new Vector2(650, -100),
            new Vector2(650, -300)
        };
    }

    // =========================================================================
    //  HUD 设置
    // =========================================================================

    void SetupHUD()
    {
        GameObject hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(transform);
        hudManager = hudObj.AddComponent<HUDManager>();
        Debug.Log("[Bootstrapper] HUD 创建完成");

        // 暂停菜单
        GameObject pauseObj = new GameObject("PauseMenu");
        pauseObj.transform.SetParent(transform);
        pauseMenu = pauseObj.AddComponent<PauseMenu>();
    }

    // =========================================================================
    //  相机设置
    // =========================================================================

    void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        // 设置正交相机
        mainCam.orthographic = true;
        mainCam.orthographicSize = 10f;
        mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        mainCam.transform.position = new Vector3(0, 0, -10f);

        // 添加跟随脚本
        cameraFollow = mainCam.gameObject.AddComponent<CameraFollow>();
        if (player != null)
        {
            cameraFollow.SetTarget(player.transform);
        }
        cameraFollow.SetMapBounds(mapHalfWidth, mapHalfHeight);

        // 添加震动脚本
        cameraShake = mainCam.gameObject.AddComponent<CameraShake>();

        Debug.Log("[Bootstrapper] 相机设置完成");
    }

    // =========================================================================
    //  回合启动
    // =========================================================================

    void StartFirstRound()
    {
        if (roundManager != null)
        {
            // RoundManager.Start() 会自动调用 StartRound()
            Debug.Log("[Bootstrapper] 回合系统就绪");
        }
    }

    // =========================================================================
    //  公共接口
    // =========================================================================

    /// <summary>
    /// 获取玩家对象
    /// </summary>
    public GameObject GetPlayer() => player;

    /// <summary>
    /// 获取所有玩家
    /// </summary>
    public GameObject[] GetAllPlayers() => allPlayers;

    /// <summary>
    /// 获取队伍管理器
    /// </summary>
    public TeamManager GetTeamManager() => teamManager;

    /// <summary>
    /// 获取回合管理器
    /// </summary>
    public RoundManager GetRoundManager() => roundManager;
}
