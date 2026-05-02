using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject botPrefab;

    [Header("Spawn Points")]
    public Vector2[] redSpawnPoints;
    public Vector2[] blueSpawnPoints;

    [Header("References")]
    public RoundManager roundManager;
    public HUDManager hudManager;

    void Start()
    {
        SetupScene();
    }

    public void SetupScene()
    {
        // Spawn red team
        for (int i = 0; i < redSpawnPoints.Length && i < 4; i++)
        {
            GameObject player = Instantiate(playerPrefab, redSpawnPoints[i], Quaternion.identity);
            // Link to round manager
        }

        // Spawn blue team bots
        for (int i = 0; i < blueSpawnPoints.Length && i < 4; i++)
        {
            GameObject bot = Instantiate(botPrefab, blueSpawnPoints[i], Quaternion.identity);
            // Link to round manager
        }

        // Setup camera
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = 5;
    }
}
