using UnityEngine;

/// <summary>
/// 预制体创建器 - 在Unity编辑器中生成完整玩家预制体
/// 使用方法: 在Project窗口右键 > Create > JeremyGame > Create Player Prefab
/// </summary>
public class PrefabCreator : MonoBehaviour
{
    [Header("队伍设置")]
    public string team = "red";  // "red" 或 "blue"

    [Header("视觉设置")]
    public Color teamColor = new Color(0.9f, 0.3f, 0.3f);

    public static void CreatePlayerPrefab()
    {
        // 创建根对象
        GameObject player = new GameObject("Player_Prefab");

        // 1. 添加刚体
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 2. 添加碰撞器
        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        // 3. 添加玩家控制脚本
        PlayerController controller = player.AddComponent<PlayerController>();
        controller.moveSpeed = 5f;
        controller.sprintMultiplier = 1.5f;

        // 4. 添加生命值系统
        PlayerHealth health = player.AddComponent<PlayerHealth>();
        health.maxHealth = 100;

        // 5. 添加武器系统
        WeaponSystem weapon = player.AddComponent<WeaponSystem>();

        // 6. 添加投掷物系统
        GrenadeSystem grenade = player.AddComponent<GrenadeSystem>();

        // 7. 添加医疗系统
        MedkitSystem medkit = player.AddComponent<MedkitSystem>();

        // 8. 添加视觉组件
        PlayerVisual visual = player.AddComponent<PlayerVisual>();
        visual.SetTeamColor(teamColor);

        // 9. 添加输入管理器
        player.AddComponent<InputManager>();

        Debug.Log("玩家预制体创建完成!");
    }

    public static void CreateBotPrefab()
    {
        GameObject bot = new GameObject("Bot_Prefab");

        // 1. 添加刚体
        Rigidbody2D rb = bot.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // 2. 添加碰撞器
        CircleCollider2D col = bot.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        // 3. 基础控制器
        bot.AddComponent<PlayerController>();

        // 4. 生命值
        PlayerHealth health = bot.AddComponent<PlayerHealth>();
        health.maxHealth = 100;

        // 5. Bot AI
        BotAIAggressive ai = bot.AddComponent<BotAIAggressive>();
        ai.viewRange = 5f;
        ai.attackRange = 3f;

        // 6. 武器
        bot.AddComponent<WeaponSystem>();

        // 7. 视觉
        PlayerVisual visual = bot.AddComponent<PlayerVisual>();
        visual.SetTeamColor(new Color(0.2f, 0.5f, 0.9f));

        Debug.Log("Bot预制体创建完成!");
    }

    public static void CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("Bullet_Prefab");

        // 添加碰撞器
        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.radius = 0.05f;
        col.isTrigger = true;

        // 添加刚体
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // 添加子弹脚本
        bullet.AddComponent<Bullet>();

        // 设置图层
        bullet.layer = LayerMask.NameToLayer("Bullet");

        Debug.Log("子弹预制体创建完成!");
    }

    [ContextMenu("Create All Prefabs")]
    public void CreateAllPrefabs()
    {
        CreatePlayerPrefab();
        CreateBotPrefab();
        CreateBulletPrefab();
    }
}

/// <summary>
/// 菜单扩展 - 方便创建预制体
/// </summary>
public static class JeremyGameMenu
{
    [UnityEditor.MenuItem("GameObject/JeremyGame/Create Player", false, 10)]
    public static void CreatePlayer()
    {
        PrefabCreator.CreatePlayerPrefab();
    }

    [UnityEditor.MenuItem("GameObject/JeremyGame/Create Bot", false, 11)]
    public static void CreateBot()
    {
        PrefabCreator.CreateBotPrefab();
    }

    [UnityEditor.MenuItem("GameObject/JeremyGame/Create Bullet", false, 12)]
    public static void CreateBullet()
    {
        PrefabCreator.CreateBulletPrefab();
    }

    [UnityEditor.MenuItem("GameObject/JeremyGame/Quick Setup Scene", false, 20)]
    public static void CreateQuickSetup()
    {
        GameObject qs = new GameObject("QuickSetup");
        qs.AddComponent<QuickSetup>();
        UnityEditor.Selection.activeGameObject = qs;
    }

    [UnityEditor.MenuItem("GameObject/JeremyGame/Create All Prefabs", false, 30)]
    public static void CreateAllPrefabs()
    {
        PrefabCreator prefab = new GameObject().AddComponent<PrefabCreator>();
        prefab.CreateAllPrefabs();
        DestroyImmediate(prefab.gameObject);
    }
}