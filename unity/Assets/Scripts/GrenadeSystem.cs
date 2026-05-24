using UnityEngine;
using System.Collections;

/// <summary>
/// 投掷物类型
/// </summary>
public enum GrenadeType
{
    Smoke,   // 烟雾弹
    Flash,   // 闪光弹
    Frag     // 手雷
}

/// <summary>
/// 投掷物数据
/// </summary>
[System.Serializable]
public class GrenadeData
{
    public GrenadeType type;
    public float cooldown = 3f;
    public float throwForce = 15f;
    public float fuseTime = 1.5f;

    // 特定类型参数
    public float radius = 5f;        // 范围
    public float duration = 5f;      // 持续时间
    public int damage = 50;          // 伤害（仅手雷）
}

/// <summary>
/// 投掷物系统
/// </summary>
public class GrenadeSystem : MonoBehaviour
{
    [Header("投掷物配置")]
    public GrenadeData[] grenadeTypes = new GrenadeData[]
    {
        new GrenadeData() { type = GrenadeType.Smoke, cooldown = 3f, throwForce = 12f, fuseTime = 1f, radius = 5f, duration = 5f },
        new GrenadeData() { type = GrenadeType.Flash, cooldown = 3f, throwForce = 12f, fuseTime = 0.5f, radius = 4f, duration = 2f },
        new GrenadeData() { type = GrenadeType.Frag, cooldown = 3f, throwForce = 15f, fuseTime = 1.5f, radius = 3f, duration = 0f, damage = 50 }
    };

    [Header("引用")]
    public GameObject smokeEffectPrefab;
    public GameObject flashEffectPrefab;
    public GameObject fragEffectPrefab;

    private int currentGrenadeIndex = 0;
    private float lastThrowTime = 0f;
    private bool isThrowing = false;
    private float chargeStartTime = 0f;

    void Update()
    {
        // G键 切换投掷物
        if (Input.GetKeyDown(KeyCode.G))
        {
            SwitchGrenade();
        }

        // 鼠标右键按住蓄力
        if (Input.GetMouseButtonDown(1))
        {
            isThrowing = true;
            chargeStartTime = Time.time;
        }

        // 鼠标右键释放投掷
        if (Input.GetMouseButtonUp(1) && isThrowing)
        {
            Throw();
            isThrowing = false;
        }
    }

    void SwitchGrenade()
    {
        currentGrenadeIndex = (currentGrenadeIndex + 1) % grenadeTypes.Length;
        Debug.Log("切换投掷物: " + grenadeTypes[currentGrenadeIndex].type);
    }

    void Throw()
    {
        GrenadeData grenade = grenadeTypes[currentGrenadeIndex];

        // 检查冷却
        if (Time.time - lastThrowTime < grenade.cooldown)
        {
            Debug.Log("投掷物冷却中...");
            return;
        }

        lastThrowTime = Time.time;

        // 计算投掷力度（按住时间越长，投得越远）
        float holdTime = Time.time - chargeStartTime;
        float forceMultiplier = Mathf.Clamp(holdTime / 2f, 0.5f, 2f);

        // 获取投掷方向（向鼠标位置）
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        Vector2 velocity = direction * grenade.throwForce * forceMultiplier;

        // 创建投掷物
        Vector3 spawnPos = transform.position + (Vector3)direction * 0.5f;
        GameObject grenadeObj = Instantiate(GetGrenadePrefab(grenade.type), spawnPos, Quaternion.identity);

        Rigidbody2D rb = grenadeObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }

        // 设置引爆
        StartCoroutine(DetonateGrenade(grenadeObj, grenade));

        Debug.Log("投掷: " + grenade.type + " 力度: " + forceMultiplier);
    }

    GameObject GetGrenadePrefab(GrenadeType type)
    {
        switch (type)
        {
            case GrenadeType.Smoke: return smokeEffectPrefab;
            case GrenadeType.Flash: return flashEffectPrefab;
            case GrenadeType.Frag: return fragEffectPrefab;
            default: return fragEffectPrefab;
        }
    }

    IEnumerator DetonateGrenade(GameObject grenade, GrenadeData data)
    {
        yield return new WaitForSeconds(data.fuseTime);

        // 根据类型触发效果
        switch (data.type)
        {
            case GrenadeType.Smoke:
                CreateSmokeEffect(grenade.transform.position, data.duration);
                break;
            case GrenadeType.Flash:
                CreateFlashEffect(grenade.transform.position, data.radius, data.duration);
                break;
            case GrenadeType.Frag:
                CreateFragExplosion(grenade.transform.position, data.radius, data.damage);
                break;
        }

        Destroy(grenade);
    }

    void CreateSmokeEffect(Vector3 position, float duration)
    {
        Debug.Log("创建烟雾区域");
        // 创建烟雾视觉效果
        GameObject smoke = Instantiate(smokeEffectPrefab, position, Quaternion.identity);
        Destroy(smoke, duration);
    }

    void CreateFlashEffect(Vector3 position, float radius, float duration)
    {
        Debug.Log("闪光弹爆炸! 致盲范围: " + radius);

        // 检查范围内的敌人并致盲
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null && !hit.CompareTag(gameObject.tag))
            {
                // 致盲效果（这里简化处理）
                Debug.Log(hit.name + " 被致盲!");
            }
        }
    }

    void CreateFragExplosion(Vector3 position, float radius, int damage)
    {
        Debug.Log("手雷爆炸! 伤害范围: " + radius + " 伤害: " + damage);

        // 范围内造成伤害
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null && !hit.CompareTag(gameObject.tag))
            {
                health.TakeDamage(damage);
            }
        }

        // 创建爆炸视觉效果
        GameObject explosion = Instantiate(fragEffectPrefab, position, Quaternion.identity);
        Destroy(explosion, 0.5f);
    }

    public int GetCurrentGrenadeIndex()
    {
        return currentGrenadeIndex;
    }

    public GrenadeType GetCurrentGrenadeType()
    {
        return grenadeTypes[currentGrenadeIndex].type;
    }
}