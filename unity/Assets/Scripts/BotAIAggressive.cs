using UnityEngine;

/// <summary>
/// Bot AI 状态机
/// </summary>
public enum BotState
{
    Idle,      // 待机
    Patrol,    // 巡逻
    Chase,     // 追击
    Attack,    // 攻击
    Retreat,   // 撤退
    UseItem    // 使用物品
}

/// <summary>
/// 高级Bot AI - 使用状态机
/// </summary>
public class BotAIAggressive : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3.5f;
    public float chaseSpeed = 4.5f;
    public float retreatSpeed = 3f;

    [Header("攻击设置")]
    public float attackRange = 8f;
    public float chaseRange = 12f;
    public float fireRate = 0.5f;
    public float accuracy = 0.8f;

    [Header("行为设置")]
    public float patrolRadius = 5f;
    public float retreatThreshold = 0.3f;  // 生命值低于30%时撤退
    public float itemUseThreshold = 0.5f; // 生命值低于50%时可能使用药品

    [Header("引用")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("巡逻点")]
    public Vector2[] patrolPoints;

    private BotState currentState = BotState.Idle;
    private Transform targetPlayer;
    private Vector3 originPosition;
    private Vector2 currentPatrolTarget;
    private int currentPatrolIndex = 0;
    private float lastFireTime = 0f;
    private float lastStateChange = 0f;
    private float stuckTime = 0f;
    private Vector3 lastPosition;

    private Rigidbody2D rb;
    private PlayerHealth health;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        originPosition = transform.position;

        // 初始化巡逻点
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            patrolPoints = new Vector2[]
            {
                originPosition + new Vector3(patrolRadius, 0, 0),
                originPosition + new Vector3(-patrolRadius, 0, 0),
                originPosition + new Vector3(0, patrolRadius, 0),
                originPosition + new Vector3(0, -patrolRadius, 0)
            };
        }

        currentPatrolTarget = patrolPoints[0];
        lastPosition = transform.position;
    }

    void Update()
    {
        // 查找最近的目标玩家
        FindTarget();

        // 状态机更新
        UpdateState();

        // 检查是否卡住
        CheckIfStuck();
    }

    void FindTarget()
    {
        // 找到所有玩家和Bot
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        System.Action<GameObject[]> checkTargets = (targets) =>
        {
            foreach (GameObject target in targets)
            {
                if (target == gameObject) continue;
                PlayerHealth health = target.GetComponent<PlayerHealth>();
                if (health == null || !health.IsAlive()) continue;

                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = target.transform;
                }
            }
        };

        checkTargets(players);
        checkTargets(bots);

        targetPlayer = closest;
    }

    void UpdateState()
    {
        float healthPercent = health != null ? health.GetCurrentHealth() / 100f : 1f;

        // 状态转换逻辑
        BotState newState = currentState;

        // 生命值过低，撤退
        if (healthPercent <= retreatThreshold)
        {
            newState = BotState.Retreat;
        }
        // 有目标且在攻击范围内
        else if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);

            if (dist <= attackRange)
            {
                newState = BotState.Attack;
            }
            else if (dist <= chaseRange)
            {
                newState = BotState.Chase;
            }
            else
            {
                newState = BotState.Patrol;
            }
        }
        else
        {
            newState = BotState.Patrol;
        }

        // 状态改变
        if (newState != currentState)
        {
            currentState = newState;
            lastStateChange = Time.time;
            Debug.Log(gameObject.name + " 进入状态: " + currentState);
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case BotState.Idle:
                Idle();
                break;
            case BotState.Patrol:
                Patrol();
                break;
            case BotState.Chase:
                Chase();
                break;
            case BotState.Attack:
                Attack();
                break;
            case BotState.Retreat:
                Retreat();
                break;
        }
    }

    void Idle()
    {
        // 原地等待
        rb.velocity = Vector2.zero;

        // 一段时间后开始巡逻
        if (Time.time - lastStateChange > 2f)
        {
            currentState = BotState.Patrol;
        }
    }

    void Patrol()
    {
        // 移动到当前巡逻点
        Vector2 direction = (currentPatrolTarget - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, currentPatrolTarget);

        if (distance < 0.5f)
        {
            // 到达巡逻点，切换到下一个
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            currentPatrolTarget = patrolPoints[currentPatrolIndex];
        }

        rb.velocity = direction * moveSpeed;
    }

    void Chase()
    {
        if (targetPlayer == null) return;

        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;

        // 保持一定距离，不靠太近
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance < 3f)
        {
            rb.velocity = -direction * chaseSpeed * 0.5f;
        }
    }

    void Attack()
    {
        if (targetPlayer == null) return;

        Vector2 toPlayer = (targetPlayer.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        // 瞄准玩家
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x);
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        // 保持最佳攻击距离
        float optimalDistance = 5f;
        if (distance > optimalDistance)
        {
            rb.velocity = toPlayer * moveSpeed;
        }
        else if (distance < optimalDistance - 1f)
        {
            rb.velocity = -toPlayer * moveSpeed * 0.5f;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        // 射击
        if (Time.time - lastFireTime > fireRate)
        {
            TryShoot();
            lastFireTime = Time.time + Random.Range(-0.1f, 0.1f);
        }
    }

    void Retreat()
    {
        if (targetPlayer == null) return;

        // 向远离玩家的方向移动
        Vector2 direction = (transform.position - targetPlayer.position).normalized;
        rb.velocity = direction * retreatSpeed;

        // 保持安全距离
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance > 8f && health != null && health.GetCurrentHealth() > 50)
        {
            currentState = BotState.Attack;
        }
    }

    void TryShoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        if (Time.time < lastFireTime) return;

        // 计算射击方向（有一定误差）
        Vector2 direction = (targetPlayer.position - firePoint.position).normalized;
        float spreadAngle = Random.Range(-10f, 10f) * (1f - accuracy);
        float angle = Mathf.Atan2(direction.y, direction.x) + spreadAngle * Mathf.Deg2Rad;

        Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 10f;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg));
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(velocity, 15, gameObject.tag);
        }
    }

    void CheckIfStuck()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime > 1f)
            {
                // 随机移动摆脱卡住
                Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                rb.velocity = randomDir * moveSpeed;
                stuckTime = 0f;
            }
        }
        else
        {
            stuckTime = 0f;
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// 被击中时调用
    /// </summary>
    public void OnHit(int damage)
    {
        // 被击中后更积极地追击
        if (currentState == BotState.Patrol || currentState == BotState.Idle)
        {
            currentState = BotState.Chase;
        }
    }
}