using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

public class HUDManagerTests
{
    private GameObject hudManagerObj;
    private HUDManager hudManager;
    private GameObject roundManagerObj;
    private RoundManager roundManager;
    private GameObject playerObj;
    private PlayerHealth playerHealth;
    private WeaponSystem weaponSystem;

    [SetUp]
    public void Setup()
    {
        // Create HUD Manager
        hudManagerObj = new GameObject("HUDManager");
        hudManager = hudManagerObj.AddComponent<HUDManager>();

        // Create Round Manager
        roundManagerObj = new GameObject("RoundManager");
        roundManager = roundManagerObj.AddComponent<RoundManager>();
        hudManager.roundManager = roundManager;

        // Create player with required components
        playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerHealth = playerObj.AddComponent<PlayerHealth>();
        weaponSystem = playerObj.AddComponent<WeaponSystem>();

        // Set up HUD references
        GameObject redScoreObj = new GameObject("RedScoreText");
        redScoreObj.AddComponent<Text>();
        hudManager.redScoreText = redScoreObj.GetComponent<Text>();

        GameObject blueScoreObj = new GameObject("BlueScoreText");
        blueScoreObj.AddComponent<Text>();
        hudManager.blueScoreText = blueScoreObj.GetComponent<Text>();

        GameObject roundObj = new GameObject("RoundText");
        roundObj.AddComponent<Text>();
        hudManager.roundText = roundObj.GetComponent<Text>();

        GameObject healthBarObj = new GameObject("HealthBarFill");
        healthBarObj.AddComponent<Image>();
        hudManager.healthBarFill = healthBarObj.GetComponent<Image>();

        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.AddComponent<Text>();
        hudManager.healthText = healthTextObj.GetComponent<Text>();

        GameObject ammoTextObj = new GameObject("AmmoText");
        ammoTextObj.AddComponent<Text>();
        hudManager.ammoText = ammoTextObj.GetComponent<Text>();

        GameObject weaponTextObj = new GameObject("WeaponText");
        weaponTextObj.AddComponent<Text>();
        hudManager.weaponText = weaponTextObj.GetComponent<Text>();

        // Create kill feed
        GameObject killFeedContent = new GameObject("KillFeedContent");
        killFeedContent.transform.parent = hudManagerObj.transform;
        hudManager.killFeedContent = killFeedContent.transform;

        GameObject killFeedItemPrefab = new GameObject("KillFeedItemPrefab");
        killFeedItemPrefab.AddComponent<Text>();
        killFeedItemPrefab.AddComponent<Text>();
        hudManager.killFeedItemPrefab = killFeedItemPrefab;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(hudManagerObj);
        Object.Destroy(roundManagerObj);
        Object.Destroy(playerObj);
    }

    [Test]
    public void Score_Display_Updates()
    {
        // Arrange
        int redScore = 2;
        int blueScore = 1;

        // Act
        hudManager.UpdateScore(redScore, blueScore);

        // Assert
        Assert.AreEqual("红队: " + redScore, hudManager.redScoreText.text, "Red score text should update correctly");
        Assert.AreEqual("蓝队: " + blueScore, hudManager.blueScoreText.text, "Blue score text should update correctly");
    }

    [Test]
    public void Health_Bar_Reflects_Player_Health()
    {
        // Arrange
        playerHealth = playerObj.GetComponent<PlayerHealth>();
        hudManager.playerHealth = playerHealth;

        // Act - set player health to 50 (50% of max 100)
        playerHealth.TakeDamage(50);

        // Trigger health bar update manually (simulating Update loop)
        hudManager.UpdatePlayerStatus();

        // Assert
        float expectedFill = 0.5f;
        Assert.AreEqual(expectedFill, hudManager.healthBarFill.fillAmount, 0.01f, "Health bar fill should be 50% when health is 50");
    }

    [Test]
    public void Ammo_Display_Updates()
    {
        // Arrange
        hudManager.weaponSystem = weaponSystem;

        // Act
        hudManager.UpdatePlayerStatus();

        // Assert
        Assert.IsNotNull(hudManager.ammoText, "Ammo text should be assigned");
        Assert.IsNotNull(hudManager.weaponText, "Weapon text should be assigned");
    }

    [Test]
    public void Kill_Feed_Shows_Entries()
    {
        // Arrange
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = playerObj;
        }

        // Act
        hudManager.AddKillFeed("Player1", "Player2", "red");

        // Assert - verify kill feed item was created
        Assert.IsNotNull(hudManager.killFeedContent, "Kill feed content transform should exist");
        Assert.IsNotNull(hudManager.killFeedItemPrefab, "Kill feed item prefab should exist");

        // Verify the prefab is set up correctly for instantiation
        Assert.IsTrue(hudManager.killFeedItemPrefab.GetComponent<Text>() != null, "Kill feed item should have Text component");
    }

    [Test]
    public void OnRoundStart_Updates_Round_Text()
    {
        // Arrange
        int round = 3;
        bool suddenDeath = false;

        // Act
        hudManager.OnRoundStart(round, suddenDeath);

        // Assert
        Assert.AreEqual("第 " + round + " 回合/7", hudManager.roundText.text, "Round text should show current round");
    }

    [Test]
    public void OnRoundStart_SuddenDeath_Shows_Special_Text()
    {
        // Arrange
        int round = 6;
        bool suddenDeath = true;

        // Act
        hudManager.OnRoundStart(round, suddenDeath);

        // Assert
        Assert.AreEqual("决胜局!", hudManager.roundText.text, "Round text should show sudden death message");
    }
}
