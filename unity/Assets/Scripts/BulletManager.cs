using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages bullet pooling and collision detection
/// </summary>
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 100;

    [Header("Performance Settings")]
    public float cleanupInterval = 5f;
    public int maxActiveBullets = 500;

    private Queue<GameObject> bulletPool;
    private List<Bullet> activeBullets;
    private float cleanupTimer;
    private int lastActiveCount;

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void Update()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer >= cleanupInterval)
        {
            CleanupInactiveBullets();
            cleanupTimer = 0f;
        }

        lastActiveCount = activeBullets.Count;
    }

    void InitializePool()
    {
        bulletPool = new Queue<GameObject>();
        activeBullets = new List<Bullet>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject SpawnBullet(Vector2 position, Vector2 velocity, int damage, string ownerTag)
    {
        if (activeBullets.Count >= maxActiveBullets)
        {
            return null;
        }

        GameObject bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab);
        }

        bullet.SetActive(true);
        bullet.transform.position = position;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Initialize(velocity, damage, ownerTag);
        }

        activeBullets.Add(bulletComp);
        return bullet;
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet.gameObject);
        activeBullets.Remove(bullet);
    }

    void CleanupInactiveBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (i >= activeBullets.Count) break;

            Bullet bullet = activeBullets[i];
            if (bullet == null || !bullet.gameObject.activeInHierarchy)
            {
                activeBullets.RemoveAt(i);
            }
        }
    }

    public int GetActiveBulletCount()
    {
        return activeBullets.Count;
    }

    public int GetPooledBulletCount()
    {
        return bulletPool.Count;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void OnApplicationQuit()
    {
        Instance = null;
    }
}