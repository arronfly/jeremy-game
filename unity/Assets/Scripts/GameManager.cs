using UnityEngine;

/// <summary>
/// 游戏管理器 - 全局游戏控制
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("场景设置")]
    public Camera mainCamera;
    public Vector2 worldBounds = new Vector2(16, 9);  // 游戏世界边界

    [Header("游戏状态")]
    public bool isPaused = false;
    public bool isGameActive = false;

    [Header("预制体")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject bulletPrefab;

    [Header("生成点")]
    public Vector2[] redSpawnPoints;
    public Vector2[] blueSpawnPoints;

    private void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 初始化游戏
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("游戏初始化...");

        // 确保相机存在
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        isGameActive = true;
        isPaused = false;
    }

    void Update()
    {
        // ESC 暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("游戏暂停");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("游戏继续");
        }
    }

    /// <summary>
    /// 检查点是否在游戏世界内
    /// </summary>
    public bool IsInWorldBounds(Vector3 position)
    {
        return position.x >= -worldBounds.x / 2 && position.x <= worldBounds.x / 2 &&
               position.y >= -worldBounds.y / 2 && position.y <= worldBounds.y / 2;
    }

    /// <summary>
    /// 将位置限制在游戏世界内
    /// </summary>
    public Vector3 ClampToWorldBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, -worldBounds.x / 2, worldBounds.x / 2),
            Mathf.Clamp(position.y, -worldBounds.y / 2, worldBounds.y / 2),
            position.z
        );
    }

    /// <summary>
    /// 生成玩家
    /// </summary>
    public GameObject SpawnPlayer(Vector2 position, string team)
    {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.tag = team == "red" ? "Player" : "Bot";
        player.name = team == "red" ? "Player" : "Bot_" + Random.Range(1, 100);
        return player;
    }

    /// <summary>
    /// 生成Bot
    /// </summary>
    public GameObject SpawnBot(Vector2 position, string team)
    {
        GameObject bot = Instantiate(botPrefab, position, Quaternion.identity);
        bot.tag = "Bot";
        bot.name = team + "_Bot_" + Random.Range(1, 100);
        return bot;
    }

    /// <summary>
    /// 结束游戏
    /// </summary>
    public void EndGame(string winner)
    {
        isGameActive = false;
        Debug.Log("游戏结束! 胜利者: " + winner);
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    public void RestartGame()
    {
        // 重置所有状态
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        // 可以在这里重新加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}