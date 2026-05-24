using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// HUD管理器 - 程序化构建并管理整个游戏HUD
/// 单例模式，自动创建所有UI元素
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    // === 引用 - 游戏系统 ===
    private PlayerHealth playerHealth;
    private WeaponSystem weaponSystem;
    private GrenadeSystem grenadeSystem;
    private MedkitSystem medkitSystem;
    private RoundManager roundManager;

    // === UI 根节点 ===
    private Canvas canvas;
    private RectTransform canvasRect;

    // === 顶部栏 ===
    private Image topBarBg;
    private Image redTeamIcon;
    private Text redScoreText;
    private Text roundText;
    private Image blueTeamIcon;
    private Text blueScoreText;

    // === 左下角 - 生命值 ===
    private Image healthBarBackground;
    private Image healthBarFill;
    private Text healthText;
    private Image[] medkitIcons;
    private Image[] medkitCooldownOverlays;
    private const int MEDKIT_COUNT = 3;

    // === 右下角 - 武器/弹药 ===
    private Text weaponNameText;
    private Text currentAmmoText;
    private Text reserveAmmoText;
    private Text reloadingText;
    private Image[] weaponSlotBoxes;
    private Text[] weaponSlotLabels;
    private const int WEAPON_SLOT_COUNT = 4;
    private Text grenadeTypeText;
    private Image grenadeCooldownOverlay;

    // === 右上角 - 击杀播报 ===
    private KillFeedController killFeedController;

    // === 中心 - 准心 ===
    private CrosshairController crosshairController;

    // === 中心 - 回合公告 ===
    private RoundOverlayController roundOverlayController;

    // === 左侧 - 小地图 ===
    private MinimapController minimapController;
    private Image minimapImage;
    private const int MINIMAP_SIZE = 100;
    private const float MAP_WORLD_SIZE = 1600f;

    // === 颜色常量 ===
    private static readonly Color SemiTransparentBlack = new Color(0f, 0f, 0f, 0.53f);
    private static readonly Color DarkGray = new Color(0.2f, 0.2f, 0.2f, 1f);
    private static readonly Color HealthGreen = new Color(0.15f, 0.68f, 0.38f, 1f);   // #27AE60
    private static readonly Color HealthYellow = new Color(0.95f, 0.77f, 0.06f, 1f);
    private static readonly Color HealthRed = new Color(0.9f, 0.2f, 0.2f, 1f);
    private static readonly Color RedTeamColor = new Color(0.9f, 0.25f, 0.25f, 1f);
    private static readonly Color BlueTeamColor = new Color(0.2f, 0.55f, 0.9f, 1f);
    private static readonly Color ActiveSlotColor = new Color(1f, 0.85f, 0.1f, 1f);
    private static readonly Color InactiveSlotColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);

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
        BuildHUD();
        FindPlayerComponents();
        SubscribeToRoundEvents();
    }

    private void Update()
    {
        UpdatePlayerStatus();
    }

    // =========================================================================
    //  HUD 构建
    // =========================================================================

    /// <summary>
    /// 程序化构建整个HUD界面
    /// </summary>
    private void BuildHUD()
    {
        CreateCanvas();
        BuildTopBar();
        BuildHealthSection();
        BuildWeaponSection();
        BuildKillFeed();
        BuildCrosshair();
        BuildRoundOverlay();
        BuildMinimap();
    }

    // --- Canvas ---

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 960);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        canvasRect = canvasObj.GetComponent<RectTransform>();
    }

    // --- 顶部栏 ---

    private void BuildTopBar()
    {
        // 背景条 - 顶部水平布局
        GameObject topBarObj = CreateUIObject("TopBar", canvasRect);
        RectTransform topBarRect = topBarObj.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0, 1);
        topBarRect.anchorMax = new Vector2(1, 1);
        topBarRect.pivot = new Vector2(0.5f, 1);
        topBarRect.anchoredPosition = new Vector2(0, 0);
        topBarRect.sizeDelta = new Vector2(0, 50);

        topBarBg = topBarObj.AddComponent<Image>();
        topBarBg.color = SemiTransparentBlack;

        HorizontalLayoutGroup topBarLayout = topBarObj.AddComponent<HorizontalLayoutGroup>();
        topBarLayout.padding = new RectOffset(20, 20, 10, 10);
        topBarLayout.spacing = 10;
        topBarLayout.childAlignment = TextAnchor.MiddleCenter;
        topBarLayout.childControlWidth = true;
        topBarLayout.childControlHeight = true;
        topBarLayout.childForceExpandWidth = true;
        topBarLayout.childForceExpandHeight = false;

        // 左 - 红队
        GameObject redSection = CreateUIObject("RedSection", topBarRect);
        LayoutElement redLayout = redSection.AddComponent<LayoutElement>();
        redLayout.preferredHeight = 30;
        redLayout.minWidth = 120;

        HorizontalLayoutGroup redGroup = redSection.AddComponent<HorizontalLayoutGroup>();
        redGroup.spacing = 8;
        redGroup.childAlignment = TextAnchor.MiddleLeft;
        redGroup.childControlWidth = false;
        redGroup.childControlHeight = false;
        redGroup.childForceExpandWidth = false;
        redGroup.childForceExpandHeight = false;

        redTeamIcon = CreateColorBlock("RedTeamIcon", redSection.GetComponent<RectTransform>(), 30, 30, RedTeamColor);
        redScoreText = CreateText("RedScore", redSection.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(60, 30), 24, Color.white, TextAnchor.MiddleLeft);
        redScoreText.text = "0";

        // 中 - 回合
        GameObject centerSection = CreateUIObject("CenterSection", topBarRect);
        LayoutElement centerLayout = centerSection.AddComponent<LayoutElement>();
        centerLayout.preferredHeight = 30;
        centerLayout.minWidth = 150;

        roundText = CreateText("RoundText", centerSection.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(0, 0), 20, Color.white, TextAnchor.MiddleCenter);
        roundText.text = "ROUND 1/7";

        // 右 - 蓝队
        GameObject blueSection = CreateUIObject("BlueSection", topBarRect);
        LayoutElement blueLayout = blueSection.AddComponent<LayoutElement>();
        blueLayout.preferredHeight = 30;
        blueLayout.minWidth = 120;

        HorizontalLayoutGroup blueGroup = blueSection.AddComponent<HorizontalLayoutGroup>();
        blueGroup.spacing = 8;
        blueGroup.childAlignment = TextAnchor.MiddleRight;
        blueGroup.childControlWidth = false;
        blueGroup.childControlHeight = false;
        blueGroup.childForceExpandWidth = false;
        blueGroup.childForceExpandHeight = false;

        blueScoreText = CreateText("BlueScore", blueSection.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(60, 30), 24, Color.white, TextAnchor.MiddleRight);
        blueScoreText.text = "0";

        blueTeamIcon = CreateColorBlock("BlueTeamIcon", blueSection.GetComponent<RectTransform>(), 30, 30, BlueTeamColor);
    }

    // --- 左下角 - 生命值 ---

    private void BuildHealthSection()
    {
        // 根容器
        GameObject healthRoot = CreateUIObject("HealthSection", canvasRect);
        RectTransform healthRootRect = healthRoot.GetComponent<RectTransform>();
        healthRootRect.anchorMin = new Vector2(0, 0);
        healthRootRect.anchorMax = new Vector2(0, 0);
        healthRootRect.pivot = new Vector2(0, 0);
        healthRootRect.anchoredPosition = new Vector2(20, 20);
        healthRootRect.sizeDelta = new Vector2(250, 80);

        // 生命条背景
        healthBarBackground = CreateColorBlock("HealthBarBg", healthRootRect,
            200, 20, DarkGray);
        RectTransform bgRect = healthBarBackground.rectTransform;
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(0, -5);
        bgRect.sizeDelta = new Vector2(200, 20);

        // 生命条填充
        GameObject fillObj = CreateUIObject("HealthBarFill", healthRootRect);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 1);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 1);
        fillRect.anchoredPosition = new Vector2(0, -5);
        fillRect.sizeDelta = new Vector2(200, 20);

        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = HealthGreen;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillAmount = 1f;

        // 生命值文字
        healthText = CreateText("HealthText", healthRootRect,
            new Vector2(0, -5), new Vector2(200, 20), 14, Color.white, TextAnchor.MiddleCenter);
        healthText.text = "100/100";

        // 急救包图标 x3
        medkitIcons = new Image[MEDKIT_COUNT];
        medkitCooldownOverlays = new Image[MEDKIT_COUNT];

        for (int i = 0; i < MEDKIT_COUNT; i++)
        {
            float xOffset = i * 35;

            // 底层图标
            medkitIcons[i] = CreateColorBlock("Medkit_" + i, healthRootRect,
                28, 28, new Color(0.1f, 0.7f, 0.2f, 0.9f));
            RectTransform medkitRect = medkitIcons[i].rectTransform;
            medkitRect.anchorMin = new Vector2(0, 0);
            medkitRect.anchorMax = new Vector2(0, 0);
            medkitRect.pivot = new Vector2(0, 0);
            medkitRect.anchoredPosition = new Vector2(xOffset, 0);
            medkitRect.sizeDelta = new Vector2(28, 28);

            // 图标文字标签
            Text medkitLabel = CreateText("MedkitLabel_" + i, healthRootRect,
                new Vector2(xOffset, 0), new Vector2(28, 28), 10, Color.white, TextAnchor.MiddleCenter);
            medkitLabel.text = (i + 1).ToString();

            // 冷却遮罩
            GameObject cooldownObj = CreateUIObject("MedkitCD_" + i, healthRootRect);
            RectTransform cdRect = cooldownObj.GetComponent<RectTransform>();
            cdRect.anchorMin = new Vector2(0, 0);
            cdRect.anchorMax = new Vector2(0, 0);
            cdRect.pivot = new Vector2(0, 0);
            cdRect.anchoredPosition = new Vector2(xOffset, 0);
            cdRect.sizeDelta = new Vector2(28, 28);

            medkitCooldownOverlays[i] = cooldownObj.AddComponent<Image>();
            medkitCooldownOverlays[i].color = new Color(0f, 0f, 0f, 0.6f);
            medkitCooldownOverlays[i].type = Image.Type.Filled;
            medkitCooldownOverlays[i].fillMethod = Image.FillMethod.Vertical;
            medkitCooldownOverlays[i].fillAmount = 0f;
        }
    }

    // --- 右下角 - 武器/弹药 ---

    private void BuildWeaponSection()
    {
        // 根容器
        GameObject weaponRoot = CreateUIObject("WeaponSection", canvasRect);
        RectTransform weaponRootRect = weaponRoot.GetComponent<RectTransform>();
        weaponRootRect.anchorMin = new Vector2(1, 0);
        weaponRootRect.anchorMax = new Vector2(1, 0);
        weaponRootRect.pivot = new Vector2(1, 0);
        weaponRootRect.anchoredPosition = new Vector2(-20, 20);
        weaponRootRect.sizeDelta = new Vector2(200, 120);

        // 武器名称
        weaponNameText = CreateText("WeaponName", weaponRootRect,
            new Vector2(0, -5), new Vector2(200, 25), 16, new Color(0.85f, 0.85f, 0.85f, 1f), TextAnchor.UpperLeft);
        weaponNameText.text = "突击步枪";
        weaponNameText.rectTransform.anchorMin = new Vector2(0, 1);
        weaponNameText.rectTransform.anchorMax = new Vector2(1, 1);
        weaponNameText.rectTransform.pivot = new Vector2(0, 1);

        // 当前弹药 (大字)
        currentAmmoText = CreateText("CurrentAmmo", weaponRootRect,
            new Vector2(0, -30), new Vector2(100, 40), 36, Color.white, TextAnchor.MiddleLeft);
        currentAmmoText.rectTransform.anchorMin = new Vector2(0, 1);
        currentAmmoText.rectTransform.pivot = new Vector2(0, 1);
        currentAmmoText.text = "30";

        // 备用弹药 (小字)
        reserveAmmoText = CreateText("ReserveAmmo", weaponRootRect,
            new Vector2(100, -30), new Vector2(80, 40), 18, new Color(0.7f, 0.7f, 0.7f, 1f), TextAnchor.MiddleLeft);
        reserveAmmoText.rectTransform.anchorMin = new Vector2(0, 1);
        reserveAmmoText.rectTransform.pivot = new Vector2(0, 1);
        reserveAmmoText.text = "/ 120";

        // 换弹指示
        reloadingText = CreateText("ReloadingText", weaponRootRect,
            new Vector2(0, -65), new Vector2(200, 20), 14, new Color(1f, 0.8f, 0.2f, 1f), TextAnchor.MiddleLeft);
        reloadingText.rectTransform.anchorMin = new Vector2(0, 1);
        reloadingText.rectTransform.pivot = new Vector2(0, 1);
        reloadingText.text = "";
        reloadingText.fontStyle = FontStyle.Bold;

        // 武器槽位 1-4
        weaponSlotBoxes = new Image[WEAPON_SLOT_COUNT];
        weaponSlotLabels = new Text[WEAPON_SLOT_COUNT];

        for (int i = 0; i < WEAPON_SLOT_COUNT; i++)
        {
            float xOffset = i * 45;

            weaponSlotBoxes[i] = CreateColorBlock("WeaponSlot_" + i, weaponRootRect,
                40, 30, InactiveSlotColor);
            RectTransform slotRect = weaponSlotBoxes[i].rectTransform;
            slotRect.anchorMin = new Vector2(0, 0);
            slotRect.anchorMax = new Vector2(0, 0);
            slotRect.pivot = new Vector2(0, 0);
            slotRect.anchoredPosition = new Vector2(xOffset, 0);
            slotRect.sizeDelta = new Vector2(40, 30);

            weaponSlotLabels[i] = CreateText("SlotLabel_" + i, weaponRootRect,
                new Vector2(xOffset, 0), new Vector2(40, 30), 14, Color.white, TextAnchor.MiddleCenter);
            weaponSlotLabels[i].rectTransform.anchorMin = new Vector2(0, 0);
            weaponSlotLabels[i].rectTransform.anchorMax = new Vector2(0, 0);
            weaponSlotLabels[i].rectTransform.pivot = new Vector2(0, 0);
            weaponSlotLabels[i].text = (i + 1).ToString();
        }

        // 高亮第一个槽位
        HighlightWeaponSlot(0);

        // 投掷物指示
        GameObject grenadeObj = CreateUIObject("GrenadeIndicator", weaponRootRect);
        RectTransform grenadeRect = grenadeObj.GetComponent<RectTransform>();
        grenadeRect.anchorMin = new Vector2(1, 0);
        grenadeRect.anchorMax = new Vector2(1, 0);
        grenadeRect.pivot = new Vector2(1, 0);
        grenadeRect.anchoredPosition = new Vector2(0, 0);
        grenadeRect.sizeDelta = new Vector2(40, 30);

        Image grenadeBg = grenadeObj.AddComponent<Image>();
        grenadeBg.color = new Color(0.5f, 0.3f, 0.1f, 0.8f);

        grenadeTypeText = CreateText("GrenadeType", weaponRootRect,
            new Vector2(-40, 0), new Vector2(40, 30), 12, Color.white, TextAnchor.MiddleCenter);
        grenadeTypeText.rectTransform.anchorMin = new Vector2(1, 0);
        grenadeTypeText.rectTransform.anchorMax = new Vector2(1, 0);
        grenadeTypeText.rectTransform.pivot = new Vector2(1, 0);
        grenadeTypeText.text = "F";

        // 投掷物冷却遮罩
        GameObject grenadeCDBg = CreateUIObject("GrenadeCD", weaponRootRect);
        RectTransform gcdRect = grenadeCDBg.GetComponent<RectTransform>();
        gcdRect.anchorMin = new Vector2(1, 0);
        gcdRect.anchorMax = new Vector2(1, 0);
        gcdRect.pivot = new Vector2(1, 0);
        gcdRect.anchoredPosition = new Vector2(-40, 0);
        gcdRect.sizeDelta = new Vector2(40, 30);

        grenadeCooldownOverlay = grenadeCDBg.AddComponent<Image>();
        grenadeCooldownOverlay.color = new Color(0f, 0f, 0f, 0.5f);
        grenadeCooldownOverlay.type = Image.Type.Filled;
        grenadeCooldownOverlay.fillMethod = Image.FillMethod.Vertical;
        grenadeCooldownOverlay.fillAmount = 0f;
    }

    // --- 右上角 - 击杀播报 ---

    private void BuildKillFeed()
    {
        GameObject killFeedRoot = CreateUIObject("KillFeed", canvasRect);
        RectTransform kfRect = killFeedRoot.GetComponent<RectTransform>();
        kfRect.anchorMin = new Vector2(1, 1);
        kfRect.anchorMax = new Vector2(1, 1);
        kfRect.pivot = new Vector2(1, 1);
        kfRect.anchoredPosition = new Vector2(-10, -60);
        kfRect.sizeDelta = new Vector2(280, 160);

        // 添加 KillFeedController 组件
        killFeedController = killFeedRoot.AddComponent<KillFeedController>();
        killFeedController.Initialize(kfRect);
    }

    // --- 中心 - 准心 ---

    private void BuildCrosshair()
    {
        GameObject crosshairRoot = CreateUIObject("Crosshair", canvasRect);
        RectTransform crossRect = crosshairRoot.GetComponent<RectTransform>();
        crossRect.anchorMin = new Vector2(0.5f, 0.5f);
        crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        crossRect.pivot = new Vector2(0.5f, 0.5f);
        crossRect.anchoredPosition = Vector2.zero;
        crossRect.sizeDelta = new Vector2(60, 60);

        // 创建4条准心线
        Image[] lines = new Image[4];
        string[] lineNames = { "Top", "Bottom", "Left", "Right" };

        for (int i = 0; i < 4; i++)
        {
            GameObject lineObj = CreateUIObject("Crosshair_" + lineNames[i], crossRect);
            Image lineImg = lineObj.AddComponent<Image>();
            lineImg.color = Color.white;
            lines[i] = lineImg;
        }

        // 黑色描边 (每个方向各一条黑色线在白色线下方)
        for (int i = 0; i < 4; i++)
        {
            GameObject outlineObj = CreateUIObject("Crosshair_Outline_" + lineNames[i], crossRect);
            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.color = Color.black;

            RectTransform outlineRect = outlineObj.GetComponent<RectTransform>();
            RectTransform lineRect = lines[i].rectTransform;

            outlineRect.anchorMin = lineRect.anchorMin;
            outlineRect.anchorMax = lineRect.anchorMax;
            outlineRect.pivot = lineRect.pivot;
            outlineRect.anchoredPosition = lineRect.anchoredPosition;
            outlineRect.sizeDelta = lineRect.sizeDelta + new Vector2(2, 2);

            outlineObj.transform.SetAsFirstSibling();
        }

        // 添加 CrosshairController
        crosshairController = crosshairRoot.AddComponent<CrosshairController>();
        crosshairController.Initialize(lines);
    }

    // --- 中心 - 回合公告覆盖 ---

    private void BuildRoundOverlay()
    {
        GameObject overlayRoot = CreateUIObject("RoundOverlay", canvasRect);
        RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = new Vector2(0, 50);
        overlayRect.sizeDelta = new Vector2(600, 100);

        CanvasGroup overlayGroup = overlayRoot.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;

        Text announcementText = CreateText("AnnouncementText", overlayRect,
            Vector2.zero, new Vector2(600, 100), 42, Color.white, TextAnchor.MiddleCenter);
        announcementText.fontStyle = FontStyle.Bold;

        // 文字描边效果
        Outline outline = announcementText.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        Shadow shadow = announcementText.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(3, -3);

        roundOverlayController = overlayRoot.AddComponent<RoundOverlayController>();
        roundOverlayController.Initialize(overlayGroup, announcementText);
    }

    // --- 左侧 - 小地图 ---

    private void BuildMinimap()
    {
        GameObject minimapRoot = CreateUIObject("Minimap", canvasRect);
        RectTransform mmRect = minimapRoot.GetComponent<RectTransform>();
        mmRect.anchorMin = new Vector2(0, 0.5f);
        mmRect.anchorMax = new Vector2(0, 0.5f);
        mmRect.pivot = new Vector2(0, 0.5f);
        mmRect.anchoredPosition = new Vector2(15, 0);
        mmRect.sizeDelta = new Vector2(MINIMAP_SIZE, MINIMAP_SIZE);

        // 背景
        Image minimapBg = minimapRoot.AddComponent<Image>();
        minimapBg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        // 圆形遮罩
        GameObject maskObj = CreateUIObject("MinimapMask", mmRect);
        RectTransform maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.pivot = new Vector2(0.5f, 0.5f);
        maskRect.sizeDelta = Vector2.zero;

        Mask circleMask = maskObj.AddComponent<Mask>();
        Image maskImage = maskObj.GetComponent<Image>();
        if (maskImage == null) maskImage = maskObj.AddComponent<Image>();

        // 创建圆形遮罩纹理
        Texture2D circleTex = new Texture2D(MINIMAP_SIZE, MINIMAP_SIZE);
        int center = MINIMAP_SIZE / 2;
        int radius = MINIMAP_SIZE / 2 - 2;
        for (int y = 0; y < MINIMAP_SIZE; y++)
        {
            for (int x = 0; x < MINIMAP_SIZE; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                circleTex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        circleTex.Apply();
        Sprite circleSprite = Sprite.Create(circleTex, new Rect(0, 0, MINIMAP_SIZE, MINIMAP_SIZE), new Vector2(0.5f, 0.5f));
        maskImage.sprite = circleSprite;
        maskImage.color = Color.white;

        // 地图图像
        GameObject mapDisplayObj = CreateUIObject("MinimapDisplay", maskRect);
        RectTransform mapDisplayRect = mapDisplayObj.GetComponent<RectTransform>();
        mapDisplayRect.anchorMin = Vector2.zero;
        mapDisplayRect.anchorMax = Vector2.one;
        mapDisplayRect.pivot = new Vector2(0.5f, 0.5f);
        mapDisplayRect.sizeDelta = Vector2.zero;

        minimapImage = mapDisplayObj.AddComponent<Image>();

        // 创建小地图控制器
        minimapController = minimapRoot.AddComponent<MinimapController>();
        minimapController.Initialize(minimapImage, MINIMAP_SIZE, MAP_WORLD_SIZE);
    }

    // =========================================================================
    //  辅助创建方法
    // =========================================================================

    /// <summary>
    /// 创建通用UI对象
    /// </summary>
    private GameObject CreateUIObject(string name, RectTransform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return obj;
    }

    /// <summary>
    /// 创建文字UI元素
    /// </summary>
    private Text CreateText(string name, RectTransform parent, Vector2 position, Vector2 size,
        int fontSize, Color color, TextAnchor alignment)
    {
        GameObject obj = CreateUIObject(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    /// <summary>
    /// 创建纯色方块
    /// </summary>
    private Image CreateColorBlock(string name, RectTransform parent, float width, float height, Color color)
    {
        GameObject obj = CreateUIObject(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        return img;
    }

    // =========================================================================
    //  初始化 - 查找游戏组件
    // =========================================================================

    private void FindPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            weaponSystem = player.GetComponent<WeaponSystem>();
            grenadeSystem = player.GetComponent<GrenadeSystem>();
            medkitSystem = player.GetComponent<MedkitSystem>();
        }

        roundManager = FindObjectOfType<RoundManager>();
    }

    private void SubscribeToRoundEvents()
    {
        if (roundManager != null)
        {
            roundManager.onScoreUpdate += UpdateScore;
            roundManager.onRoundStart += OnRoundStart;
            roundManager.onGameOver += OnGameOver;
        }
    }

    // =========================================================================
    //  每帧更新
    // =========================================================================

    private void UpdatePlayerStatus()
    {
        UpdateHealthFromSystem();
        UpdateAmmoFromSystem();
        UpdateGrenadeFromSystem();
        UpdateMedkitFromSystem();
    }

    private void UpdateHealthFromSystem()
    {
        if (playerHealth == null) return;

        int current = playerHealth.GetCurrentHealth();
        int max = playerHealth.maxHealth;
        UpdateHealthBar(current, max);
    }

    private void UpdateAmmoFromSystem()
    {
        if (weaponSystem == null) return;

        UpdateAmmo(
            weaponSystem.GetCurrentAmmo(),
            weaponSystem.GetReserveAmmo(),
            weaponSystem.IsReloading()
        );

        UpdateWeapon(
            weaponSystem.GetCurrentWeaponName(),
            System.Array.IndexOf(
                new string[] { "突击步枪", "狙击枪", "冲锋枪", "散弹枪" },
                weaponSystem.GetCurrentWeaponName()
            )
        );
    }

    private void UpdateGrenadeFromSystem()
    {
        if (grenadeSystem == null) return;

        GrenadeType type = grenadeSystem.GetCurrentGrenadeType();
        string typeName = "";
        switch (type)
        {
            case GrenadeType.Smoke: typeName = "烟雾"; break;
            case GrenadeType.Flash: typeName = "闪光"; break;
            case GrenadeType.Frag: typeName = "手雷"; break;
        }

        if (grenadeTypeText != null)
        {
            grenadeTypeText.text = typeName;
        }
    }

    private void UpdateMedkitFromSystem()
    {
        if (medkitSystem == null) return;

        float[] cooldowns = new float[MEDKIT_COUNT];
        for (int i = 0; i < MEDKIT_COUNT && i < medkitSystem.medkitTypes.Length; i++)
        {
            cooldowns[i] = 0f;
        }
        UpdateMedkitCooldowns(cooldowns);
    }

    // =========================================================================
    //  公开更新方法
    // =========================================================================

    /// <summary>
    /// 更新生命值条 - 动画填充、变色
    /// </summary>
    public void UpdateHealthBar(int current, int max)
    {
        if (healthBarFill == null) return;

        float percent = Mathf.Clamp01((float)current / max);
        healthBarFill.fillAmount = percent;

        // 根据百分比变色
        if (percent < 0.3f)
            healthBarFill.color = HealthRed;
        else if (percent < 0.6f)
            healthBarFill.color = HealthYellow;
        else
            healthBarFill.color = HealthGreen;

        if (healthText != null)
        {
            healthText.text = current + "/" + max;
        }
    }

    /// <summary>
    /// 更新弹药显示
    /// </summary>
    public void UpdateAmmo(int current, int reserve, bool reloading)
    {
        if (currentAmmoText != null)
        {
            currentAmmoText.text = current.ToString();

            // 低弹药时变红
            currentAmmoText.color = current <= 5 ? HealthRed : Color.white;
        }

        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = "/ " + reserve;
        }

        if (reloadingText != null)
        {
            reloadingText.text = reloading ? "换弹中..." : "";
        }
    }

    /// <summary>
    /// 更新武器名称和高亮槽位
    /// </summary>
    public void UpdateWeapon(string name, int index)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = name;
        }

        if (index >= 0 && index < WEAPON_SLOT_COUNT)
        {
            HighlightWeaponSlot(index);
        }
    }

    /// <summary>
    /// 高亮指定武器槽位
    /// </summary>
    private void HighlightWeaponSlot(int activeIndex)
    {
        for (int i = 0; i < WEAPON_SLOT_COUNT; i++)
        {
            if (weaponSlotBoxes[i] != null)
            {
                weaponSlotBoxes[i].color = (i == activeIndex) ? ActiveSlotColor : InactiveSlotColor;
            }
            if (weaponSlotLabels[i] != null)
            {
                weaponSlotLabels[i].color = (i == activeIndex) ? Color.black : Color.white;
            }
        }
    }

    /// <summary>
    /// 更新比分
    /// </summary>
    public void UpdateScore(int red, int blue)
    {
        if (redScoreText != null)
            redScoreText.text = red.ToString();

        if (blueScoreText != null)
            blueScoreText.text = blue.ToString();
    }

    /// <summary>
    /// 更新回合显示
    /// </summary>
    public void UpdateRound(int round, int max)
    {
        if (roundText != null)
            roundText.text = "ROUND " + round + "/" + max;
    }

    /// <summary>
    /// 添加击杀播报条目
    /// </summary>
    public void AddKillFeedEntry(string killer, string victim, string weapon, string killerTeam)
    {
        if (killFeedController != null)
        {
            killFeedController.AddEntry(killer, victim, weapon, killerTeam);
        }
    }

    /// <summary>
    /// 显示回合公告
    /// </summary>
    public void ShowRoundAnnouncement(string text, float duration)
    {
        if (roundOverlayController != null)
        {
            roundOverlayController.Show(text, duration);
        }
    }

    /// <summary>
    /// 更新小地图
    /// </summary>
    public void UpdateMinimap(Vector2[] friendlyPositions, Vector2[] enemyPositions, Vector2 playerPos)
    {
        if (minimapController != null)
        {
            minimapController.UpdateMap(friendlyPositions, enemyPositions, playerPos);
        }
    }

    /// <summary>
    /// 设置准心扩散度
    /// </summary>
    public void SetCrosshairSpread(float spread)
    {
        if (crosshairController != null)
        {
            crosshairController.SetSpread(spread);
        }
    }

    /// <summary>
    /// 更新急救包冷却
    /// </summary>
    public void UpdateMedkitCooldowns(float[] cooldowns)
    {
        for (int i = 0; i < MEDKIT_COUNT && i < cooldowns.Length; i++)
        {
            if (medkitCooldownOverlays[i] != null)
            {
                float fill = Mathf.Clamp01(cooldowns[i]);
                medkitCooldownOverlays[i].fillAmount = fill;

                // 冷却中暗化图标
                if (medkitIcons[i] != null)
                {
                    Color iconColor = medkitIcons[i].color;
                    iconColor.a = fill > 0f ? 0.4f : 0.9f;
                    medkitIcons[i].color = iconColor;
                }
            }
        }
    }

    // =========================================================================
    //  回合事件回调
    // =========================================================================

    private void OnRoundStart(int round, bool suddenDeath)
    {
        if (suddenDeath)
        {
            UpdateRound(round, 7);
            ShowRoundAnnouncement("SUDDEN DEATH", 3f);
        }
        else
        {
            UpdateRound(round, 7);
            ShowRoundAnnouncement("ROUND " + round + " - FIGHT!", 3f);
        }
    }

    private void OnGameOver(string winner, int[] scores)
    {
        string winnerText = (winner == "red") ? "RED TEAM WINS" : "BLUE TEAM WINS";
        ShowRoundAnnouncement(winnerText, 5f);
    }

    // =========================================================================
    //  生命周期
    // =========================================================================

    private void OnDestroy()
    {
        if (roundManager != null)
        {
            roundManager.onScoreUpdate -= UpdateScore;
            roundManager.onRoundStart -= OnRoundStart;
            roundManager.onGameOver -= OnGameOver;
        }
    }
}