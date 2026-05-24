using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 小地图控制器 - 在小地图图像上渲染玩家点和结构
/// 100px 对应 1600 世界单位
/// </summary>
public class MinimapController : MonoBehaviour
{
    private Image mapImage;
    private Texture2D mapTexture;
    private int mapSize;
    private float worldSize;
    private float scale; // pixels per world unit

    // 颜色常量
    private static readonly Color FriendlyColor = new Color(0.2f, 0.9f, 0.2f, 1f);
    private static readonly Color EnemyColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    private static readonly Color SelfColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color WallColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    private static readonly Color BackgroundColor = new Color(0.1f, 0.15f, 0.1f, 0.6f);

    // 结构物位置（世界坐标中的矩形）
    private List<Rect> structures = new List<Rect>();

    /// <summary>
    /// 初始化小地图
    /// </summary>
    public void Initialize(Image image, int size, float worldExtent)
    {
        mapImage = image;
        mapSize = size;
        worldSize = worldExtent;
        scale = mapSize / worldSize;

        // 创建纹理
        mapTexture = new Texture2D(mapSize, mapSize);
        mapTexture.filterMode = FilterMode.Point;

        // 设置默认结构（示例墙壁）
        AddDefaultStructures();

        // 创建精灵
        UpdateSprite();
    }

    /// <summary>
    /// 添加默认示例结构
    /// </summary>
    private void AddDefaultStructures()
    {
        // 中央掩体
        structures.Add(new Rect(-100, -50, 200, 100));
        // 左侧墙壁
        structures.Add(new Rect(-600, -300, 40, 600));
        // 右侧墙壁
        structures.Add(new Rect(560, -300, 40, 600));
        // 上方掩体
        structures.Add(new Rect(-200, 300, 400, 30));
        // 下方掩体
        structures.Add(new Rect(-200, -330, 400, 30));
    }

    /// <summary>
    /// 更新小地图显示
    /// </summary>
    public void UpdateMap(Vector2[] friendlyPositions, Vector2[] enemyPositions, Vector2 playerPos)
    {
        if (mapTexture == null) return;

        // 清空为背景色
        FillTexture(BackgroundColor);

        // 绘制结构物
        foreach (Rect structure in structures)
        {
            DrawStructure(structure);
        }

        // 绘制友方点
        if (friendlyPositions != null)
        {
            foreach (Vector2 pos in friendlyPositions)
            {
                DrawDot(WorldToMap(pos), FriendlyColor);
            }
        }

        // 绘制敌方点
        if (enemyPositions != null)
        {
            foreach (Vector2 pos in enemyPositions)
            {
                DrawDot(WorldToMap(pos), EnemyColor);
            }
        }

        // 绘制自己（白色，稍大）
        DrawDot(WorldToMap(playerPos), SelfColor, 2);

        UpdateSprite();
    }

    /// <summary>
    /// 世界坐标转小地图坐标
    /// </summary>
    private Vector2Int WorldToMap(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x + worldSize * 0.5f) * scale);
        int y = Mathf.RoundToInt((worldPos.y + worldSize * 0.5f) * scale);
        x = Mathf.Clamp(x, 0, mapSize - 1);
        y = Mathf.Clamp(y, 0, mapSize - 1);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 填充纹理为单一颜色
    /// </summary>
    private void FillTexture(Color color)
    {
        Color[] pixels = mapTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        mapTexture.SetPixels(pixels);
    }

    /// <summary>
    /// 绘制3x3（或指定大小）的点
    /// </summary>
    private void DrawDot(Vector2Int center, Color color, int radius = 1)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int px = center.x + dx;
                int py = center.y + dy;
                if (px >= 0 && px < mapSize && py >= 0 && py < mapSize)
                {
                    mapTexture.SetPixel(px, py, color);
                }
            }
        }
    }

    /// <summary>
    /// 绘制结构物矩形
    /// </summary>
    private void DrawStructure(Rect worldRect)
    {
        Vector2Int min = WorldToMap(new Vector2(worldRect.xMin, worldRect.yMin));
        Vector2Int max = WorldToMap(new Vector2(worldRect.xMax, worldRect.yMax));

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                if (x >= 0 && x < mapSize && y >= 0 && y < mapSize)
                {
                    mapTexture.SetPixel(x, y, WallColor);
                }
            }
        }
    }

    /// <summary>
    /// 将纹理应用为精灵
    /// </summary>
    private void UpdateSprite()
    {
        mapTexture.Apply();
        Sprite sprite = Sprite.Create(
            mapTexture,
            new Rect(0, 0, mapSize, mapSize),
            new Vector2(0.5f, 0.5f),
            100f
        );
        if (mapImage != null)
        {
            mapImage.sprite = sprite;
        }
    }

    /// <summary>
    /// 添加自定义结构物
    /// </summary>
    public void AddStructure(Rect worldRect)
    {
        structures.Add(worldRect);
    }

    /// <summary>
    /// 清除所有自定义结构物
    /// </summary>
    public void ClearStructures()
    {
        structures.Clear();
    }

    private void OnDestroy()
    {
        if (mapTexture != null)
        {
            Destroy(mapTexture);
        }
    }
}