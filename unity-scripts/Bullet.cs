using UnityEngine;

/// <summary>
/// 子弹 - 支持多种武器
/// </summary>
public class Bullet : MonoBehaviour
{
    private Vector2 velocity;
    private int damage;
    private string ownerTag;
    private Rigidbody2D rb;
    private bool initialized = false;
    private float lifetime = 3f;
    private float createdTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 初始化子弹
    /// </summary>
    public void Initialize(Vector2 velocity, int damage, string ownerTag)
    {
        this.velocity = velocity;
        this.damage = damage;
        this.ownerTag = ownerTag;
        this.createdTime = Time.time;
        this.initialized = true;

        rb.velocity = velocity;
        rb.rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    }

    // 兼容旧版本
    public void Initialize(Vector2 direction, float speed, int damage, string tag)
    {
        Initialize(direction * speed, damage, tag);
    }

    void Update()
    {
        if (!initialized) return;

        // 生命周期结束
        if (Time.time - createdTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 碰到任何物体都销毁
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        // 忽略碰撞的标签
        if (other.CompareTag(ownerTag)) return;

        // 检测玩家或Bot
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            // 伤害判定
            health.TakeDamage(damage);

            // 击中效果
            CreateHitEffect();
        }

        Destroy(gameObject);
    }

    void CreateHitEffect()
    {
        // 创建击中粒子效果（可以用简单的几何图形代替）
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.2f;
        effect.GetComponent<Renderer>().material.color = Color.red;

        // 0.2秒后销毁
        Destroy(effect, 0.2f);
    }

    public int GetDamage()
    {
        return damage;
    }

    public string GetOwnerTag()
    {
        return ownerTag;
    }
}