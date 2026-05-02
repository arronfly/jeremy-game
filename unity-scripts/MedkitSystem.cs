using UnityEngine;
using System.Collections;

/// <summary>
/// 药品类型
/// </summary>
public enum MedkitType
{
    FirstAidKit,   // 急救包
    Bandage,       // 绷带
    EnergyDrink    // 能量饮料
}

/// <summary>
/// 药品配置
/// </summary>
[System.Serializable]
public class MedkitData
{
    public MedkitType type;
    public float cooldown = 5f;
    public float useTime = 1f;       // 使用时间
    public int healAmount = 10;      // 治疗量
    public float speedBoost = 0f;     // 速度加成
    public float boostDuration = 0f; // 加成持续时间
}

/// <summary>
/// 药品系统
/// </summary>
public class MedkitSystem : MonoBehaviour
{
    [Header("药品配置")]
    public MedkitData[] medkitTypes = new MedkitData[]
    {
        new MedkitData() { type = MedkitType.FirstAidKit, cooldown = 10f, useTime = 3f, healAmount = 50 },
        new MedkitData() { type = MedkitType.Bandage, cooldown = 5f, useTime = 1f, healAmount = 10 },
        new MedkitData() { type = MedkitType.EnergyDrink, cooldown = 8f, useTime = 0.5f, healAmount = 25, speedBoost = 0.2f, boostDuration = 10f }
    };

    [Header("状态")]
    private bool[] canUse = new bool[] { true, true, true };
    private bool[] isUsing = new bool[] { false, false, false };
    private float[] cooldownEndTime = new float[] { 0, 0, 0 };
    private float[] useStartTime = new float[] { 0, 0, 0 };

    private PlayerHealth healthSystem;
    private float originalSpeed;

    void Start()
    {
        healthSystem = GetComponent<PlayerHealth>();
        originalSpeed = GetComponent<PlayerController>()?.moveSpeed ?? 5f;
    }

    void Update()
    {
        // 7键 - 急救包
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TryUseMedkit(0);
        }
        // 8键 - 绷带
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TryUseMedkit(1);
        }
        // 9键 - 能量饮料
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TryUseMedkit(2);
        }
    }

    void TryUseMedkit(int index)
    {
        if (index < 0 || index >= medkitTypes.Length) return;
        if (isUsing[index]) return;
        if (!canUse[index]) return;
        if (Time.time < cooldownEndTime[index])
        {
            Debug.Log(medkitTypes[index].type + " 冷却中...");
            return;
        }

        // 如果正在使用其他药品，取消
        for (int i = 0; i < isUsing.Length; i++)
        {
            if (isUsing[i]) CancelUse(i);
        }

        // 开始使用
        isUsing[index] = true;
        useStartTime[index] = Time.time;
        Debug.Log("使用 " + medkitTypes[index].type + "...");

        StartCoroutine(UseMedkit(index));
    }

    IEnumerator UseMedkit(int index)
    {
        MedkitData data = medkitTypes[index];
        yield return new WaitForSeconds(data.useTime);

        if (healthSystem != null)
        {
            healthSystem.Heal(data.healAmount);
        }

        // 如果是能量饮料，应用速度加成
        if (data.speedBoost > 0)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.moveSpeed = originalSpeed * (1 + data.speedBoost);
                yield return new WaitForSeconds(data.boostDuration);
                pc.moveSpeed = originalSpeed;
                Debug.Log("能量饮料效果结束");
            }
        }

        // 完成
        isUsing[index] = false;
        canUse[index] = false;
        cooldownEndTime[index] = Time.time + data.cooldown;

        Debug.Log(data.type + " 使用完成!");
    }

    void CancelUse(int index)
    {
        isUsing[index] = false;
        Debug.Log("取消使用 " + medkitTypes[index].type);
    }

    public bool IsUsingAny()
    {
        foreach (bool using_ in isUsing)
        {
            if (using_) return true;
        }
        return false;
    }

    public string GetUsingMedkitName()
    {
        for (int i = 0; i < isUsing.Length; i++)
        {
            if (isUsing[i]) return medkitTypes[i].type.ToString();
        }
        return null;
    }
}