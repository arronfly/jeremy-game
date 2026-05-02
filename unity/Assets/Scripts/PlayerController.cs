using UnityEngine;

/// <summary>
/// 玩家控制脚本 - 完整版
/// 控制: WASD移动, 鼠标瞄准, 左键射击, R换弹, 1-4武器, G投掷, 7-9药品
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("瞄准设置")]
    public bool aimAtMouse = true;
    public float aimRotationSpeed = 10f;

    [Header("引用 - 如果留空将自动查找")]
    public WeaponSystem weaponSystem;
    public GrenadeSystem grenadeSystem;
    public MedkitSystem medkitSystem;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private Vector2 moveDirection;
    private Vector2 lookDirection;
    private bool isSprinting = false;
    private Transform bodyTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        bodyTransform = transform.Find("Body");

        // 如果组件为空，自动获取
        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();
        if (grenadeSystem == null)
            grenadeSystem = GetComponent<GrenadeSystem>();
        if (medkitSystem == null)
            medkitSystem = GetComponent<MedkitSystem>();
    }

    void Update()
    {
        // === 移动输入 ===
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(horizontal, vertical).normalized;

        // 冲刺
        isSprinting = Input.GetKey(KeyCode.LeftShift) && moveDirection.magnitude > 0;

        // === 瞄准 ===
        if (aimAtMouse)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lookDirection = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // === 特殊动作 ===
        // 空格 - 近战攻击（如果有）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 预留：近战攻击
        }
    }

    void FixedUpdate()
    {
        // === 应用移动 ===
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector2 velocity = moveDirection * speed;

        // 平滑移动
        rb.velocity = Vector2.Lerp(rb.velocity, velocity, 0.5f);

        // 保持在世界边界内
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7.5f, 7.5f);
        pos.y = Mathf.Clamp(pos.y, -4f, 4f);
        transform.position = pos;
    }

    /// <summary>
    /// 获取移动速度（考虑加成）
    /// </summary>
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public int GetCurrentHealth()
    {
        return health != null ? health.GetCurrentHealth() : 0;
    }

    /// <summary>
    /// 是否还活着
    /// </summary>
    public bool IsAlive()
    {
        return health != null && health.IsAlive();
    }
}