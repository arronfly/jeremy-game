using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 输入管理器 - 集中处理所有玩家输入
/// 支持输入缓冲，用于改进射击和投掷手感
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("输入缓冲设置")]
    public float shootBufferTime = 0.1f;
    public float grenadeBufferTime = 0.2f;

    private struct InputBuffer
    {
        public float time;
        public bool pressed;
    }

    private Dictionary<KeyCode, InputBuffer> shootBuffer = new Dictionary<KeyCode, InputBuffer>();
    private Dictionary<KeyCode, InputBuffer> grenadeBuffer = new Dictionary<KeyCode, InputBuffer>();

    [Header("移动输入")]
    public Vector2 moveInput { get; private set; }
    public bool isSprinting { get; private set; }

    [Header("瞄准输入")]
    public Vector2 lookDirection { get; private set; }

    [Header("武器输入")]
    public KeyCode[] weaponKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode meleeKey = KeyCode.Space;

    [Header("投掷物输入")]
    public KeyCode grenadeKey = KeyCode.G;

    [Header("药品输入")]
    public KeyCode[] medkitKeys = new KeyCode[] { KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    void Update()
    {
        UpdateMoveInput();
        UpdateLookInput();
        UpdateShootBuffer();
        UpdateGrenadeBuffer();
    }

    void UpdateMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && moveInput.magnitude > 0;
    }

    void UpdateLookInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lookDirection = (mousePos - transform.position).normalized;
    }

    void UpdateShootBuffer()
    {
        // 射击缓冲 - 左键
        bool leftMouse = Input.GetMouseButtonDown(0);
        if (leftMouse)
        {
            shootBuffer[KeyCode.Mouse0] = new InputBuffer { time = Time.time, pressed = true };
        }

        // 清理过期缓冲
        List<KeyCode> keysToRemove = new List<KeyCode>();
        foreach (var kvp in shootBuffer)
        {
            if (Time.time - kvp.Value.time > shootBufferTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            shootBuffer.Remove(key);
        }
    }

    void UpdateGrenadeBuffer()
    {
        // 投掷物缓冲 - 右键
        bool rightMouse = Input.GetMouseButtonDown(1);
        if (rightMouse)
        {
            grenadeBuffer[KeyCode.Mouse1] = new InputBuffer { time = Time.time, pressed = true };
        }

        // 清理过期缓冲
        List<KeyCode> keysToRemove = new List<KeyCode>();
        foreach (var kvp in grenadeBuffer)
        {
            if (Time.time - kvp.Value.time > grenadeBufferTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            grenadeBuffer.Remove(key);
        }
    }

    /// <summary>
    /// 检测射击缓冲是否有效
    /// </summary>
    public bool IsShootBuffered()
    {
        return shootBuffer.ContainsKey(KeyCode.Mouse0);
    }

    /// <summary>
    /// 检测投掷物缓冲是否有效
    /// </summary>
    public bool IsGrenadeBuffered()
    {
        return grenadeBuffer.ContainsKey(KeyCode.Mouse1);
    }

    /// <summary>
    /// 获取当前按下的武器键 (1-4)
    /// </summary>
    public int GetWeaponSwitch()
    {
        for (int i = 0; i < weaponKeys.Length; i++)
        {
            if (Input.GetKeyDown(weaponKeys[i]))
            {
                return i + 1;
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取当前按下的药品键 (7-9)
    /// </summary>
    public int GetMedkitUse()
    {
        for (int i = 0; i < medkitKeys.Length; i++)
        {
            if (Input.GetKeyDown(medkitKeys[i]))
            {
                return i + 1;
            }
        }
        return 0;
    }

    /// <summary>
    /// 是否按下换弹键
    /// </summary>
    public bool IsReloadPressed()
    {
        return Input.GetKeyDown(reloadKey);
    }

    /// <summary>
    /// 是否按下近战键
    /// </summary>
    public bool IsMeleePressed()
    {
        return Input.GetKeyDown(meleeKey);
    }

    /// <summary>
    /// 是否按下投掷键
    /// </summary>
    public bool IsGrenadePressed()
    {
        return Input.GetKeyDown(grenadeKey);
    }
}
