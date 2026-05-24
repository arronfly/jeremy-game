using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 子弹对象池管理器 - 优化子弹生成性能
/// </summary>
public class BulletManager : MonoBehaviour
{
    [Header("子弹预制体")]
    public GameObject bulletPrefab;

    [Header("池设置")]
    public int initialPoolSize = 20;
    public int maxPoolSize = 50;

    private Queue<Bullet> bulletPool = new Queue<Bullet>();
    private List<Bullet> activeBullets = new List<Bullet>();

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletManager: bulletPrefab 未设置!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet();
        }
    }

    Bullet CreateNewBullet()
    {
        GameObject obj = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
        obj.SetActive(false);
        Bullet bullet = obj.GetComponent<Bullet>();
        bulletPool.Enqueue(bullet);
        return bullet;
    }

    /// <summary>
    /// 从池中获取子弹
    /// </summary>
    public Bullet GetBullet()
    {
        Bullet bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else if (activeBullets.Count < maxPoolSize)
        {
            bullet = CreateNewBullet();
            bulletPool.Dequeue(); // 刚创建的刚从池中取出
        }
        else
        {
            Debug.LogWarning("BulletManager: 子弹池已满!");
            return null;
        }

        bullet.gameObject.SetActive(true);
        activeBullets.Add(bullet);
        return bullet;
    }

    /// <summary>
    /// 回收子弹到池中
    /// </summary>
    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.gameObject.SetActive(false);
        activeBullets.Remove(bullet);
        bulletPool.Enqueue(bullet);
    }

    /// <summary>
    /// 获取当前活跃子弹数量
    /// </summary>
    public int GetActiveCount()
    {
        return activeBullets.Count;
    }

    /// <summary>
    /// 获取池中可用子弹数量
    /// </summary>
    public int GetAvailableCount()
    {
        return bulletPool.Count;
    }

    /// <summary>
    /// 发射子弹
    /// </summary>
    public void FireBullet(Vector2 position, Vector2 velocity, int damage, string ownerTag)
    {
        Bullet bullet = GetBullet();
        if (bullet != null)
        {
            bullet.Initialize(velocity, damage, ownerTag);
            bullet.transform.position = position;
        }
    }

    /// <summary>
    /// 清理所有活跃子弹
    /// </summary>
    public void ClearAllBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            ReturnBullet(activeBullets[i]);
        }
    }
}
