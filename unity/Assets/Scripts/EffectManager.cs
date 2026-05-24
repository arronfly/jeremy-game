using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 特效管理器 - 管理所有视觉效果的对象池
/// 通过 SpriteManager 获取精灵资源，自动创建和管理特效实例
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    /// <summary>
    /// 特效对象池: 按特效名称分组，每组维护一个可复用对象队列
    /// </summary>
    private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();

    /// <summary>
    /// 特效预制模板: 按特效名称分组，存储每个类型的模板 GameObject
    /// </summary>
    private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();

    /// <summary>
    /// 每种特效类型的预创建实例数量
    /// </summary>
    private const int POOL_INITIAL_SIZE = 5;

    /// <summary>
    /// 特效排序层 (Effect = layer 4)
    /// </summary>
    private const int EFFECT_SORTING_ORDER = 4;

    /// <summary>
    /// 每种特效的默认生命周期配置
    /// </summary>
    private static readonly Dictionary<string, float> defaultLifetimes = new Dictionary<string, float>
    {
        { "muzzle_flash", 0.05f },
        { "bullet_trail", 0.1f },
        { "blood_splash", 2f },
        { "explosion", 0.5f },
        { "smoke", 5f },
        { "flash_screen", 0.3f },
        { "spawn_ring", 0.5f }
    };

    /// <summary>
    /// 当前正在运行的自动回收协程引用，用于 ClearAll 时停止
    /// </summary>
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ──────────────────────────────────────────────
    // Initialization (called by GameBootstrapper)
    // ──────────────────────────────────────────────

    /// <summary>
    /// 初始化特效系统 - 创建所有特效模板并预填充对象池
    /// 由 GameBootstrapper 在游戏启动时调用
    /// </summary>
    public void Initialize()
    {
        CreateEffectTemplates();
        PreFillPools();
        Debug.Log("EffectManager: Initialized with " + effectPrefabs.Count + " effect types.");
    }

    /// <summary>
    /// 为所有特效类型创建模板 GameObject
    /// 模板是禁用状态的 GameObject，作为 Instantiate 的源
    /// </summary>
    private void CreateEffectTemplates()
    {
        CreateSimpleTemplate("muzzle_flash", "muzzle_flash_0");
        CreateSimpleTemplate("bullet_trail", "bullet_trail");
        CreateBloodTemplate();
        CreateExplosionTemplate();
        CreateSimpleTemplate("smoke", "smoke", 3f);
        CreateSimpleTemplate("flash_screen", "flash_screen");
        CreateSimpleTemplate("spawn_ring", "spawn_ring");
    }

    /// <summary>
    /// 创建简单特效模板 (单个精灵，无动画)
    /// </summary>
    private void CreateSimpleTemplate(string effectName, string spriteName, float scale = 1f)
    {
        Sprite sprite = SpriteManager.Instance.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogWarning("EffectManager: Sprite '" + spriteName + "' not found for effect '" + effectName + "'");
            return;
        }

        GameObject template = new GameObject("Template_" + effectName);
        template.transform.SetParent(transform);

        SpriteRenderer sr = template.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = EFFECT_SORTING_ORDER;

        if (effectName == "flash_screen")
        {
            // flash_screen 特殊处理: 作为 UI 覆盖层
            sr.sortingOrder = 100;
        }

        template.transform.localScale = Vector3.one * scale;
        template.SetActive(false);

        effectPrefabs[effectName] = template;
    }

    /// <summary>
    /// 创建 blood_splash 模板 (运行时随机选择 blood_0 ~ blood_3)
    /// </summary>
    private void CreateBloodTemplate()
    {
        // 使用 blood_0 作为基础模板，运行时在 SpawnEffect 中随机替换
        Sprite sprite = SpriteManager.Instance.GetSprite("blood_0");
        if (sprite == null)
        {
            Debug.LogWarning("EffectManager: Blood sprite not found.");
            return;
        }

        GameObject template = new GameObject("Template_blood_splash");
        template.transform.SetParent(transform);

        SpriteRenderer sr = template.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = EFFECT_SORTING_ORDER;

        template.SetActive(false);

        effectPrefabs["blood_splash"] = template;
    }

    /// <summary>
    /// 创建 explosion 模板 (需要多帧动画支持)
    /// 存储 explosion_0 到 explosion_4 精灵列表
    /// </summary>
    private void CreateExplosionTemplate()
    {
        Sprite sprite = SpriteManager.Instance.GetSprite("explosion_0");
        if (sprite == null)
        {
            Debug.LogWarning("EffectManager: Explosion sprite not found.");
            return;
        }

        GameObject template = new GameObject("Template_explosion");
        template.transform.SetParent(transform);

        SpriteRenderer sr = template.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = EFFECT_SORTING_ORDER;

        template.SetActive(false);

        // 附加爆炸帧数据组件
        ExplosionFrames frames = template.AddComponent<ExplosionFrames>();
        frames.frameSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            frames.frameSprites[i] = SpriteManager.Instance.GetSprite("explosion_" + i);
        }

        effectPrefabs["explosion"] = template;
    }

    /// <summary>
    /// 预填充对象池: 为每种特效类型创建初始实例
    /// </summary>
    private void PreFillPools()
    {
        foreach (var kvp in effectPrefabs)
        {
            string name = kvp.Key;
            GameObject template = kvp.Value;

            effectPools[name] = new Queue<GameObject>();

            for (int i = 0; i < POOL_INITIAL_SIZE; i++)
            {
                GameObject instance = CreateInstanceFromTemplate(name, template);
                effectPools[name].Enqueue(instance);
            }
        }
    }

    /// <summary>
    /// 从模板创建一个新实例
    /// </summary>
    private GameObject CreateInstanceFromTemplate(string effectName, GameObject template)
    {
        GameObject obj = Instantiate(template, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(transform);
        obj.name = "Effect_" + effectName;
        obj.SetActive(false);
        return obj;
    }

    // ──────────────────────────────────────────────
    // Spawn / Return
    // ──────────────────────────────────────────────

    /// <summary>
    /// 在指定位置生成特效
    /// 从对象池获取或创建新实例，设置位置并启动自动回收
    /// </summary>
    /// <param name="name">特效名称</param>
    /// <param name="position">世界坐标位置</param>
    /// <param name="lifetime">持续时间(秒)，0 使用默认值</param>
    /// <returns>生成的特效 GameObject 引用</returns>
    public GameObject SpawnEffect(string name, Vector2 position, float lifetime = 0f)
    {
        if (!effectPrefabs.ContainsKey(name))
        {
            Debug.LogWarning("EffectManager: Unknown effect type '" + name + "'");
            return null;
        }

        // 如果未指定生命周期，使用默认值
        if (lifetime <= 0f && defaultLifetimes.ContainsKey(name))
        {
            lifetime = defaultLifetimes[name];
        }
        else if (lifetime <= 0f)
        {
            lifetime = 0.5f;
        }

        GameObject effect;

        // 尝试从池中取出
        if (effectPools.ContainsKey(name) && effectPools[name].Count > 0)
        {
            effect = effectPools[name].Dequeue();
        }
        else
        {
            // 池为空，创建新实例
            effect = CreateInstanceFromTemplate(name, effectPrefabs[name]);
        }

        // blood_splash: 随机选择血迹精灵
        if (name == "blood_splash")
        {
            SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int variant = Random.Range(0, 4);
                sr.sprite = SpriteManager.Instance.GetSprite("blood_" + variant);
            }
        }

        // explosion: 启动帧动画
        if (name == "explosion")
        {
            ExplosionFrames frames = effect.GetComponent<ExplosionFrames>();
            if (frames != null)
            {
                StartCoroutine(EffectAnimation(effect, frames.frameSprites, lifetime));
            }
        }

        // 设置位置并激活
        effect.transform.position = new Vector3(position.x, position.y, 0f);
        effect.SetActive(true);

        // 启动自动回收协程
        Coroutine co = StartCoroutine(AutoReturnEffect(name, effect, lifetime));
        activeCoroutines.Add(co);

        return effect;
    }

    /// <summary>
    /// 将特效对象回收到对象池
    /// </summary>
    /// <param name="name">特效类型名称</param>
    /// <param name="effect">要回收的 GameObject</param>
    public void ReturnEffect(string name, GameObject effect)
    {
        if (effect == null) return;

        effect.SetActive(false);

        if (!effectPools.ContainsKey(name))
        {
            effectPools[name] = new Queue<GameObject>();
        }

        effectPools[name].Enqueue(effect);
    }

    /// <summary>
    /// 清理所有活跃特效，全部回收到对象池
    /// </summary>
    public void ClearAll()
    {
        // 停止所有运行中的自动回收协程
        foreach (Coroutine co in activeCoroutines)
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
        }
        activeCoroutines.Clear();

        // 遍历所有子对象，将活跃的特效回收到池中
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);

                // 从对象名称解析特效类型
                string objName = child.gameObject.name;
                string effectType = ParseEffectType(objName);

                if (!string.IsNullOrEmpty(effectType) && effectPools.ContainsKey(effectType))
                {
                    effectPools[effectType].Enqueue(child.gameObject);
                }
            }
        }
    }

    // ──────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────

    /// <summary>
    /// 自动回收协程: 等待 lifetime 秒后回收特效到对象池
    /// </summary>
    private IEnumerator AutoReturnEffect(string name, GameObject effect, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        // 从活跃协程列表中移除自身引用
        activeCoroutines.RemoveAll(c => c == null);

        ReturnEffect(name, effect);
    }

    /// <summary>
    /// 特效帧动画协程: 在生命周期内循环播放精灵帧序列
    /// 用于 explosion 特效 (explosion_0 到 explosion_4)
    /// </summary>
    private IEnumerator EffectAnimation(GameObject effect, Sprite[] frames, float totalDuration)
    {
        if (frames == null || frames.Length == 0) yield break;

        SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float frameDuration = totalDuration / frames.Length;
        int frameCount = frames.Length;

        for (int i = 0; i < frameCount; i++)
        {
            if (effect == null || !effect.activeSelf) yield break;

            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameDuration);
        }
    }

    // ──────────────────────────────────────────────
    // Utility
    // ──────────────────────────────────────────────

    /// <summary>
    /// 从对象名称解析特效类型
    /// 例如 "Effect_muzzle_flash" -> "muzzle_flash"
    /// </summary>
    private string ParseEffectType(string objName)
    {
        // 模板对象以 "Template_" 开头，跳过
        if (objName.StartsWith("Template_")) return null;

        // 实例对象以 "Effect_" 开头
        if (objName.StartsWith("Effect_"))
        {
            return objName.Substring(7); // "Effect_".Length = 7
        }

        // 克隆对象名称带 "(Clone)" 后缀
        if (objName.Contains("(Clone)"))
        {
            string cleaned = objName.Replace("(Clone)", "");
            if (cleaned.StartsWith("Effect_"))
            {
                return cleaned.Substring(7);
            }
        }

        return null;
    }

    /// <summary>
    /// 获取指定特效类型的对象池可用数量
    /// </summary>
    public int GetPoolCount(string name)
    {
        if (effectPools.ContainsKey(name))
        {
            return effectPools[name].Count;
        }
        return 0;
    }

    /// <summary>
    /// 获取已注册的特效类型数量
    /// </summary>
    public int GetRegisteredEffectCount()
    {
        return effectPrefabs.Count;
    }
}

/// <summary>
/// 爆炸帧数据组件 - 存储爆炸动画的精灵帧序列
/// 附加在 explosion 模板上，供 EffectAnimation 协程读取
/// </summary>
public class ExplosionFrames : MonoBehaviour
{
    public Sprite[] frameSprites;
}
