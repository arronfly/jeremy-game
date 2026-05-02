using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD管理器 - 显示游戏界面
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("分数显示")]
    public Text redScoreText;
    public Text blueScoreText;
    public Text roundText;

    [Header("玩家状态")]
    public Image healthBarFill;
    public Text healthText;
    public Text ammoText;
    public Text weaponText;

    [Header("物品栏")]
    public Image[] grenadeIndicators;
    public Image[] medkitIndicators;

    [Header("击杀信息")]
    public Transform killFeedContent;
    public GameObject killFeedItemPrefab;

    [Header("引用")]
    public RoundManager roundManager;

    private PlayerHealth playerHealth;
    private WeaponSystem weaponSystem;
    private GrenadeSystem grenadeSystem;
    private MedkitSystem medkitSystem;

    void Start()
    {
        // 找到玩家组件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            weaponSystem = player.GetComponent<WeaponSystem>();
            grenadeSystem = player.GetComponent<GrenadeSystem>();
            medkitSystem = player.GetComponent<MedkitSystem>();
        }

        // 订阅回合事件
        if (roundManager != null)
        {
            roundManager.onScoreUpdate += UpdateScore;
            roundManager.onRoundStart += OnRoundStart;
            roundManager.onGameOver += OnGameOver;
        }
    }

    void Update()
    {
        UpdatePlayerStatus();
        UpdateItemIndicators();
    }

    void UpdatePlayerStatus()
    {
        // 更新生命值
        if (playerHealth != null)
        {
            int health = playerHealth.GetCurrentHealth();
            int maxHealth = playerHealth.maxHealth;
            float healthPercent = (float)health / maxHealth;

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercent;

                // 颜色变化
                if (healthPercent < 0.3f)
                    healthBarFill.color = Color.red;
                else if (healthPercent < 0.6f)
                    healthBarFill.color = Color.yellow;
                else
                    healthBarFill.color = Color.green;
            }

            if (healthText != null)
            {
                healthText.text = health + "/" + maxHealth;
            }
        }

        // 更新弹药
        if (weaponSystem != null)
        {
            if (weaponText != null)
            {
                string reloadStatus = weaponSystem.IsReloading() ? " (换弹中...)" : "";
                weaponText.text = weaponSystem.GetCurrentWeaponName() + reloadStatus;
            }

            if (ammoText != null)
            {
                ammoText.text = weaponSystem.GetCurrentAmmo() + " / " + weaponSystem.GetReserveAmmo();
            }
        }
    }

    void UpdateItemIndicators()
    {
        // 更新投掷物指示
        if (grenadeSystem != null && grenadeIndicators != null)
        {
            int currentIndex = grenadeSystem.GetCurrentGrenadeIndex();
            for (int i = 0; i < grenadeIndicators.Length; i++)
            {
                grenadeIndicators[i].color = (i == currentIndex) ? Color.white : Color.gray;
            }
        }

        // 更新药品指示
        if (medkitSystem != null && medkitIndicators != null)
        {
            // 可以根据冷却状态更新颜色
        }
    }

    public void UpdateScore(int red, int blue)
    {
        if (redScoreText != null)
            redScoreText.text = "红队: " + red;

        if (blueScoreText != null)
            blueScoreText.text = "蓝队: " + blue;
    }

    public void OnRoundStart(int round, bool suddenDeath)
    {
        if (roundText != null)
        {
            if (suddenDeath)
                roundText.text = "决胜局!";
            else
                roundText.text = "第 " + round + " 回合/7";
        }
    }

    public void OnGameOver(string winner, int[] scores)
    {
        string winnerName = (winner == "red") ? "红队" : "蓝队";
        Debug.Log(winnerName + " 胜利! 最终比分: " + scores[0] + " - " + scores[1]);

        // 可以显示胜利界面
    }

    /// <summary>
    /// 添加击杀信息到击杀播报
    /// </summary>
    public void AddKillFeed(string killer, string victim, string killerTeam)
    {
        if (killFeedContent == null || killFeedItemPrefab == null) return;

        GameObject item = Instantiate(killFeedItemPrefab, killFeedContent);

        // 设置文字
        Text[] texts = item.GetComponentsInChildren<Text>();
        if (texts.Length >= 2)
        {
            Color killerColor = (killerTeam == "red") ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.2f, 0.6f, 0.9f);
            texts[0].text = killer;
            texts[0].color = killerColor;
            texts[1].text = " 击杀 " + victim;
        }

        // 2秒后销毁
        Destroy(item, 5f);
    }
}