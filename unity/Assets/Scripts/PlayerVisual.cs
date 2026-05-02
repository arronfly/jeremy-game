using UnityEngine;

/// <summary>
/// Handles player visual representation - creates 3D-style character
/// Head (circle), torso (ellipse), limbs (lines), shadow
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color teamColor = Color.red;
    public float shadowOffset = 0.1f;

    private GameObject head;
    private GameObject torso;
    private GameObject leftArm;
    private GameObject rightArm;
    private GameObject leftLeg;
    private GameObject rightLeg;
    private GameObject weapon;
    private GameObject shadow;

    private static readonly Color DefaultSkinColor = new Color(1f, 0.87f, 0.71f);

    void Start()
    {
        CreateVisuals();
    }

    void CreateVisuals()
    {
        // Create shadow
        shadow = CreateCircle("Shadow", Color.black, 0.3f, new Vector2(0, -0.4f));
        // Make shadow semi-transparent
        SpriteRenderer shadowSR = shadow.GetComponent<SpriteRenderer>();
        if (shadowSR != null)
        {
            Color shadowColor = shadowSR.color;
            shadowColor.a = 0.3f;
            shadowSR.color = shadowColor;
        }

        // Create torso (ellipse)
        torso = CreateEllipse("Torso", teamColor, new Vector2(0, 0), 0.15f, 0.2f);

        // Create head
        head = CreateCircle("Head", DefaultSkinColor, 0.12f, new Vector2(0, 0.2f));

        // Create arms
        leftArm = CreateLine("LeftArm", teamColor, new Vector2(-0.2f, 0.05f), new Vector2(-0.35f, -0.15f));
        rightArm = CreateLine("RightArm", teamColor, new Vector2(0.2f, 0.05f), new Vector2(0.35f, -0.15f));

        // Create legs
        leftLeg = CreateLine("LeftLeg", teamColor, new Vector2(-0.1f, -0.2f), new Vector2(-0.15f, -0.45f));
        rightLeg = CreateLine("RightLeg", teamColor, new Vector2(0.1f, -0.2f), new Vector2(0.15f, -0.45f));

        // Create weapon
        weapon = CreateLine("Weapon", Color.gray, new Vector2(0, 0), new Vector2(0.4f, 0));

        // Set sorting order so player renders correctly
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 1;
            }
        }
    }

    /// <summary>
    /// Creates a circular sprite using Unity's Sprite.Create with generated texture
    /// </summary>
    GameObject CreateCircle(string name, Color color, float size, Vector2 offset)
    {
        GameObject obj = new GameObject(name);

        // Generate circle texture programmatically
        Texture2D texture = GenerateCircleTexture(Mathf.CeilToInt(size * 64), color);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            64f
        );

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        obj.transform.parent = transform;
        obj.transform.localPosition = offset;

        // Set local scale to achieve desired size
        float pixelsPerUnit = 64f;
        obj.transform.localScale = new Vector3(size * 2, size * 2, 1f);

        return obj;
    }

    /// <summary>
    /// Creates an ellipse (oval) sprite using generated texture
    /// </summary>
    GameObject CreateEllipse(string name, Color color, Vector2 center, float width, float height)
    {
        GameObject obj = new GameObject(name);

        // Generate ellipse texture programmatically
        Texture2D texture = GenerateEllipseTexture(Mathf.CeilToInt(width * 64), Mathf.CeilToInt(height * 64), color);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            64f
        );

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        obj.transform.parent = transform;
        obj.transform.localPosition = center;

        // Scale to desired dimensions
        obj.transform.localScale = Vector3.one;

        return obj;
    }

    /// <summary>
    /// Creates a line using LineRenderer component
    /// </summary>
    GameObject CreateLine(string name, Color color, Vector2 start, Vector2 end)
    {
        GameObject obj = new GameObject(name);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        // Create material for the line
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lr.material = lineMaterial;

        Vector3 start3D = new Vector3(start.x, start.y, 0);
        Vector3 end3D = new Vector3(end.x, end.y, 0);
        lr.SetPosition(0, start3D);
        lr.SetPosition(1, end3D);

        obj.transform.parent = transform;

        return obj;
    }

    /// <summary>
    /// Generates a circular texture with anti-aliased edges
    /// </summary>
    Texture2D GenerateCircleTexture(int diameter, Color color)
    {
        Texture2D texture = new Texture2D(diameter, diameter);
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[diameter * diameter];
        int radius = diameter / 2;
        int centerX = radius;
        int centerY = radius;

        // Use squared distance for circle test
        float radiusSq = (float)radius * radius;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                int index = y * diameter + x;
                float distSq = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);

                if (distSq <= radiusSq)
                {
                    // Anti-aliasing at edge
                    float dist = Mathf.Sqrt(distSq);
                    float t = (radius - dist) / radius;

                    if (t > 0.8f)
                    {
                        // Soft edge anti-aliasing
                        Color c = color;
                        c.a = (t - 0.8f) / 0.2f;
                        pixels[index] = c;
                    }
                    else
                    {
                        pixels[index] = color;
                    }
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Generates an ellipse texture with anti-aliased edges
    /// </summary>
    Texture2D GenerateEllipseTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        int centerX = width / 2;
        int centerY = height / 2;
        float radiusX = width / 2f;
        float radiusY = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                // Normalized distance from center
                float dx = (x - centerX) / radiusX;
                float dy = (y - centerY) / radiusY;
                float distSq = dx * dx + dy * dy;

                if (distSq <= 1f)
                {
                    // Anti-aliasing at edge
                    float dist = Mathf.Sqrt(distSq);

                    if (dist > 0.85f)
                    {
                        // Soft edge anti-aliasing
                        Color c = color;
                        c.a = 1f - (dist - 0.85f) / 0.15f;
                        pixels[index] = c;
                    }
                    else
                    {
                        pixels[index] = color;
                    }
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Rotates the body by the specified angle in degrees
    /// </summary>
    public void RotateBody(float angle)
    {
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Updates the team color for all body parts
    /// </summary>
    public void SetTeamColor(Color newColor)
    {
        teamColor = newColor;

        if (torso != null)
        {
            SpriteRenderer torsoSR = torso.GetComponent<SpriteRenderer>();
            if (torsoSR != null) torsoSR.color = teamColor;
        }
        if (leftArm != null)
        {
            LineRenderer leftArmLR = leftArm.GetComponent<LineRenderer>();
            if (leftArmLR != null)
            {
                leftArmLR.startColor = teamColor;
                leftArmLR.endColor = teamColor;
            }
        }
        if (rightArm != null)
        {
            LineRenderer rightArmLR = rightArm.GetComponent<LineRenderer>();
            if (rightArmLR != null)
            {
                rightArmLR.startColor = teamColor;
                rightArmLR.endColor = teamColor;
            }
        }
        if (leftLeg != null)
        {
            LineRenderer leftLegLR = leftLeg.GetComponent<LineRenderer>();
            if (leftLegLR != null)
            {
                leftLegLR.startColor = teamColor;
                leftLegLR.endColor = teamColor;
            }
        }
        if (rightLeg != null)
        {
            LineRenderer rightLegLR = rightLeg.GetComponent<LineRenderer>();
            if (rightLegLR != null)
            {
                rightLegLR.startColor = teamColor;
                rightLegLR.endColor = teamColor;
            }
        }
    }
}