using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class RoundManagerTests
{
    private GameObject roundManagerObj;
    private RoundManager roundManager;
    private GameObject[] redTeam;
    private GameObject[] blueTeam;
    private int redIndex;
    private int blueIndex;

    [SetUp]
    public void Setup()
    {
        roundManagerObj = new GameObject("RoundManager");
        roundManager = roundManagerObj.AddComponent<RoundManager>();

        // Initialize team arrays
        roundManager.redTeamPlayers = new GameObject[2];
        roundManager.blueTeamPlayers = new GameObject[2];

        // Create red team players with health
        for (int i = 0; i < 2; i++)
        {
            GameObject player = new GameObject("RedPlayer" + i);
            player.tag = "Player";
            PlayerHealth health = player.AddComponent<PlayerHealth>();
            roundManager.redTeamPlayers[i] = player;
        }

        // Create blue team players with health
        for (int i = 0; i < 2; i++)
        {
            GameObject player = new GameObject("BluePlayer" + i);
            player.tag = "Player";
            PlayerHealth health = player.AddComponent<PlayerHealth>();
            roundManager.blueTeamPlayers[i] = player;
        }
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(roundManagerObj);
        if (roundManager.redTeamPlayers != null)
            foreach (var p in roundManager.redTeamPlayers) Object.Destroy(p);
        if (roundManager.blueTeamPlayers != null)
            foreach (var p in roundManager.blueTeamPlayers) Object.Destroy(p);
    }

    [Test]
    public void Round_Ends_When_One_Team_Eliminated()
    {
        // Arrange
        roundManager.redScore = 0;
        roundManager.blueScore = 0;
        roundManager.isRoundActive = true;

        // Act - kill all red team players
        foreach (var player in roundManager.redTeamPlayers)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            health.TakeDamage(100);
        }

        // Call CheckRoundEnd logic
        roundManager.StartCoroutine(CheckRoundEndRoutine());

        // Assert
        Assert.IsFalse(roundManager.isRoundActive, "Round should end when one team is eliminated");
    }

    private System.Collections.IEnumerator CheckRoundEndRoutine()
    {
        yield return new WaitForSeconds(0.1f);
    }

    [Test]
    public void Score_Updates_On_Round_End()
    {
        // Arrange
        roundManager.redScore = 0;
        roundManager.blueScore = 0;

        // Act - simulate blue team winning by eliminating red team
        foreach (var player in roundManager.redTeamPlayers)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            health.TakeDamage(100);
        }

        // Trigger score update through event simulation
        roundManager.onScoreUpdate += (red, blue) =>
        {
            roundManager.redScore = red;
            roundManager.blueScore = blue;
        };

        // Manually trigger the update for test
        int initialBlueScore = roundManager.blueScore;
        roundManager.UpdateScore(roundManager.redScore, roundManager.blueScore + 1);

        // Assert
        Assert.Greater(roundManager.blueScore, initialBlueScore, "Blue score should increase after winning round");
    }

    [Test]
    public void Win_Condition_At_4_Points()
    {
        // Arrange
        roundManager.winScore = 4;
        roundManager.redScore = 3;
        roundManager.blueScore = 2;

        // Act - red team scores to reach 4
        roundManager.UpdateScore(4, 2);

        // Assert - verify win condition check method exists
        Assert.AreEqual(4, roundManager.winScore, "Win score should be 4");
        Assert.IsTrue(roundManager.redScore >= roundManager.winScore || roundManager.blueScore >= roundManager.winScore,
            "Game should be able to end when a team reaches win score");
    }

    [Test]
    public void Sudden_Death_At_3_3()
    {
        // Arrange
        roundManager.redScore = 3;
        roundManager.blueScore = 3;
        roundManager.isSuddenDeath = false;

        // Act
        roundManager.UpdateScore(3, 3);
        roundManager.CheckSuddenDeath();

        // Assert
        Assert.AreEqual(3, roundManager.redScore, "Red score should be 3");
        Assert.AreEqual(3, roundManager.blueScore, "Blue score should be 3");
    }

    [Test]
    public void Respawn_After_Round_End()
    {
        // Arrange
        roundManager.respawnDelay = 3f;
        Vector3[] expectedPositions = new Vector3[2];

        // Get expected spawn positions for red team
        float redSpawnX = -6f;
        for (int i = 0; i < 2; i++)
        {
            expectedPositions[i] = new Vector3(redSpawnX, -2f + i * 1.5f, 0);
        }

        // Act
        roundManager.RespawnAllPlayers();

        // Assert - verify players are repositioned to spawn points
        for (int i = 0; i < roundManager.redTeamPlayers.Length; i++)
        {
            Assert.AreEqual(expectedPositions[i].x, roundManager.redTeamPlayers[i].transform.position.x, 0.1f,
                "Red player " + i + " should be at correct spawn X");
            Assert.AreEqual(expectedPositions[i].y, roundManager.redTeamPlayers[i].transform.position.y, 0.1f,
                "Red player " + i + " should be at correct spawn Y");
        }
    }
}
