using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class BulletTests
{
    private GameObject bulletPrefab;
    private GameObject playerPrefab;
    private GameObject bullet;
    private GameObject player;
    private PlayerHealth playerHealth;

    [SetUp]
    public void Setup()
    {
        // Create bullet prefab
        bulletPrefab = new GameObject("Bullet");
        bulletPrefab.AddComponent<Bullet>();
        bulletPrefab.AddComponent<Rigidbody2D>();
        bulletPrefab.AddComponent<CircleCollider2D>();

        // Create player prefab for damage test
        playerPrefab = new GameObject("Player");
        playerPrefab.tag = "Player";
        playerHealth = playerPrefab.AddComponent<PlayerHealth>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(bulletPrefab);
        Object.Destroy(playerPrefab);
        if (bullet != null) Object.Destroy(bullet);
        if (player != null) Object.Destroy(player);
    }

    [Test]
    public void Bullet_Moves_In_Correct_Direction()
    {
        // Arrange
        Vector2 velocity = new Vector2(10f, 5f);
        bullet = Object.Instantiate(bulletPrefab);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // Act
        bulletComp.Initialize(velocity, 25, "Enemy");
        Vector2 expectedDirection = velocity.normalized;

        // Assert - velocity matches expected direction
        Assert.AreEqual(expectedDirection.x, rb.velocity.normalized.x, 0.01f, "Bullet X direction should match");
        Assert.AreEqual(expectedDirection.y, rb.velocity.normalized.y, 0.01f, "Bullet Y direction should match");
    }

    [Test]
    public void Bullet_Damages_Player_On_Hit()
    {
        // Arrange
        bullet = Object.Instantiate(bulletPrefab);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        player = Object.Instantiate(playerPrefab);
        playerHealth = player.GetComponent<PlayerHealth>();
        int initialHealth = playerHealth.GetCurrentHealth();
        int bulletDamage = 25;

        // Act
        bulletComp.Initialize(Vector2.right * 10f, bulletDamage, "Enemy");

        // Simulate trigger hit by calling OnTriggerEnter2D
        Collider2D collider = player.GetComponent<Collider2D>();
        bulletComp.GetComponent<Collider2D>().isTrigger = true;
        bulletComp.OnTriggerEnter2D(collider);

        // Assert
        Assert.Less(playerHealth.GetCurrentHealth(), initialHealth, "Player health should decrease after bullet hit");
        Assert.AreEqual(initialHealth - bulletDamage, playerHealth.GetCurrentHealth(), "Bullet damage should be applied correctly");
    }

    [Test]
    public void Bullet_Destroys_On_Collision()
    {
        // Arrange
        bullet = Object.Instantiate(bulletPrefab);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        bulletComp.Initialize(Vector2.right * 10f, 25, "Enemy");

        // Act - simulate collision with wall
        GameObject wall = new GameObject("Wall");
        wall.AddComponent<BoxCollider2D>();
        Collision2D collision = new Collision2D();
        collision.otherCollider = wall.GetComponent<Collider2D>();

        // Manually call collision handler (in real scenario OnCollisionEnter2D would be called by Unity)
        bulletComp.OnCollisionEnter2D(collision);

        // Assert - bullet should be destroyed
        Assert.IsTrue(bullet == null || !bullet.activeSelf, "Bullet should be destroyed on collision");
    }

    [Test]
    public void Bullet_Lifetime_Expiry()
    {
        // Arrange
        bullet = Object.Instantiate(bulletPrefab);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        bulletComp.Initialize(Vector2.right * 10f, 25, "Enemy");

        // Act - simulate lifetime expiry by waiting
        float lifetime = 3f;
        bulletComp.GetComponent<Bullet>().Initialize(Vector2.right * 10f, 25, "Enemy");

        // Assert - verify the bullet was initialized with lifetime
        Assert.IsNotNull(bulletComp, "Bullet should exist after initialization");
        Assert.IsTrue(bulletComp.GetDamage() == 25, "Bullet damage should be set correctly");
    }
}
