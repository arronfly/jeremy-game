using UnityEngine;

/// <summary>
/// 玩家生命值系统 - 测试用
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " 受到了 " + amount + " 点伤害，剩余生命: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 被击杀了!");
        // 简单的处理: 0.5秒后重生
        Invoke("Respawn", 0.5f);
    }

    void Respawn()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " 已重生!");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}