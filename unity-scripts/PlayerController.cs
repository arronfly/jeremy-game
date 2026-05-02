using UnityEngine;

/// <summary>
/// 玩家控制脚本 - 简化版测试
/// 控制: WASD移动, 鼠标瞄准, 左键射击
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;

    [Header("射击设置")]
    public float fireRate = 0.2f;
    public float bulletSpeed = 10f;
    public int damage = 20;

    [Header("引用")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Rigidbody2D rb;
    private float lastFireTime = 0f;
    private Vector2 moveDirection;
    private Vector2 lookDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        // 读取移动输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(horizontal, vertical).normalized;

        // 鼠标瞄准
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lookDirection = (mousePos - transform.position).normalized;

        // 射击
        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    void FixedUpdate()
    {
        // 应用移动
        rb.velocity = moveDirection * moveSpeed;

        // 确保玩家在屏幕内
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
        transform.position = pos;
    }

    void TryShoot()
    {
        if (Time.time - lastFireTime < fireRate) return;
        lastFireTime = Time.time;

        // 创建子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(lookDirection, bulletSpeed, damage, gameObject.tag);
        }
    }
}