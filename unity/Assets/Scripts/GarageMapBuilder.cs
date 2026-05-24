using UnityEngine;

/// <summary>
/// 假车库地图构建器 - 程序化生成完整的"Fake Garage"地图
/// 所有地图元素均从代码生成，无需手动场景设置
/// 坐标系统: 1 Unity单位 = 1 游戏像素, 世界范围 x:-800~800, y:-600~600
/// </summary>
public class GarageMapBuilder : MonoBehaviour
{
    [Header("Team Spawn Points")]
    public Vector2[] redSpawnPoints = new Vector2[]
    {
        new Vector2(-650, 300),
        new Vector2(-650, 100),
        new Vector2(-650, -100),
        new Vector2(-650, -300)
    };

    public Vector2[] blueSpawnPoints = new Vector2[]
    {
        new Vector2(650, 300),
        new Vector2(650, 100),
        new Vector2(650, -100),
        new Vector2(650, -300)
    };

    // Sorting layer name constants
    private const string LayerGround = "Ground";
    private const string LayerCover = "Cover";
    private const string LayerCharacter = "Character";
    private const string LayerBullet = "Bullet";
    private const string LayerEffect = "Effect";
    private const string LayerUI = "UI";

    // Sorting order constants
    private const int OrderGround = -1;
    private const int OrderCover = 0;
    private const int OrderWall = 1;

    // Map parent
    private Transform mapRoot;

    /// <summary>
    /// BuildMap - Called by GameBootstrapper to construct the full map.
    /// Returns this GarageMapBuilder instance so spawn points can be read.
    /// </summary>
    public GarageMapBuilder BuildMap()
    {
        Debug.Log("=== GarageMapBuilder: Building Fake Garage Map ===");

        // Create root container
        GameObject rootObj = new GameObject("GarageMap");
        mapRoot = rootObj.transform;

        // Build map layers in order
        BuildGround();
        BuildBoundaryWalls();
        BuildGarages();
        BuildCenterObstacles();
        BuildExternalCover();

        Debug.Log("=== GarageMapBuilder: Map construction complete ===");
        return this;
    }

    // ========================================================================
    // GROUND
    // ========================================================================

    private void BuildGround()
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.SetParent(mapRoot);
        ground.transform.position = Vector3.zero;

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.Instance.GetSprite("ground");
        sr.sortingLayerName = LayerGround;
        sr.sortingOrder = OrderGround;

        // Scale to fill the world bounds (1600x1200 pixels)
        ground.transform.localScale = new Vector3(1600f, 1200f, 1f);

        // No collider on ground
    }

    // ========================================================================
    // BOUNDARY WALLS
    // ========================================================================

    private void BuildBoundaryWalls()
    {
        GameObject wallsParent = new GameObject("BoundaryWalls");
        wallsParent.transform.SetParent(mapRoot);

        // North wall
        CreateWall("Wall_North", wallsParent.transform,
            new Vector2(0, 580f), new Vector2(1600f, 40f));

        // South wall
        CreateWall("Wall_South", wallsParent.transform,
            new Vector2(0, -580f), new Vector2(1600f, 40f));

        // West wall
        CreateWall("Wall_West", wallsParent.transform,
            new Vector2(-780f, 0), new Vector2(40f, 1200f));

        // East wall
        CreateWall("Wall_East", wallsParent.transform,
            new Vector2(780f, 0), new Vector2(40f, 1200f));
    }

    // ========================================================================
    // GARAGES (4: A top-left, B top-right, C bottom-left, D bottom-right)
    // ========================================================================

    private void BuildGarages()
    {
        GameObject garagesParent = new GameObject("Garages");
        garagesParent.transform.SetParent(mapRoot);

        // Garage A - top-left, door opens east (right)
        CreateGarage("Garage_A", new Vector2(-500f, 250f), new Vector2(200f, 200f), "east");

        // Garage B - top-right, door opens west (left)
        CreateGarage("Garage_B", new Vector2(500f, 250f), new Vector2(200f, 200f), "west");

        // Garage C - bottom-left, door opens east (right)
        CreateGarage("Garage_C", new Vector2(-500f, -250f), new Vector2(200f, 200f), "east");

        // Garage D - bottom-right, door opens west (left)
        CreateGarage("Garage_D", new Vector2(500f, -250f), new Vector2(200f, 200f), "west");
    }

    /// <summary>
    /// Creates a single garage structure with 3 solid walls, 1 open door side,
    /// 2 window openings, and 1 internal cover crate.
    /// </summary>
    private void CreateGarage(string name, Vector2 center, Vector2 size, string doorSide)
    {
        GameObject garage = new GameObject(name);
        garage.transform.SetParent(mapRoot);
        garage.transform.position = center;

        float halfW = size.x / 2f;
        float halfH = size.y / 2f;

        // Wall thickness
        float wallThickness = 12f;

        // North wall (always solid)
        CreateWall("NorthWall", garage.transform,
            new Vector2(0, halfH), new Vector2(size.x, wallThickness));

        // South wall (always solid)
        CreateWall("SouthWall", garage.transform,
            new Vector2(0, -halfH), new Vector2(size.x, wallThickness));

        // Side walls - one is solid with windows, the other is the door side
        if (doorSide == "east")
        {
            // West wall (solid, with windows)
            CreateWallWithWindows("WestWall", garage.transform,
                new Vector2(-halfW, 0), new Vector2(wallThickness, size.y), size.y);

            // East side (door - visual only, no collider)
            CreateDoorSide("EastDoor", garage.transform,
                new Vector2(halfW, 0), new Vector2(wallThickness, size.y));
        }
        else // doorSide == "west"
        {
            // East wall (solid, with windows)
            CreateWallWithWindows("EastWall", garage.transform,
                new Vector2(halfW, 0), new Vector2(wallThickness, size.y), size.y);

            // West side (door - visual only, no collider)
            CreateDoorSide("WestDoor", garage.transform,
                new Vector2(-halfW, 0), new Vector2(wallThickness, size.y));
        }

        // Internal cover/crate inside the garage
        CreateCover("GarageCrate", garage.transform,
            new Vector2(0, 0), new Vector2(48f, 48f), "crate");
    }

    /// <summary>
    /// Creates a wall segment with two window openings (visual gaps).
    /// The wall is split into 3 segments: bottom, middle (between windows), top.
    /// Window sprites are placed at the gaps.
    /// </summary>
    private void CreateWallWithWindows(string name, Transform parent, Vector2 localPos, Vector2 size, float wallHeight)
    {
        float windowHeight = 30f;
        float windowGap = 10f;

        // Three solid segments: below window 1, between windows, above window 2
        float segmentHeight = (wallHeight - 2f * windowHeight - 2f * windowGap) / 3f;

        // Bottom segment
        float bottomY = localPos.y - wallHeight / 2f + segmentHeight / 2f;
        CreateWall(name + "_Bottom", parent,
            new Vector2(localPos.x, bottomY),
            new Vector2(size.x, segmentHeight));

        // Middle segment (centered between the two windows)
        float middleHeight = wallHeight - 2f * (segmentHeight + windowGap + windowHeight);
        middleHeight = Mathf.Max(middleHeight, 10f);
        CreateWall(name + "_Middle", parent,
            new Vector2(localPos.x, localPos.y),
            new Vector2(size.x, middleHeight));

        // Top segment
        float topY = localPos.y + wallHeight / 2f - segmentHeight / 2f;
        CreateWall(name + "_Top", parent,
            new Vector2(localPos.x, topY),
            new Vector2(size.x, segmentHeight));

        // Window visual sprites (no colliders, just decorative)
        float win1Y = localPos.y - wallHeight / 2f + segmentHeight + windowGap + windowHeight / 2f;
        float win2Y = localPos.y + wallHeight / 2f - segmentHeight - windowGap - windowHeight / 2f;

        CreateWindowSprite(name + "_Window1", parent, new Vector2(localPos.x, win1Y), new Vector2(size.x + 4f, windowHeight));
        CreateWindowSprite(name + "_Window2", parent, new Vector2(localPos.x, win2Y), new Vector2(size.x + 4f, windowHeight));
    }

    /// <summary>
    /// Creates a visual-only window sprite (no collider).
    /// </summary>
    private void CreateWindowSprite(string name, Transform parent, Vector2 localPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.Instance.GetSprite("window");
        sr.sortingLayerName = LayerCover;
        sr.sortingOrder = OrderWall;

        // Scale sprite to match the desired size
        if (sr.sprite != null)
        {
            float scaleX = size.x / sr.sprite.bounds.size.x;
            float scaleY = size.y / sr.sprite.bounds.size.y;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    /// <summary>
    /// Creates the door side of a garage (visual only, no collider, so players can enter).
    /// </summary>
    private void CreateDoorSide(string name, Transform parent, Vector2 localPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.Instance.GetSprite("wall_door");
        sr.sortingLayerName = LayerCover;
        sr.sortingOrder = OrderWall;
        sr.color = new Color(0.6f, 0.5f, 0.4f, 0.5f); // Semi-transparent to indicate door

        // Scale to match size
        if (sr.sprite != null)
        {
            float scaleX = size.x / sr.sprite.bounds.size.x;
            float scaleY = size.y / sr.sprite.bounds.size.y;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // No collider - this is the entry point
    }

    // ========================================================================
    // CENTER OBSTACLES (cross formation + corner crates)
    // ========================================================================

    private void BuildCenterObstacles()
    {
        GameObject centerParent = new GameObject("CenterObstacles");
        centerParent.transform.SetParent(mapRoot);

        // Vertical cover (cross vertical bar)
        CreateCover("CenterVertical", centerParent.transform,
            new Vector2(0, 0), new Vector2(32f, 200f), "cover");

        // Horizontal cover (cross horizontal bar)
        CreateCover("CenterHorizontal", centerParent.transform,
            new Vector2(0, 0), new Vector2(200f, 32f), "cover");

        // Four corner crates
        CreateCover("Crate_TL", centerParent.transform,
            new Vector2(-150f, 150f), new Vector2(64f, 64f), "crate");

        CreateCover("Crate_TR", centerParent.transform,
            new Vector2(150f, 150f), new Vector2(64f, 64f), "crate");

        CreateCover("Crate_BL", centerParent.transform,
            new Vector2(-150f, -150f), new Vector2(64f, 64f), "crate");

        CreateCover("Crate_BR", centerParent.transform,
            new Vector2(150f, -150f), new Vector2(64f, 64f), "crate");
    }

    // ========================================================================
    // EXTERNAL COVER (10 positions around the map)
    // ========================================================================

    private void BuildExternalCover()
    {
        GameObject coverParent = new GameObject("ExternalCover");
        coverParent.transform.SetParent(mapRoot);

        // Sandbag positions (80x30)
        CreateCover("Sandbag_NW", coverParent.transform,
            new Vector2(-300f, 400f), new Vector2(80f, 30f), "sandbag");

        CreateCover("Sandbag_NE", coverParent.transform,
            new Vector2(300f, 400f), new Vector2(80f, 30f), "sandbag");

        CreateCover("Sandbag_SW", coverParent.transform,
            new Vector2(-300f, -400f), new Vector2(80f, 30f), "sandbag");

        CreateCover("Sandbag_SE", coverParent.transform,
            new Vector2(300f, -400f), new Vector2(80f, 30f), "sandbag");

        CreateCover("Sandbag_MidL", coverParent.transform,
            new Vector2(-150f, 200f), new Vector2(80f, 30f), "sandbag");

        CreateCover("Sandbag_MidR", coverParent.transform,
            new Vector2(150f, 200f), new Vector2(80f, 30f), "sandbag");

        // Crate positions (64x64)
        CreateCover("Crate_West", coverParent.transform,
            new Vector2(-650f, 0), new Vector2(64f, 64f), "crate");

        CreateCover("Crate_East", coverParent.transform,
            new Vector2(650f, 0), new Vector2(64f, 64f), "crate");

        // Barrel positions (40x50)
        CreateCover("Barrel_North", coverParent.transform,
            new Vector2(0, 400f), new Vector2(40f, 50f), "barrel");

        CreateCover("Barrel_South", coverParent.transform,
            new Vector2(0, -400f), new Vector2(40f, 50f), "barrel");
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Creates a solid wall with sprite renderer and box collider.
    /// Used for boundary walls and garage structural walls.
    /// Tag: "Wall"
    /// </summary>
    private GameObject CreateWall(string name, Transform parent, Vector2 localPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.tag = "Wall";

        // Sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.Instance.GetSprite("wall");
        sr.sortingLayerName = LayerCover;
        sr.sortingOrder = OrderWall;

        // Scale sprite to desired world size
        if (sr.sprite != null)
        {
            float scaleX = size.x / sr.sprite.bounds.size.x;
            float scaleY = size.y / sr.sprite.bounds.size.y;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Collider - keep localScale at 1 and set size directly in local units
        go.transform.localScale = Vector3.one;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        return go;
    }

    /// <summary>
    /// Creates a cover object with sprite renderer and box collider.
    /// Used for sandbags, crates, barrels, and other map cover.
    /// Tag: "Cover"
    /// </summary>
    private GameObject CreateCover(string name, Transform parent, Vector2 localPos, Vector2 size, string spriteName)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.tag = "Cover";

        // Sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.Instance.GetSprite(spriteName);
        sr.sortingLayerName = LayerCover;
        sr.sortingOrder = OrderCover;

        // Scale sprite to desired size
        if (sr.sprite != null)
        {
            float scaleX = size.x / sr.sprite.bounds.size.x;
            float scaleY = size.y / sr.sprite.bounds.size.y;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Collider matching the visual size
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        return go;
    }

    // ========================================================================
    // PUBLIC ACCESSORS
    // ========================================================================

    /// <summary>
    /// Returns the red team spawn points array.
    /// </summary>
    public Vector2[] GetRedSpawnPoints()
    {
        return redSpawnPoints;
    }

    /// <summary>
    /// Returns the blue team spawn points array.
    /// </summary>
    public Vector2[] GetBlueSpawnPoints()
    {
        return blueSpawnPoints;
    }
}
