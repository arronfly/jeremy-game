using UnityEngine;

/// <summary>
/// 玩家生命值系统 - 完整版
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 100;
    public float invulnerabilityTime = 0.5f;  // 受击后无敌时间

    [Header("视觉反馈")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;

    [Header("事件")]
    public delegate void OnDeathEvent();
    public event OnDeathEvent onDeath;

    private int currentHealth;
    private float lastDamageTime = 0f;
    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        currentHealth = maxHealth;

        // 保存原始颜色用于恢复
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int amount)
    {
        // 无敌时间检查
        if (Time.time - lastDamageTime < invulnerabilityTime)
        {
            return;
        }

        lastDamageTime = Time.time;
        currentHealth -= amount;

        Debug.Log(gameObject.name + " 受到了 " + amount + " 点伤害，剩余生命: " + currentHealth);

        // 视觉反馈 - 变红
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log(gameObject.name + " 恢复了 " + amount + " 点生命，当前: " + currentHealth);
    }

    /// <summary>
    /// 完全治愈
    /// </summary>
    public void FullHeal()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " 完全治愈!");
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    void Die()
    {
        Debug.Log(gameObject.name + " 被击杀了!");

        // 触发事件
        onDeath?.Invoke();

        // 简单处理: 隐藏并延迟重生
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 伤害闪红效果
    /// </summary>
    System.Collections.IEnumerator DamageFlash()
    {
        foreach (Renderer r in renderers)
        {
            r.material.color = damageColor;
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // 恢复原色
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取生命值百分比
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// 是否还活着
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}