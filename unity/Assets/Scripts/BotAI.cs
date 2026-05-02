using UnityEngine;

/// <summary>
/// 简单的Bot AI - 测试用
/// 会向玩家移动并射击
/// </summary>
public class BotAI : MonoBehaviour
{
    [Header("AI设置")]
    public float moveSpeed = 3f;
    public float fireRate = 1f;
    public float bulletSpeed = 8f;
    public int damage = 15;
    public float detectionRange = 10f;

    [Header("引用")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player;

    private Rigidbody2D rb;
    private float lastFireTime = 0f;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // 找到玩家对象
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // 计算到玩家的方向和距离
        Vector2 toPlayer = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // 移动
        if (distance > 3f)
        {
            // 远离时移动向玩家
            moveDirection = toPlayer;
        }
        else if (distance < 2f)
        {
            // 太近时后退
            moveDirection = -toPlayer;
        }
        else
        {
            // 保持距离，原地射击
            moveDirection = Vector2.zero;
        }

        // 射击
        if (Time.time - lastFireTime > fireRate)
        {
            TryShoot(toPlayer);
            lastFireTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;

        // 确保在屏幕内
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
        transform.position = pos;
    }

    void TryShoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed, damage, gameObject.tag);
        }
    }
}