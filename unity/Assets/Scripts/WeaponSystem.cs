using UnityEngine;

/// <summary>
/// 武器配置数据
/// </summary>
[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public int damage;
    public float fireRate;        // 秒
    public int magazineSize;
    public int maxReserveAmmo;
    public float spread;          // 子弹扩散角度
    public int pellets;           // 散弹子弹数
    public float reloadTime;      // 秒
    public float bulletSpeed;     // 子弹速度
}

/// <summary>
/// 武器系统 - 管理武器切换、射击、换弹
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("武器数据")]
    public WeaponData[] weapons = new WeaponData[]
    {
        new WeaponData() { weaponName = "突击步枪", damage = 25, fireRate = 0.2f, magazineSize = 30, maxReserveAmmo = 120, spread = 0.05f, pellets = 1, reloadTime = 1.5f, bulletSpeed = 20f },
        new WeaponData() { weaponName = "狙击枪", damage = 80, fireRate = 1.5f, magazineSize = 5, maxReserveAmmo = 20, spread = 0f, pellets = 1, reloadTime = 2.5f, bulletSpeed = 30f },
        new WeaponData() { weaponName = "冲锋枪", damage = 18, fireRate = 0.1f, magazineSize = 35, maxReserveAmmo = 140, spread = 0.1f, pellets = 1, reloadTime = 1.5f, bulletSpeed = 15f },
        new WeaponData() { weaponName = "散弹枪", damage = 15, fireRate = 0.8f, magazineSize = 8, maxReserveAmmo = 32, spread = 0.3f, pellets = 8, reloadTime = 2f, bulletSpeed = 18f }
    };

    [Header("引用")]
    public GameObject[] bulletPrefabs;  // 对应每种武器的子弹
    public Transform firePoint;

    private int currentWeaponIndex = 0;
    private int currentAmmo;
    private int reserveAmmo;
    private float lastFireTime = 0f;
    private bool isReloading = false;

    void Start()
    {
        EquipWeapon(0);
    }

    void Update()
    {
        // 武器切换 (1-4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);

        // 换弹 (R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        // 射击 (鼠标左键)
        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        currentWeaponIndex = index;
        currentAmmo = weapons[index].magazineSize;
        reserveAmmo = weapons[index].maxReserveAmmo;
        isReloading = false;

        Debug.Log("切换武器: " + weapons[index].weaponName);
    }

    public void TryShoot()
    {
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        WeaponData weapon = weapons[currentWeaponIndex];
        float timeSinceLastFire = Time.time - lastFireTime;

        if (timeSinceLastFire < weapon.fireRate) return;

        lastFireTime = Time.time;
        currentAmmo--;

        // 创建子弹
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        FireBullets(direction, weapon);

        Debug.Log(weapon.weaponName + " 射击! 剩余弹药: " + currentAmmo + "/" + reserveAmmo);
    }

    void FireBullets(Vector2 direction, WeaponData weapon)
    {
        int pellets = weapon.pellets;
        float spread = weapon.spread;

        for (int i = 0; i < pellets; i++)
        {
            float angle = Mathf.Atan2(direction.y, direction.x);
            float spreadAngle = angle + Random.Range(-spread, spread);

            Vector2 velocity = new Vector2(Mathf.Cos(spreadAngle), Mathf.Sin(spreadAngle)) * weapon.bulletSpeed;

            GameObject bullet = Instantiate(
                bulletPrefabs[currentWeaponIndex],
                firePoint.position,
                Quaternion.Euler(0, 0, Mathf.Rad2Deg * spreadAngle)
            );

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(velocity, weapon.damage, gameObject.tag);
            }
        }
    }

    public void Reload()
    {
        if (isReloading) return;
        if (currentAmmo >= weapons[currentWeaponIndex].magazineSize) return;
        if (reserveAmmo <= 0) return;

        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("换弹中...");

        yield return new WaitForSeconds(weapons[currentWeaponIndex].reloadTime);

        int needed = weapons[currentWeaponIndex].magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
        Debug.Log("换弹完成! 弹药: " + currentAmmo + "/" + reserveAmmo);
    }

    public string GetCurrentWeaponName()
    {
        return weapons[currentWeaponIndex].weaponName;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetReserveAmmo()
    {
        return reserveAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}