using UnityEngine;

/// <summary>
/// 玩家视觉组件 - 创建伪3D风格的2D角色
/// 包含: 头部(圆)、躯干(矩形)、四肢(线段)、阴影
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [Header("颜色设置")]
    public Color bodyColor = Color.white;
    public Color headColor = Color.white;
    public Color teamColor = new Color(0.9f, 0.3f, 0.3f, 1f); // 红色队伍

    [Header("身体比例")]
    public float headRadius = 0.15f;
    public float torsoWidth = 0.15f;
    public float torsoHeight = 0.3f;
    public float limbLength = 0.2f;

    [Header("阴影")]
    public Color shadowColor = new Color(0, 0, 0, 0.3f);
    public float shadowRadius = 0.3f;

    [Header("武器")]
    public GameObject weaponModel;

    private SpriteRenderer headRenderer;
    private SpriteRenderer torsoRenderer;
    private SpriteRenderer[] limbRenderers = new SpriteRenderer[4];
    private SpriteRenderer shadowRenderer;
    private Transform weaponMount;

    void Start()
    {
        CreateVisuals();
    }

    void CreateVisuals()
    {
        // 创建阴影
        CreateShadow();

        // 创建头部 (圆形)
        CreateHead();

        // 创建躯干 (矩形)
        CreateTorso();

        // 创建四肢
        CreateLimbs();

        // 创建武器挂载点
        CreateWeaponMount();
    }

    void CreateShadow()
    {
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(transform);
        shadow.transform.localPosition = new Vector3(0, -0.1f, 0);
        shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = Sprite.Create(
            CreateCircleTexture(Mathf.RoundToInt(shadowRadius * 100) * 2),
            new Rect(0, 0, Mathf.RoundToInt(shadowRadius * 100) * 2, Mathf.RoundToInt(shadowRadius * 100) * 2),
            new Vector2(0.5f, 0.5f),
            100
        );
        shadowRenderer.color = shadowColor;
        shadowRenderer.sortingOrder = -1;
    }

    void CreateHead()
    {
        GameObject head = new GameObject("Head");
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 0.35f, 0);
        headRenderer = head.AddComponent<SpriteRenderer>();
        headRenderer.sprite = Sprite.Create(
            CreateCircleTexture(Mathf.RoundToInt(headRadius * 100) * 2),
            new Rect(0, 0, Mathf.RoundToInt(headRadius * 100) * 2, Mathf.RoundToInt(headRadius * 100) * 2),
            new Vector2(0.5f, 0.5f),
            100
        );
        headRenderer.color = headColor;
        headRenderer.sortingOrder = 2;
    }

    void CreateTorso()
    {
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(transform);
        torso.transform.localPosition = new Vector3(0, 0.1f, 0);
        torsoRenderer = torso.AddComponent<SpriteRenderer>();
        torsoRenderer.sprite = Sprite.Create(
            CreateRectangleTexture(Mathf.RoundToInt(torsoWidth * 100), Mathf.RoundToInt(torsoHeight * 100)),
            new Rect(0, 0, Mathf.RoundToInt(torsoWidth * 100), Mathf.RoundToInt(torsoHeight * 100)),
            new Vector2(0.5f, 0.5f),
            100
        );
        torsoRenderer.color = bodyColor;
        torsoRenderer.sortingOrder = 1;
    }

    void CreateLimbs()
    {
        string[] limbNames = { "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
        Vector2[] limbPositions = {
            new Vector2(-0.12f, 0.05f),
            new Vector2(0.12f, 0.05f),
            new Vector2(-0.08f, -0.15f),
            new Vector2(0.08f, -0.15f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject limb = new GameObject(limbNames[i]);
            limb.transform.SetParent(transform);
            limb.transform.localPosition = limbPositions[i];
            limb.transform.localRotation = Quaternion.Euler(0, 0, (i < 2) ? -20f : -10f);

            limbRenderers[i] = limb.AddComponent<SpriteRenderer>();
            int size = Mathf.RoundToInt(limbLength * 100);
            limbRenderers[i].sprite = Sprite.Create(
                CreateRectangleTexture(4, size),
                new Rect(0, 0, 4, size),
                new Vector2(0.5f, 0),
                100
            );
            limbRenderers[i].color = bodyColor;
            limbRenderers[i].sortingOrder = 1;
        }
    }

    void CreateWeaponMount()
    {
        weaponMount = new GameObject("WeaponMount").transform;
        weaponMount.SetParent(transform);
        weaponMount.localPosition = new Vector3(0.2f, 0.1f, 0);
    }

    /// <summary>
    /// 设置队伍颜色
    /// </summary>
    public void SetTeamColor(Color color)
    {
        teamColor = color;
        if (headRenderer != null) headRenderer.color = Color.Lerp(headColor, color, 0.3f);
        if (torsoRenderer != null) torsoRenderer.color = Color.Lerp(bodyColor, color, 0.2f);
    }

    /// <summary>
    /// 面向方向旋转 ( degrees)
    /// </summary>
    public void FaceDirection(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 创建圆形纹理
    /// </summary>
    Texture2D CreateCircleTexture(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] colors = new Color[resolution * resolution];
        int center = resolution / 2;
        float radius = center;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                colors[y * resolution + x] = (dist <= radius) ? Color.white : Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// 创建矩形纹理
    /// </summary>
    Texture2D CreateRectangleTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }
}
