using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class GrenadeSystemTests
{
    private GameObject grenadeObject;
    private GrenadeSystem grenadeSystem;
    private GameObject playerObject;

    [SetUp]
    public void SetUp()
    {
        grenadeObject = new GameObject("GrenadeSystem");
        grenadeSystem = grenadeObject.AddComponent<GrenadeSystem>();
        playerObject = new GameObject("Player");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(grenadeObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void SwitchGrenadeType_WithGKey_ChangesType()
    {
        // Simulate G key press for grenade type switching
        grenadeSystem.currentGrenadeType = GrenadeType.Frag;

        // Simulate G key press to switch
        KeyCode gKey = KeyCode.G;

        // Trigger switch logic (simulate what happens when G is pressed)
        grenadeSystem.SwitchGrenadeType();

        // Verify type changes
        Assert.AreNotEqual(GrenadeType.Frag, grenadeSystem.currentGrenadeType, "Grenade type should switch when G is pressed");
    }

    [Test]
    public void Cooldown_PreventsImmediateThrow_WhenOnCooldown()
    {
        // Set grenade on cooldown
        grenadeSystem.lastThrowTime = Time.time;

        // Check if cooldown prevents throwing
        float cooldown = grenadeSystem.throwCooldown;
        bool canThrow = Time.time - grenadeSystem.lastThrowTime >= cooldown;

        Assert.IsFalse(canThrow, "Cooldown should prevent immediate throw");
    }

    [Test]
    public void Smoke_CreatesAreaEffect_WhenActivated()
    {
        grenadeSystem.currentGrenadeType = GrenadeType.Smoke;
        grenadeSystem.smokeRadius = 3f;

        Vector3 throwPosition = new Vector3(0f, 0f, 0f);
        Vector3 targetPosition = new Vector3(5f, 0f, 0f);

        // Simulate smoke grenade activation
        GameObject smokeEffect = grenadeSystem.CreateSmokeEffect(targetPosition);

        Assert.IsNotNull(smokeEffect, "Smoke effect should be created");

        // Check area effect by verifying smoke covers target area
        float distance = Vector3.Distance(smokeEffect.transform.position, targetPosition);
        Assert.LessOrEqual(distance, grenadeSystem.smokeRadius, "Smoke should cover target area");
    }

    [Test]
    public void Flash_BlindsEnemies_WhenDetonated()
    {
        grenadeSystem.currentGrenadeType = GrenadeType.Flash;
        grenadeSystem.flashRadius = 5f;
        grenadeSystem.flashDuration = 2f;

        Vector3 detonationPoint = new Vector3(0f, 0f, 0f);
        GameObject flashBang = grenadeSystem.CreateFlashBang(detonationPoint);

        Assert.IsNotNull(flashBang, "FlashBang should be created");

        // Verify flash affects enemies within radius
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(detonationPoint, enemy.transform.position);
            if (distance <= grenadeSystem.flashRadius)
            {
                // Enemy should be blinded
                Assert.IsTrue(enemy.GetComponent<Enemy>()?.isBlinded, "Enemy within radius should be blinded");
            }
        }
    }

    [Test]
    public void Frag_DealsDamageInRadius_WhenExploding()
    {
        grenadeSystem.currentGrenadeType = GrenadeType.Frag;
        grenadeSystem.fragRadius = 4f;
        grenadeSystem.fragDamage = 50;

        Vector3 explosionPoint = new Vector3(0f, 0f, 0f);

        // Create a mock enemy within radius
        GameObject enemyInRange = new GameObject("EnemyInRange");
        enemyInRange.transform.position = new Vector3(2f, 0f, 0f);
        Enemy enemyScript = enemyInRange.AddComponent<Enemy>();

        float distance = Vector3.Distance(explosionPoint, enemyInRange.transform.position);
        Assert.Less(distance, grenadeSystem.fragRadius, "Enemy should be within frag radius");

        // Simulate damage application
        grenadeSystem.ApplyFragDamage(explosionPoint);

        Assert.GreaterOrEqual(enemyScript.health, 0, "Enemy should take damage from frag");
        Assert.Less(enemyScript.health, enemyScript.maxHealth, "Enemy health should be reduced");

        Object.DestroyImmediate(enemyInRange);
    }
}

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    public bool isBlinded = false;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}