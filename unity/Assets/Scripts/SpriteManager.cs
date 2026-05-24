using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 精灵管理器 - 程序化生成所有2D游戏视觉精灵
/// 歼灭团竞2 (2D top-down military shooter)
/// Art style: realistic/military (olive green, brown, camo, metallic tones)
/// </summary>
public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("Generation Settings")]
    [SerializeField] private int noiseSeed = 42;

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GenerateAllSprites();
        Debug.Log("SpriteManager: All sprites generated. Cache count: " + spriteCache.Count);
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    /// <summary>
    /// Generate every sprite the game needs. Called once at startup.
    /// </summary>
    public void GenerateAllSprites()
    {
        GenerateMapSprites();
        GenerateCharacterSprites();
        GenerateWeaponSprites();
        GenerateEffectSprites();
        GenerateUISprites();
    }

    /// <summary>
    /// Retrieve a cached sprite by name. Returns null if not found.
    /// </summary>
    public Sprite GetSprite(string name)
    {
        if (spriteCache.TryGetValue(name, out Sprite sprite))
            return sprite;

        Debug.LogWarning("SpriteManager: Sprite '" + name + "' not found in cache.");
        return null;
    }

    // ──────────────────────────────────────────────
    // Noise utility
    // ──────────────────────────────────────────────

    /// <summary>
    /// Simple sine-based pseudo-random noise returning 0-1.
    /// </summary>
    private float Noise(float x, float y)
    {
        float dot = x * 12.9898f + y * 78.233f + noiseSeed;
        float s = Mathf.Sin(dot) * 43758.5453f;
        return s - Mathf.Floor(s);
    }

    // ──────────────────────────────────────────────
    // Core texture helpers
    // ──────────────────────────────────────────────

    private Texture2D CreateTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private Sprite Finalize(Texture2D tex)
    {
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
    }

    private void Cache(string name, Sprite sprite)
    {
        spriteCache[name] = sprite;
    }

    // ──────────────────────────────────────────────
    // MAP SPRITES
    // ──────────────────────────────────────────────

    private void GenerateMapSprites()
    {
        Cache("ground",       MakeGround());
        Cache("wall",         MakeWall());
        Cache("garage_wall",  MakeGarageWall());
        Cache("sandbag",      MakeSandbag());
        Cache("crate",        MakeCrate());
        Cache("barrel",       MakeBarrel());
        Cache("window",       MakeWindow());
        Cache("boundary",     MakeBoundary());
    }

    private Sprite MakeGround()
    {
        int size = 200;
        Texture2D tex = CreateTexture(size, size);
        Color baseColor = HexColor("#3A3A3A");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Noise(x * 0.1f, y * 0.1f);
                float variation = (n - 0.5f) * 0.15f;
                tex.SetPixel(x, y, Tint(baseColor, variation));
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeWall()
    {
        int w = 64, h = 64;
        Texture2D tex = CreateTexture(w, h);
        Color baseColor = HexColor("#666666");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Noise(x * 0.15f, y * 0.15f);
                float variation = (n - 0.5f) * 0.12f;
                Color c = Tint(baseColor, variation);

                // Edge shadow (top 2 pixels darker, bottom 2 lighter)
                if (y >= h - 2)
                    c = Tint(c, -0.08f);
                else if (y < 2)
                    c = Tint(c, 0.06f);

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeGarageWall()
    {
        int w = 64, h = 64;
        Texture2D tex = CreateTexture(w, h);
        Color baseColor = HexColor("#555555");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Noise(x * 0.12f, y * 0.12f);
                float variation = (n - 0.5f) * 0.08f;
                Color c = Tint(baseColor, variation);

                // Horizontal rivet lines every 8 pixels
                if (y % 8 < 1)
                {
                    c = Tint(c, -0.12f);
                }

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeSandbag()
    {
        int w = 60, h = 30;
        Texture2D tex = CreateTexture(w, h);
        Color baseColor = HexColor("#D2B48C");
        int bagH = h / 3; // 3 rows of stacked bags

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Determine which bag row
                int row = y / bagH;
                int offset = (row % 2 == 0) ? 0 : w / 4;

                // Bag boundaries within the row
                int localY = y % bagH;
                int bagW = w / 2;
                int localX = ((x + offset) % w) % bagW;

                // Rounded bag shape check
                float nx = (float)localX / bagW;
                float ny = (float)localY / bagH;
                float edgeDist = Mathf.Min(nx, 1f - nx, ny, 1f - ny);

                float n = Noise(x * 0.2f, y * 0.2f);
                float variation = (n - 0.5f) * 0.1f;
                Color c = Tint(baseColor, variation);

                // Darken edges of each bag
                if (edgeDist < 0.12f)
                    c = Tint(c, -0.15f);

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeCrate()
    {
        int size = 64;
        Texture2D tex = CreateTexture(size, size);
        Color woodColor = HexColor("#8B4513");
        Color strapColor = HexColor("#5C3317");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Noise(x * 0.15f, y * 0.15f);
                float variation = (n - 0.5f) * 0.1f;
                Color c = Tint(woodColor, variation);

                // Cross straps (2px wide diagonal lines)
                bool onDiag1 = Mathf.Abs(x - y) < 2;
                bool onDiag2 = Mathf.Abs(x - (size - 1 - y)) < 2;

                // Horizontal & vertical center straps
                bool onH = Mathf.Abs(y - size / 2) < 2;
                bool onV = Mathf.Abs(x - size / 2) < 2;

                if (onDiag1 || onDiag2 || onH || onV)
                    c = strapColor;

                // Edge border
                if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                    c = Tint(woodColor, -0.2f);

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeBarrel()
    {
        int w = 40, h = 50;
        Texture2D tex = CreateTexture(w, h);
        Color barrelColor = HexColor("#B22222");
        Color bandColor = HexColor("#8B0000");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Elliptical shape
                float cx = (x - w / 2f) / (w / 2f);
                float cy = (y - h / 2f) / (h / 2f);
                bool inside = (cx * cx + cy * cy) <= 1f;

                if (!inside)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                float n = Noise(x * 0.2f, y * 0.15f);
                float variation = (n - 0.5f) * 0.08f;
                Color c = Tint(barrelColor, variation);

                // Metal bands at 20%, 50%, 80% height
                float yPct = (float)y / h;
                if (Mathf.Abs(yPct - 0.2f) < 0.03f ||
                    Mathf.Abs(yPct - 0.5f) < 0.03f ||
                    Mathf.Abs(yPct - 0.8f) < 0.03f)
                {
                    c = bandColor;
                }

                // Specular highlight
                if (cx < -0.1f && cx > -0.4f && cy > -0.3f && cy < 0.3f)
                {
                    c = Tint(c, 0.12f);
                }

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeWindow()
    {
        int w = 80, h = 24;
        Texture2D tex = CreateTexture(w, h);
        Color glassColor = new Color(0.6f, 0.8f, 1f, 0.5f);
        Color crackColor = new Color(0.8f, 0.85f, 0.9f, 0.7f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Noise(x * 0.3f, y * 0.3f);
                Color c = glassColor;

                // Crack pattern: jagged lines from center
                int cx = w / 2, cy = h / 2;
                int dx = x - cx, dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Cracks along specific angles
                float crackLine = Mathf.Sin(angle * 3f + dist * 0.2f);
                if (crackLine > 0.85f && dist < 30f)
                    c = crackColor;

                // Frame border
                if (x < 2 || x >= w - 2 || y < 2 || y >= h - 2)
                    c = HexColor("#444444");

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeBoundary()
    {
        int w = 64, h = 16;
        Texture2D tex = CreateTexture(w, h);
        Color darkGray = HexColor("#2A2A2A");
        Color hazardYellow = HexColor("#FFCC00");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Hazard stripe pattern (diagonal)
                bool stripe = ((x + y) / 6) % 2 == 0;
                Color c = stripe ? hazardYellow : darkGray;

                // Noise
                float n = Noise(x * 0.1f, y * 0.1f);
                c = Tint(c, (n - 0.5f) * 0.06f);

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    // ──────────────────────────────────────────────
    // CHARACTER SPRITES
    // ──────────────────────────────────────────────

    private void GenerateCharacterSprites()
    {
        Cache("head",       MakeHead());
        Cache("torso_red",  MakeTorso(HexColor("#8B3A3A")));
        Cache("torso_blue", MakeTorso(HexColor("#3A5A8B")));
        Cache("limb_red",   MakeLimb(HexColor("#8B3A3A")));
        Cache("limb_blue",  MakeLimb(HexColor("#3A5A8B")));
        Cache("shadow",     MakeShadow());
        Cache("helmet_red", MakeHelmet(HexColor("#8B3A3A")));
        Cache("helmet_blue", MakeHelmet(HexColor("#3A5A8B")));
    }

    private Sprite MakeHead()
    {
        int size = 20;
        Texture2D tex = CreateTexture(size, size);
        Color skinColor = HexColor("#D4A574");
        int center = size / 2;
        int radius = size / 2 - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    Color c = skinColor;

                    // Subtle noise for skin variation
                    float n = Noise(x * 0.3f, y * 0.3f);
                    c = Tint(c, (n - 0.5f) * 0.06f);

                    // Helmet rim (top arc)
                    if (y >= size - 3 && dist <= radius)
                    {
                        c = Tint(c, -0.15f);
                    }

                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeTorso(Color spotColor)
    {
        int w = 20, h = 30;
        Texture2D tex = CreateTexture(w, h);
        Color baseColor = HexColor("#4A5D23");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Noise(x * 0.25f, y * 0.25f);
                Color c = baseColor;

                // Camo: blend spot color where noise > 0.5
                if (n > 0.5f)
                {
                    float blend = (n - 0.5f) * 2f;
                    c = Color.Lerp(baseColor, spotColor, blend);
                }

                // Add additional micro noise
                float micro = Noise(x * 0.8f, y * 0.8f);
                c = Tint(c, (micro - 0.5f) * 0.06f);

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeLimb(Color spotColor)
    {
        int w = 4, h = 20;
        Texture2D tex = CreateTexture(w, h);
        Color baseColor = HexColor("#4A5D23");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Noise(x * 0.3f, y * 0.3f);
                Color c = baseColor;

                if (n > 0.5f)
                {
                    float blend = (n - 0.5f) * 2f;
                    c = Color.Lerp(baseColor, spotColor, blend);
                }

                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeShadow()
    {
        int w = 30, h = 12;
        Texture2D tex = CreateTexture(w, h);
        Color shadowColor = new Color(0, 0, 0, 0.35f);
        int cx = w / 2, cy = h / 2;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (float)(x - cx) / (w / 2f);
                float dy = (float)(y - cy) / (h / 2f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= 1f)
                {
                    float alpha = 0.35f * (1f - dist * 0.5f);
                    tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeHelmet(Color tintColor)
    {
        int w = 16, h = 8;
        Texture2D tex = CreateTexture(w, h);
        Color baseOlive = HexColor("#4A5D23");
        Color helmetColor = Color.Lerp(baseOlive, tintColor, 0.3f);
        int cx = w / 2, cy = h / 2;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (float)(x - cx) / (w / 2f);
                float dy = (float)(y - cy) / (h / 2f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= 1f)
                {
                    float n = Noise(x * 0.3f, y * 0.3f);
                    Color c = Tint(helmetColor, (n - 0.5f) * 0.08f);

                    // Top highlight
                    if (dy < -0.3f)
                        c = Tint(c, 0.08f);

                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    // ──────────────────────────────────────────────
    // WEAPON SPRITES
    // ──────────────────────────────────────────────

    private void GenerateWeaponSprites()
    {
        Cache("weapon_ar",      MakeWeaponAR());
        Cache("weapon_sniper",  MakeWeaponSniper());
        Cache("weapon_smg",     MakeWeaponSMG());
        Cache("weapon_shotgun", MakeWeaponShotgun());
    }

    private Sprite MakeWeaponAR()
    {
        int w = 40, h = 10;
        Texture2D tex = CreateTexture(w, h);
        Color metal = HexColor("#444444");
        Color darkMetal = HexColor("#333333");
        Color grip = HexColor("#3A3A3A");

        ClearTransparent(tex, w, h);

        // Stock (x: 0-8)
        FillRect(tex, 0, 3, 8, 4, darkMetal);
        SetPixelSafe(tex, 7, 3, metal);

        // Body/receiver (x: 8-25)
        FillRect(tex, 8, 2, 17, 6, metal);
        // Detail line
        FillRect(tex, 8, 4, 17, 2, Tint(metal, 0.04f));

        // Barrel (x: 25-38)
        FillRect(tex, 25, 3, 13, 4, darkMetal);
        FillRect(tex, 25, 4, 13, 2, metal);

        // Muzzle (x: 38-40)
        FillRect(tex, 38, 3, 2, 4, Tint(darkMetal, -0.05f));

        // Grip (bottom)
        FillRect(tex, 16, 7, 4, 3, grip);

        // Magazine
        FillRect(tex, 20, 6, 3, 3, Tint(darkMetal, -0.08f));

        // Sight post
        SetPixelSafe(tex, 24, 1, metal);
        SetPixelSafe(tex, 24, 2, metal);

        return Finalize(tex);
    }

    private Sprite MakeWeaponSniper()
    {
        int w = 55, h = 8;
        Texture2D tex = CreateTexture(w, h);
        Color metal = HexColor("#3D3D3D");
        Color darkMetal = HexColor("#2E2E2E");
        Color grip = HexColor("#383838");

        ClearTransparent(tex, w, h);

        // Stock (x: 0-10)
        FillRect(tex, 0, 2, 10, 4, darkMetal);
        FillRect(tex, 2, 3, 6, 2, metal);

        // Body/receiver (x: 10-30)
        FillRect(tex, 10, 2, 20, 4, metal);
        FillRect(tex, 10, 3, 20, 2, Tint(metal, 0.03f));

        // Long barrel (x: 30-52)
        FillRect(tex, 30, 3, 22, 2, darkMetal);

        // Muzzle brake (x: 52-55)
        FillRect(tex, 52, 2, 3, 4, Tint(darkMetal, -0.06f));

        // Scope
        FillRect(tex, 15, 0, 8, 2, metal);
        SetPixelSafe(tex, 14, 1, darkMetal);
        SetPixelSafe(tex, 23, 1, darkMetal);

        // Bolt handle
        SetPixelSafe(tex, 26, 1, metal);

        // Grip
        FillRect(tex, 20, 6, 3, 2, grip);

        // Magazine
        FillRect(tex, 24, 5, 2, 2, darkMetal);

        return Finalize(tex);
    }

    private Sprite MakeWeaponSMG()
    {
        int w = 30, h = 10;
        Texture2D tex = CreateTexture(w, h);
        Color metal = HexColor("#484848");
        Color darkMetal = HexColor("#363636");
        Color grip = HexColor("#3C3C3C");

        ClearTransparent(tex, w, h);

        // Folding stock (x: 0-6)
        FillRect(tex, 0, 4, 6, 2, darkMetal);

        // Body (x: 6-18)
        FillRect(tex, 6, 3, 12, 5, metal);
        FillRect(tex, 6, 4, 12, 3, Tint(metal, 0.03f));

        // Short barrel (x: 18-28)
        FillRect(tex, 18, 4, 10, 3, darkMetal);

        // Muzzle
        FillRect(tex, 28, 4, 2, 3, Tint(darkMetal, -0.05f));

        // Grip
        FillRect(tex, 10, 8, 3, 2, grip);

        // Magazine (short, forward)
        FillRect(tex, 14, 7, 2, 3, darkMetal);

        return Finalize(tex);
    }

    private Sprite MakeWeaponShotgun()
    {
        int w = 35, h = 12;
        Texture2D tex = CreateTexture(w, h);
        Color metal = HexColor("#4A4A4A");
        Color darkMetal = HexColor("#383838");
        Color wood = HexColor("#5C3A1E");
        Color grip = HexColor("#3C3C3C");

        ClearTransparent(tex, w, h);

        // Wooden stock (x: 0-10)
        FillRect(tex, 0, 3, 10, 6, wood);
        FillRect(tex, 1, 4, 8, 4, Tint(wood, 0.05f));

        // Body/receiver (x: 10-20)
        FillRect(tex, 10, 3, 10, 6, metal);
        FillRect(tex, 10, 4, 10, 4, Tint(metal, 0.03f));

        // Wide barrel (x: 20-33)
        FillRect(tex, 20, 4, 13, 4, darkMetal);
        FillRect(tex, 20, 5, 13, 2, metal);

        // Muzzle
        FillRect(tex, 33, 4, 2, 4, Tint(darkMetal, -0.06f));

        // Pump/foregrip
        FillRect(tex, 22, 8, 5, 2, wood);

        // Rear grip
        FillRect(tex, 12, 9, 3, 3, grip);

        // Trigger guard hint
        SetPixelSafe(tex, 14, 9, darkMetal);

        return Finalize(tex);
    }

    // ──────────────────────────────────────────────
    // EFFECT SPRITES
    // ──────────────────────────────────────────────

    private void GenerateEffectSprites()
    {
        // Muzzle flash: 3 frames of expanding bright circle
        for (int i = 0; i < 3; i++)
            Cache("muzzle_flash_" + i, MakeMuzzleFlash(i));

        Cache("bullet_trail", MakeBulletTrail());

        // Blood splatter: 4 random variants
        for (int i = 0; i < 4; i++)
            Cache("blood_" + i, MakeBloodSplatter(i));

        // Explosion: 5 expanding ring frames
        for (int i = 0; i < 5; i++)
            Cache("explosion_" + i, MakeExplosionFrame(i));

        Cache("smoke",       MakeSmoke());
        Cache("flash_screen", MakeFlashScreen());
        Cache("spawn_ring",  MakeSpawnRing());
    }

    private Sprite MakeMuzzleFlash(int frame)
    {
        int size = 20;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float maxRadius = 3f + frame * 3f; // Expanding: 3, 6, 9

        Color white = Color.white;
        Color yellow = HexColor("#FFFF00");
        Color orange = HexColor("#FF8800");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = dist / maxRadius;

                if (t <= 1f)
                {
                    Color c;
                    if (t < 0.3f)
                        c = white;
                    else if (t < 0.6f)
                        c = Color.Lerp(white, yellow, (t - 0.3f) / 0.3f);
                    else
                        c = Color.Lerp(yellow, orange, (t - 0.6f) / 0.4f);

                    // Fade out alpha at edges
                    float alpha = 1f - t * t;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeBulletTrail()
    {
        int w = 30, h = 4;
        Texture2D tex = CreateTexture(w, h);
        Color white = new Color(1f, 1f, 1f, 0.9f);
        Color yellow = new Color(1f, 1f, 0.4f, 0.7f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float t = (float)x / w; // 0=left(front) to 1=right(back)
                float verticalFade = 1f - Mathf.Abs(y - h / 2f) / (h / 2f);

                Color c = Color.Lerp(white, yellow, t);
                c.a = (1f - t * 0.8f) * verticalFade;
                tex.SetPixel(x, y, c);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeBloodSplatter(int variant)
    {
        int size = 24;
        Texture2D tex = CreateTexture(size, h: size);
        Color bloodColor = HexColor("#8B0000");
        int center = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Use noise seeded by variant to create random splatter shape
                float n = Noise(x * 0.4f + variant * 7.3f, y * 0.4f + variant * 13.1f);
                float threshold = 0.35f + dist * 0.02f;

                if (n > threshold && dist < 10f)
                {
                    float alpha = 0.7f + n * 0.3f;
                    Color c = Tint(bloodColor, (n - 0.5f) * 0.15f);
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeExplosionFrame(int frame)
    {
        int size = 60;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float innerRadius = frame * 4f;
        float outerRadius = 6f + frame * 8f;

        // Color progression: white -> yellow -> orange -> red -> black/char
        Color[] ringColors = new Color[]
        {
            Color.white,
            HexColor("#FFFF00"),
            HexColor("#FF8800"),
            HexColor("#CC2200"),
            HexColor("#222222")
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (dist <= outerRadius)
                {
                    // Ring structure: outside ring is current frame color, inside fades to darker
                    float t = (dist - innerRadius) / (outerRadius - innerRadius);
                    t = Mathf.Clamp01(t);

                    Color outerColor = ringColors[frame];
                    Color innerColor = frame < 4 ? ringColors[frame + 1] : ringColors[4];
                    Color c = Color.Lerp(innerColor, outerColor, t);

                    // Add noise for irregular edge
                    float n = Noise(x * 0.2f + frame * 3f, y * 0.2f + frame * 5f);
                    if (n < 0.3f && dist > outerRadius * 0.7f)
                    {
                        c.a = 0f;
                    }

                    // Overall alpha fades as explosion progresses
                    c.a *= (1f - frame * 0.15f);

                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeSmoke()
    {
        int size = 80;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float maxRadius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;

                if (dist <= 1f)
                {
                    float n = Noise(x * 0.08f, y * 0.08f);
                    float alpha = (1f - dist) * 0.3f * (0.5f + n * 0.5f);
                    float gray = 0.5f + n * 0.2f;
                    tex.SetPixel(x, y, new Color(gray, gray, gray, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeFlashScreen()
    {
        int size = 100;
        Texture2D tex = CreateTexture(size, size);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeSpawnRing()
    {
        int size = 40;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float outerR = size / 2f - 2f;
        float innerR = outerR - 3f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (dist >= innerR && dist <= outerR)
                {
                    float t = (dist - innerR) / (outerR - innerR);
                    float glow = 1f - Mathf.Abs(t - 0.5f) * 2f;

                    Color c = Color.Lerp(new Color(0.5f, 0.7f, 1f, 0.8f), Color.white, glow);
                    tex.SetPixel(x, y, c);
                }
                else if (dist < innerR)
                {
                    // Inner glow
                    float alpha = (1f - dist / innerR) * 0.15f;
                    tex.SetPixel(x, y, new Color(0.5f, 0.7f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    // ──────────────────────────────────────────────
    // UI SPRITES
    // ──────────────────────────────────────────────

    private void GenerateUISprites()
    {
        Cache("crosshair_h",        MakeCrosshairH());
        Cache("crosshair_v",        MakeCrosshairV());
        Cache("health_bar_bg",      MakeHealthBarBg());
        Cache("minimap_border",     MakeMinimapBorder());
        Cache("weapon_slot",        MakeWeaponSlot(false));
        Cache("weapon_slot_active", MakeWeaponSlot(true));
        Cache("dot_red",            MakeDot(Color.red));
        Cache("dot_blue",           MakeDot(new Color(0.2f, 0.5f, 1f)));
        Cache("dot_white",          MakeDot(Color.white));
    }

    private Sprite MakeCrosshairH()
    {
        int w = 20, h = 4;
        Texture2D tex = CreateTexture(w, h);
        Color white = Color.white;
        Color outline = Color.black;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Gap in center (pixels 8-11)
                bool inGap = x >= 8 && x <= 11;
                // Outline is outer ring of 4px height
                bool isOutline = y == 0 || y == h - 1;

                if (inGap)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (isOutline)
                {
                    tex.SetPixel(x, y, outline);
                }
                else
                {
                    tex.SetPixel(x, y, white);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeCrosshairV()
    {
        int w = 4, h = 20;
        Texture2D tex = CreateTexture(w, h);
        Color white = Color.white;
        Color outline = Color.black;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Gap in center (pixels 8-11)
                bool inGap = y >= 8 && y <= 11;
                bool isOutline = x == 0 || x == w - 1;

                if (inGap)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else if (isOutline)
                {
                    tex.SetPixel(x, y, outline);
                }
                else
                {
                    tex.SetPixel(x, y, white);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeHealthBarBg()
    {
        int w = 200, h = 20;
        Texture2D tex = CreateTexture(w, h);
        Color bg = HexColor("#333333");
        Color border = HexColor("#1A1A1A");

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = x < 1 || x >= w - 1 || y < 1 || y >= h - 1;
                tex.SetPixel(x, y, isBorder ? border : bg);
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeMinimapBorder()
    {
        int size = 104;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float outerR = size / 2f - 1f;
        float innerR = outerR - 3f;
        Color borderColor = HexColor("#AAAAAA");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (dist >= innerR && dist <= outerR)
                {
                    tex.SetPixel(x, y, borderColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeWeaponSlot(bool active)
    {
        int size = 50;
        Texture2D tex = CreateTexture(size, size);
        Color bg = HexColor("#2A2A2A");
        Color borderNormal = HexColor("#555555");
        Color borderActive = HexColor("#FFFFFF");
        int radius = 6; // Rounded corner radius

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Rounded corner check
                bool inCorner = false;
                int[] cornersX = { radius, size - radius - 1 };
                int[] cornersY = { radius, size - radius - 1 };

                for (int cy = 0; cy < 2; cy++)
                {
                    for (int cx = 0; cx < 2; cx++)
                    {
                        bool inXCorner = (cx == 0) ? x < radius : x > size - radius - 1;
                        bool inYCorner = (cy == 0) ? y < radius : y > size - radius - 1;

                        if (inXCorner && inYCorner)
                        {
                            float dist = Vector2.Distance(
                                new Vector2(x, y),
                                new Vector2(cornersX[cx], cornersY[cy])
                            );
                            if (dist > radius)
                                inCorner = true;
                        }
                    }
                }

                if (inCorner)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                bool isBorder = x < 2 || x >= size - 2 || y < 2 || y >= size - 2;
                if (isBorder)
                {
                    tex.SetPixel(x, y, active ? borderActive : borderNormal);
                }
                else
                {
                    tex.SetPixel(x, y, bg);
                }
            }
        }
        return Finalize(tex);
    }

    private Sprite MakeDot(Color color)
    {
        int size = 6;
        Texture2D tex = CreateTexture(size, size);
        int center = size / 2;
        float radius = size / 2f - 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        return Finalize(tex);
    }

    // ──────────────────────────────────────────────
    // Utility helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Parse a hex color string (e.g. "#3A3A3A") into a Unity Color.
    /// </summary>
    private static Color HexColor(string hex)
    {
        hex = hex.Replace("#", "");
        float r = (int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)) / 255f;
        float g = (int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)) / 255f;
        float b = (int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)) / 255f;
        return new Color(r, g, b, 1f);
    }

    /// <summary>
    /// Lighten or darken a color by a signed amount (-1 to +1).
    /// </summary>
    private static Color Tint(Color c, float amount)
    {
        return new Color(
            Mathf.Clamp01(c.r + amount),
            Mathf.Clamp01(c.g + amount),
            Mathf.Clamp01(c.b + amount),
            c.a
        );
    }

    /// <summary>
    /// Fill all pixels of a texture with Color.clear.
    /// </summary>
    private static void ClearTransparent(Texture2D tex, int w, int h)
    {
        Color[] clear = new Color[w * h];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);
    }

    /// <summary>
    /// Fill a rectangular region of a texture with a solid color.
    /// </summary>
    private static void FillRect(Texture2D tex, int x0, int y0, int w, int h, Color c)
    {
        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                int px = x0 + dx;
                int py = y0 + dy;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, c);
            }
        }
    }

    /// <summary>
    /// Set a pixel only if coordinates are within the texture bounds.
    /// </summary>
    private static void SetPixelSafe(Texture2D tex, int x, int y, Color c)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            tex.SetPixel(x, y, c);
    }
}
