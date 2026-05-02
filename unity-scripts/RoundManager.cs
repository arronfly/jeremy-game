using UnityEngine;

/// <summary>
/// 回合管理器 - 处理回合逻辑和胜负判断
/// </summary>
public class RoundManager : MonoBehaviour
{
    [Header("游戏设置")]
    public int winScore = 4;
    public int maxRounds = 7;
    public float respawnDelay = 3f;
    public float roundEndDelay = 2f;

    [Header("引用")]
    public GameObject[] redTeamPlayers;
    public GameObject[] blueTeamPlayers;

    [Header("状态")]
    public int currentRound = 1;
    public int redScore = 0;
    public int blueScore = 0;
    public bool isRoundActive = false;
    public bool isGameOver = false;
    public bool isSuddenDeath = false;

    public delegate void OnRoundStart(int round, bool suddenDeath);
    public delegate void OnRoundEnd(string winner, int[] scores);
    public delegate void OnScoreUpdate(int red, int blue);
    public delegate void OnGameOver(string winner, int[] scores);

    public event OnRoundStart onRoundStart;
    public event OnRoundEnd onRoundEnd;
    public event OnScoreUpdate onScoreUpdate;
    public event OnGameOver onGameOver;

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        if (isGameOver) return;

        // 检查回合是否结束
        if (isRoundActive)
        {
            CheckRoundEnd();
        }
    }

    /// <summary>
    /// 开始新回合
    /// </summary>
    public void StartRound()
    {
        if (isGameOver) return;

        Debug.Log("=== 第 " + currentRound + " 回合开始 ===");

        // 重生所有玩家
        RespawnAllPlayers();

        isRoundActive = true;

        // 触发事件
        onRoundStart?.Invoke(currentRound, isSuddenDeath);
    }

    /// <summary>
    /// 检查回合是否结束
    /// </summary>
    void CheckRoundEnd()
    {
        int redAlive = GetAliveCount(redTeamPlayers);
        int blueAlive = GetAliveCount(blueTeamPlayers);

        if (redAlive == 0 || blueAlive == 0)
        {
            EndRound(redAlive == 0 ? "blue" : "red");
        }
    }

    int GetAliveCount(GameObject[] players)
    {
        int count = 0;
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null && health.GetCurrentHealth() > 0)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 结束回合
    /// </summary>
    void EndRound(string winner)
    {
        isRoundActive = false;

        if (winner == "red")
        {
            redScore++;
            Debug.Log("红队获胜!");
        }
        else
        {
            blueScore++;
            Debug.Log("蓝队获胜!");
        }

        onScoreUpdate?.Invoke(redScore, blueScore);

        // 检查是否游戏结束
        if (CheckGameOver())
        {
            return;
        }

        // 进入下一回合
        currentRound++;
        if (currentRound > maxRounds)
        {
            currentRound = 1;
        }

        // 3:3 时进入决胜局
        if (redScore == 3 && blueScore == 3)
        {
            isSuddenDeath = true;
            Debug.Log("=== 决胜局 ===");
        }

        // 延迟开始下一回合
        Invoke("StartRound", roundEndDelay);
    }

    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    bool CheckGameOver()
    {
        if (redScore >= winScore)
        {
            isGameOver = true;
            Debug.Log("===== 红队胜利! =====");
            Debug.Log("最终比分: " + redScore + " - " + blueScore);
            onGameOver?.Invoke("red", new int[] { redScore, blueScore });
            return true;
        }
        else if (blueScore >= winScore)
        {
            isGameOver = true;
            Debug.Log("===== 蓝队胜利! =====");
            Debug.Log("最终比分: " + redScore + " - " + blueScore);
            onGameOver?.Invoke("blue", new int[] { redScore, blueScore });
            return true;
        }
        return false;
    }

    /// <summary>
    /// 重生所有玩家
    /// </summary>
    void RespawnAllPlayers()
    {
        // 重生红队 (左边)
        float redSpawnX = -6f;
        for (int i = 0; i < redTeamPlayers.Length; i++)
        {
            if (redTeamPlayers[i] != null)
            {
                Vector3 spawnPos = new Vector3(redSpawnX, -2f + i * 1.5f, 0);
                RespawnPlayer(redTeamPlayers[i], spawnPos);
            }
        }

        // 重生蓝队 (右边)
        float blueSpawnX = 6f;
        for (int i = 0; i < blueTeamPlayers.Length; i++)
        {
            if (blueTeamPlayers[i] != null)
            {
                Vector3 spawnPos = new Vector3(blueSpawnX, -2f + i * 1.5f, 0);
                RespawnPlayer(blueTeamPlayers[i], spawnPos);
            }
        }
    }

    void RespawnPlayer(GameObject player, Vector3 position)
    {
        player.transform.position = position;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.FullHeal();
        }

        // 激活玩家
        player.SetActive(true);
    }

    /// <summary>
    /// 获取当前比分
    /// </summary>
    public int[] GetScores()
    {
        return new int[] { redScore, blueScore };
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    public void ResetGame()
    {
        currentRound = 1;
        redScore = 0;
        blueScore = 0;
        isRoundActive = false;
        isGameOver = false;
        isSuddenDeath = false;

        StartRound();
    }
}