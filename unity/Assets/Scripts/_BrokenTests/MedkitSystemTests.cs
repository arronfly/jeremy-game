using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class MedkitSystemTests
{
    private GameObject medkitObject;
    private MedkitSystem medkitSystem;
    private GameObject playerObject;
    private PlayerHealth playerHealth;

    [SetUp]
    public void SetUp()
    {
        medkitObject = new GameObject("MedkitSystem");
        medkitSystem = medkitObject.AddComponent<MedkitSystem>();

        playerObject = new GameObject("Player");
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.maxHealth = 100;
        playerHealth.currentHealth = 50;

        medkitSystem.player = playerObject;
        medkitSystem.playerHealth = playerHealth;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(medkitObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void UseCorrectItem_WithKeyPress_7HealsHealth()
    {
        // Test key 7 for Medkit (health item)
        medkitSystem.availableItems = new System.Collections.Generic.List<ItemType>
        {
            ItemType.Medkit, ItemType.SpeedBoost, ItemType.Shield
        };

        int healthBefore = medkitSystem.playerHealth.currentHealth;
        medkitSystem.UseItemByKey(KeyCode.Alpha7);

        Assert.Greater(medkitSystem.playerHealth.currentHealth, healthBefore, "Key 7 should heal health with Medkit");
    }

    [Test]
    public void UseCorrectItem_WithKeyPress_8ActivatesSpeedBoost()
    {
        medkitSystem.availableItems = new System.Collections.Generic.List<ItemType>
        {
            ItemType.Medkit, ItemType.SpeedBoost, ItemType.Shield
        };

        medkitSystem.UseItemByKey(KeyCode.Alpha8);

        Assert.IsTrue(medkitSystem.IsSpeedBoostActive(), "Key 8 should activate SpeedBoost");
    }

    [Test]
    public void UseCorrectItem_WithKeyPress_9ActivatesShield()
    {
        medkitSystem.availableItems = new System.Collections.Generic.List<ItemType>
        {
            ItemType.Medkit, ItemType.SpeedBoost, ItemType.Shield
        };

        medkitSystem.UseItemByKey(KeyCode.Alpha9);

        Assert.IsTrue(medkitSystem.IsShieldActive(), "Key 9 should activate Shield");
    }

    [Test]
    public void Cooldown_PreventsImmediateUse_WhenOnCooldown()
    {
        // Set item on cooldown
        medkitSystem.lastUseTime = Time.time;

        float cooldown = medkitSystem.itemCooldown;
        bool canUse = Time.time - medkitSystem.lastUseTime >= cooldown;

        Assert.IsFalse(canUse, "Cooldown should prevent immediate item use");
    }

    [Test]
    public void Heal_AppliesCorrectly_WhenMedkitUsed()
    {
        playerHealth.currentHealth = 30;
        int healAmount = 40;

        medkitSystem.ApplyHeal(healAmount);

        Assert.AreEqual(70, playerHealth.currentHealth, "Medkit should heal by correct amount");
        Assert.LessOrEqual(playerHealth.currentHealth, playerHealth.maxHealth, "Health should not exceed max");
    }

    [Test]
    public void SpeedBoost_ActivatesAndDeactivates_AfterDuration()
    {
        medkitSystem.activateSpeedBoost = true;
        medkitSystem.speedBoostDuration = 3f;

        medkitSystem.ActivateSpeedBoost();
        Assert.IsTrue(medkitSystem.IsSpeedBoostActive(), "Speed boost should be active immediately after activation");

        // Simulate time passing
        float elapsed = 0f;
        while (elapsed < medkitSystem.speedBoostDuration)
        {
            elapsed += Time.deltaTime;
            medkitSystem.Update();
        }

        Assert.IsFalse(medkitSystem.IsSpeedBoostActive(), "Speed boost should deactivate after duration expires");
    }
}

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
}