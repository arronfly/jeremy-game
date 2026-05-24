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
        // 创建2D击中粒子效果
        GameObject effect = new GameObject("HitEffect");
        effect.transform.position = transform.position;
        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(8);
        sr.color = new Color(1f, 0.3f, 0.1f, 0.9f);
        sr.sortingOrder = 10;

        // 0.2秒后销毁
        Destroy(effect, 0.2f);
    }

    Sprite CreateCircleSprite(int radius)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        int center = radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
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