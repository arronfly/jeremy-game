using UnityEngine;

/// <summary>
/// Sets up a minimal test scene with player, enemies, camera, and lighting.
/// Used for PlayMode tests to ensure consistent scene state.
/// </summary>
public class TestSceneSettings : MonoBehaviour
{
    [Header("Test Player Settings")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Vector2 _playerSpawnPosition = new Vector2(0, 0);

    [Header("Test Enemy Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Vector2[] _enemySpawnPositions = new Vector2[]
    {
        new Vector2(10, 0),
        new Vector2(-10, 0),
        new Vector2(5, 5),
        new Vector2(-5, -5)
    };

    [Header("Camera Settings")]
    [SerializeField] private Vector3 _cameraPosition = new Vector3(0, 10, -10);
    [SerializeField] private float _cameraFOV = 60f;

    private GameObject _playerInstance;
    private GameObject[] _enemyInstances;
    private Camera _mainCamera;

    public GameObject PlayerInstance => _playerInstance;
    public GameObject[] EnemyInstances => _enemyInstances;
    public Camera MainCamera => _mainCamera;

    private void Awake()
    {
        SetupTestScene();
    }

    private void SetupTestScene()
    {
        SetupLighting();
        SetupCamera();
        SetupPlayer();
        SetupEnemies();
    }

    private void SetupLighting()
    {
        Light mainLight = GameObject.Find("Main Light")?.GetComponent<Light>();
        if (mainLight == null)
        {
            GameObject lightGO = new GameObject("Main Light");
            mainLight = lightGO.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        mainLight.intensity = 1f;
        mainLight.color = Color.white;
    }

    private void SetupCamera()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            _mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
        }
        _mainCamera.transform.position = _cameraPosition;
        _mainCamera.transform.rotation = Quaternion.identity;
        _mainCamera.fieldOfView = _cameraFOV;
        _mainCamera.orthographic = false;
        _mainCamera.nearClipPlane = 0.1f;
        _mainCamera.farClipPlane = 1000f;
    }

    private void SetupPlayer()
    {
        if (_playerPrefab == null)
        {
            Debug.LogWarning("TestSceneSettings: Player prefab not assigned. Creating placeholder.");
            _playerInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _playerInstance.name = "TestPlayer";
        }
        else
        {
            _playerInstance = Instantiate(_playerPrefab, _playerSpawnPosition, Quaternion.identity);
            _playerInstance.name = "TestPlayer";
        }
    }

    private void SetupEnemies()
    {
        int enemyCount = _enemySpawnPositions.Length;
        _enemyInstances = new GameObject[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogWarning($"TestSceneSettings: Enemy prefab not assigned. Creating placeholder for enemy {i}.");
                _enemyInstances[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _enemyInstances[i].name = $"TestEnemy_{i}";
            }
            else
            {
                _enemyInstances[i] = Instantiate(_enemyPrefab, _enemySpawnPositions[i], Quaternion.identity);
                _enemyInstances[i].name = $"TestEnemy_{i}";
            }
        }
    }

    /// <summary>
    /// Gets the player instance by searching the scene.
    /// </summary>
    public GameObject GetPlayer()
    {
        if (_playerInstance != null)
            return _playerInstance;

        GameObject player = GameObject.Find("TestPlayer");
        return player ?? GameObject.FindGameObjectWithTag("Player");
    }

    /// <summary>
    /// Gets all enemy instances currently in the scene.
    /// </summary>
    public GameObject[] GetEnemies()
    {
        if (_enemyInstances != null && _enemyInstances.Length > 0)
            return _enemyInstances;

        return GameObject.FindGameObjectsWithTag("Enemy");
    }

    /// <summary>
    /// Gets the main camera reference.
    /// </summary>
    public Camera GetMainCamera()
    {
        return Camera.main ?? _mainCamera;
    }

    private void OnDestroy()
    {
        CleanupTestObjects();
    }

    private void CleanupTestObjects()
    {
        if (_playerInstance != null && Application.isPlaying == false)
        {
            DestroyImmediate(_playerInstance);
        }

        if (_enemyInstances != null)
        {
            foreach (var enemy in _enemyInstances)
            {
                if (enemy != null && Application.isPlaying == false)
                    DestroyImmediate(enemy);
            }
        }
    }
}