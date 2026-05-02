using UnityEngine;

/// <summary>
/// 子弹脚本 - 测试用
/// </summary>
public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private string ownerTag;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 dir, float spd, int dmg, string tag)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        ownerTag = tag;

        rb.velocity = direction * speed;

        // 3秒后自动销毁
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 碰到其他物体就销毁
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果碰到玩家（不是子弹主人的队伍）
        if (other.CompareTag("Player") && !other.gameObject.CompareTag(ownerTag))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}