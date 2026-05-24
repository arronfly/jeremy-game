/// <summary>
/// Constants used in PlayMode tests for consistent test values.
/// </summary>
public static class TestConstants
{
    // Player test constants
    public const float TEST_PLAYER_SPEED = 5f;
    public const float TEST_PLAYER_HEALTH = 100f;
    public const float TEST_PLAYER_MAX_HEALTH = 100f;

    // Bullet test constants
    public const float TEST_BULLET_SPEED = 10f;
    public const float TEST_BULLET_LIFETIME = 3f;
    public const float TEST_BULLET_DAMAGE = 20f;

    // Damage test constants
    public const float TEST_DAMAGE = 20f;
    public const float TEST_HEADSHOT_MULTIPLIER = 2f;
    public const float TEST_BODYSHOT_MULTIPLIER = 1f;

    // Fire rate test constants
    public const float TEST_FIRE_RATE = 0.2f;
    public const float TEST_FIRE_RATE_DELAY = 0.1f;

    // Enemy test constants
    public const float TEST_ENEMY_SPEED = 3f;
    public const float TEST_ENEMY_HEALTH = 50f;
    public const int TEST_ENEMIES_COUNT = 4;

    // Spawn positions for test objects
    public const float SPAWN_POS_X_DEFAULT = 0f;
    public const float SPAWN_POS_Y_DEFAULT = 0f;
    public const float SPAWN_POS_Z_DEFAULT = 0f;

    public static readonly Vector3 PLAYER_SPAWN_POSITION = new Vector3(0, 0, 0);
    public static readonly Vector3 ENEMY_SPAWN_POSITION_1 = new Vector3(10, 0, 0);
    public static readonly Vector3 ENEMY_SPAWN_POSITION_2 = new Vector3(-10, 0, 0);
    public static readonly Vector3 ENEMY_SPAWN_POSITION_3 = new Vector3(5, 5, 0);
    public static readonly Vector3 ENEMY_SPAWN_POSITION_4 = new Vector3(-5, -5, 0);

    // Array of all enemy spawn positions for convenience
    public static readonly Vector3[] ENEMY_SPAWN_POSITIONS = new Vector3[]
    {
        ENEMY_SPAWN_POSITION_1,
        ENEMY_SPAWN_POSITION_2,
        ENEMY_SPAWN_POSITION_3,
        ENEMY_SPAWN_POSITION_4
    };

    // Camera test positions
    public static readonly Vector3 CAMERA_TEST_POSITION = new Vector3(0, 10, -10);
    public const float CAMERA_TEST_FOV = 60f;

    // Grenade test constants
    public const float TEST_GRENADE_FUSE_TIME = 3f;
    public const float TEST_GRENADE_EXPLOSION_RADIUS = 5f;
    public const float TEST_GRENADE_DAMAGE = 30f;

    // Medkit test constants
    public const float TEST_MEDKIT_HEAL_AMOUNT = 25f;
    public const float TEST_MEDKIT_PICKUP_RADIUS = 2f;

    // Game state test constants
    public const int TEST_INITIAL_SCORE = 0;
    public const int TEST_SCORE_PER_KILL = 100;
    public const float TEST_ROUND_DURATION = 180f;

    // Timing constants for tests
    public const float TINY_DELAY = 0.01f;
    public const float SHORT_DELAY = 0.5f;
    public const float MEDIUM_DELAY = 1f;
    public const float LONG_DELAY = 5f;

    // Tolerance for floating point comparisons in tests
    public const float FLOAT_TOLERANCE = 0.001f;
    public const double DOUBLE_TOLERANCE = 0.001d;
}