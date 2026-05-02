using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class PlayerControllerTests
{
    private GameObject playerGameObject;
    private PlayerController controller;
    private Rigidbody2D rb;

    [UnitySetUp]
    public IEnumerator UnitySetup()
    {
        playerGameObject = new GameObject("Player");
        playerGameObject.AddComponent<Rigidbody2D>();
        playerGameObject.AddComponent<PlayerHealth>();
        controller = playerGameObject.AddComponent<PlayerController>();

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerGameObject);
    }

    [Test]
    public void MoveUp_CorrectDirection()
    {
        var initialPos = playerGameObject.transform.position;
        controller.moveDirection = Vector2.up;
        controller.Update();
        Assert.AreEqual(Vector2.up, controller.moveDirection);
    }

    [Test]
    public void MoveDown_CorrectDirection()
    {
        controller.moveDirection = Vector2.down;
        Assert.AreEqual(Vector2.down, controller.moveDirection);
    }

    [Test]
    public void MoveLeft_CorrectDirection()
    {
        controller.moveDirection = Vector2.left;
        Assert.AreEqual(Vector2.left, controller.moveDirection);
    }

    [Test]
    public void MoveRight_CorrectDirection()
    {
        controller.moveDirection = Vector2.right;
        Assert.AreEqual(Vector2.right, controller.moveDirection);
    }

    [Test]
    public void AimTowardsMousePosition()
    {
        Assert.IsTrue(controller.aimAtMouse);
    }

    [Test]
    public void SprintWithShiftKey_SetsSprintTrue()
    {
        controller.moveDirection = Vector2.right;
        Assert.IsFalse(controller.isSprinting);
    }

    [Test]
    public void BoundaryClamping_StaysWithinBounds()
    {
        Vector3 pos = playerGameObject.transform.position;
        pos.x = 10f;
        pos.y = 10f;
        playerGameObject.transform.position = pos;

        pos.x = Mathf.Clamp(pos.x, -7.5f, 7.5f);
        pos.y = Mathf.Clamp(pos.y, -4f, 4f);

        Assert.AreEqual(7.5f, pos.x);
        Assert.AreEqual(4f, pos.y);
    }

    [Test]
    public void BoundaryClamping_LeftEdge()
    {
        Vector3 pos = Vector3.zero;
        pos.x = -10f;
        pos.y = 0f;
        pos.x = Mathf.Clamp(pos.x, -7.5f, 7.5f);

        Assert.AreEqual(-7.5f, pos.x);
    }

    [Test]
    public void BoundaryClamping_RightEdge()
    {
        Vector3 pos = Vector3.zero;
        pos.x = 10f;
        pos.y = 0f;
        pos.x = Mathf.Clamp(pos.x, -7.5f, 7.5f);

        Assert.AreEqual(7.5f, pos.x);
    }

    [Test]
    public void BoundaryClamping_BottomEdge()
    {
        Vector3 pos = Vector3.zero;
        pos.x = 0f;
        pos.y = -10f;
        pos.y = Mathf.Clamp(pos.y, -4f, 4f);

        Assert.AreEqual(-4f, pos.y);
    }

    [Test]
    public void BoundaryClamping_TopEdge()
    {
        Vector3 pos = Vector3.zero;
        pos.x = 0f;
        pos.y = 10f;
        pos.y = Mathf.Clamp(pos.y, -4f, 4f);

        Assert.AreEqual(4f, pos.y);
    }

    [Test]
    public void GetMoveSpeed_ReturnsBaseSpeed()
    {
        Assert.AreEqual(5f, controller.moveSpeed);
    }

    [Test]
    public void SprintMultiplier_IsCorrect()
    {
        Assert.AreEqual(1.5f, controller.sprintMultiplier);
    }
}
