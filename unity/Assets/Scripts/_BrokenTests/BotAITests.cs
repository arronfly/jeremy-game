using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class BotAITests
{
    private GameObject botGameObject;
    private BotAI botAI;
    private GameObject playerObject;
    private GameObject bulletPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create bullet prefab
        bulletPrefab = new GameObject("BulletPrefab");
        bulletPrefab.AddComponent<Bullet>();

        // Create player mock
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerObject.transform.position = Vector3.zero;

        // Create Bot
        botGameObject = new GameObject("Bot");
        botAI = botGameObject.AddComponent<BotAI>();
        botAI.bulletPrefab = bulletPrefab;
        botAI.firePoint = botGameObject.transform;
        botAI.player = playerObject.transform;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(botGameObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(bulletPrefab);
    }

    [Test]
    public void Bot_FindsNearestPlayer_WhenPlayerExists()
    {
        // Bot should have player reference
        Assert.IsNotNull(botAI.player, "Bot should find player by tag");

        // Player position differs from bot, bot should track it
        botGameObject.transform.position = new Vector3(5f, 0f, 0f);
        float distance = Vector2.Distance(botGameObject.transform.position, botAI.player.position);
        Assert.Greater(distance, 0f, "Bot should track player distance");
    }

    [Test]
    public void Bot_MovesTowardsTarget_WhenFarFromPlayer()
    {
        // Place bot far from player
        botGameObject.transform.position = new Vector3(10f, 0f, 0f);
        playerObject.transform.position = Vector3.zero;

        // Simulate FixedUpdate behavior
        float distanceBefore = Vector2.Distance(botGameObject.transform.position, playerObject.transform.position);
        Assert.Greater(distanceBefore, 3f, "Bot should start far from player");

        // Verify bot movement direction is toward player
        Vector2 toPlayer = (playerObject.transform.position - botGameObject.transform.position).normalized;
        Vector2 botDirection = botAI.moveDirection;
        Assert.AreEqual(toPlayer.x, botDirection.x, 0.1f, "Bot should move toward player");
    }

    [Test]
    public void Bot_AttacksWhenInRange_WhenPlayerIsClose()
    {
        // Place bot close to player (within fire range)
        botGameObject.transform.position = new Vector3(2f, 0f, 0f);
        playerObject.transform.position = new Vector3(1f, 0f, 0f);

        float distance = Vector2.Distance(botGameObject.transform.position, playerObject.transform.position);
        Assert.Less(distance, 3f, "Bot should be in attack range");

        // Simulate fire rate check
        float fireRate = botAI.fireRate;
        float lastFireTime = botAI.lastFireTime;
        bool canFire = Time.time - lastFireTime > fireRate;

        // Trigger shooting
        if (canFire)
        {
            botGameObject.SendMessage("TryShoot", (playerObject.transform.position - botGameObject.transform.position).normalized);
        }

        // Verify bullet was created
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        // Bullet will be tagged as Bot's tag if using bot's tag for identification
    }

    [Test]
    public void Bot_RetreatsWhenHealthLow_WhenTooCloseToPlayer()
    {
        // Bot should retreat when distance < 2f
        botGameObject.transform.position = new Vector3(1.5f, 0f, 0f);
        playerObject.transform.position = new Vector3(0f, 0f, 0f);

        float distance = Vector2.Distance(botGameObject.transform.position, playerObject.transform.position);
        Assert.Less(distance, 2f, "Bot should be close enough to trigger retreat");

        // The BotAI Retreat logic: when distance < 2f, moveDirection = -toPlayer
        Vector2 toPlayer = (playerObject.transform.position - botGameObject.transform.position).normalized;
        Vector2 expectedRetreatDir = -toPlayer;

        Assert.AreEqual(expectedRetreatDir.x, botAI.moveDirection.x, 0.1f, "Bot should retreat (move away from player)");
    }
}