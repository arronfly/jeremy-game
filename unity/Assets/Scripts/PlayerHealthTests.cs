using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class PlayerHealthTests
{
    private GameObject playerGameObject;
    private PlayerHealth health;

    [UnitySetUp]
    public IEnumerator UnitySetup()
    {
        playerGameObject = new GameObject("Player");
        health = playerGameObject.AddComponent<PlayerHealth>();
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerGameObject);
    }

    [Test]
    public void TakeDamage_ReducesHealthCorrectly()
    {
        int initialHealth = health.GetCurrentHealth();
        health.TakeDamage(25);
        Assert.AreEqual(initialHealth - 25, health.GetCurrentHealth());
    }

    [Test]
    public void TakeDamage_MultipleTimes()
    {
        health.TakeDamage(10);
        health.TakeDamage(15);
        Assert.AreEqual(75, health.GetCurrentHealth());
    }

    [Test]
    public void Heal_IncreasesHealth()
    {
        health.TakeDamage(50);
        health.Heal(25);
        Assert.AreEqual(75, health.GetCurrentHealth());
    }

    [Test]
    public void Heal_CappedAtMaxHealth()
    {
        health.Heal(50);
        Assert.AreEqual(100, health.GetCurrentHealth());
    }

    [Test]
    public void Heal_DoesNotExceedMaxHealth()
    {
        health.Heal(200);
        Assert.AreEqual(100, health.GetCurrentHealth());
    }

    [Test]
    public void Death_AtZeroHealth()
    {
        health.TakeDamage(100);
        Assert.AreEqual(0, health.GetCurrentHealth());
        Assert.IsFalse(health.IsAlive());
    }

    [Test]
    public void Death_PlayerBecomesInactive()
    {
        health.TakeDamage(100);
        Assert.IsFalse(playerGameObject.activeSelf);
    }

    [Test]
    public void Invulnerability_AfterDamage()
    {
        health.TakeDamage(10);
        Assert.IsTrue(Time.time - health.lastDamageTime < health.invulnerabilityTime);
    }

    [Test]
    public void Invulnerability_PreventsDamage()
    {
        int healthBefore = health.GetCurrentHealth();
        health.TakeDamage(10);
        int healthAfterFirstHit = health.GetCurrentHealth();
        health.TakeDamage(10);
        Assert.AreEqual(healthAfterFirstHit, healthBefore - 10);
    }

    [Test]
    public void FullHeal_SetsToMaxHealth()
    {
        health.TakeDamage(50);
        health.FullHeal();
        Assert.AreEqual(100, health.GetCurrentHealth());
    }

    [Test]
    public void GetHealthPercent_ReturnsCorrectPercentage()
    {
        health.TakeDamage(50);
        Assert.AreEqual(0.5f, health.GetHealthPercent());
    }

    [Test]
    public void IsAlive_ReturnsTrue()
    {
        Assert.IsTrue(health.IsAlive());
    }

    [Test]
    public void IsAlive_ReturnsFalseAtZeroHealth()
    {
        health.TakeDamage(100);
        Assert.IsFalse(health.IsAlive());
    }

    [Test]
    public void MaxHealth_IsCorrect()
    {
        Assert.AreEqual(100, health.maxHealth);
    }
}
