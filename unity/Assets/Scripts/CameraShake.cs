using UnityEngine;
using System.Collections;

/// <summary>
/// 摄像机震动效果 - 配合 CameraFollow 使用
/// 多次震动调用会叠加（取较强者）
/// </summary>
[RequireComponent(typeof(CameraFollow))]
public class CameraShake : MonoBehaviour
{
    [Header("震动参数")]
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.2f;

    private CameraFollow cameraFollow;
    private Coroutine shakeCoroutine;

    // 当前正在执行的震动参数 (用于叠加比较)
    private float currentIntensity = 0f;
    private float currentDuration = 0f;
    private float currentElapsed = 0f;

    private void Awake()
    {
        cameraFollow = GetComponent<CameraFollow>();
    }

    /// <summary>
    /// 触发摄像机震动
    /// 传入 0 使用默认值；多次调用取较强的震动 (更大强度或更长剩余时间)
    /// </summary>
    /// <param name="intensity">震动强度(像素偏移量)，0 使用默认值</param>
    /// <param name="duration">震动持续时间(秒)，0 使用默认值</param>
    public void Shake(float intensity = 0, float duration = 0)
    {
        if (intensity <= 0) intensity = shakeIntensity;
        if (duration <= 0) duration = shakeDuration;

        // 叠加逻辑: 取更强的震动
        // 如果新震动强度更大，或当前震动即将结束，则替换
        bool shouldReplace = false;

        if (shakeCoroutine == null)
        {
            // 没有正在运行的震动，直接启动
            shouldReplace = true;
        }
        else if (intensity > currentIntensity)
        {
            // 新震动强度更强，替换
            shouldReplace = true;
        }
        else
        {
            // 计算当前震动的剩余强度
            float remainingRatio = 1f - (currentElapsed / currentDuration);
            float remainingIntensity = currentIntensity * remainingRatio;

            if (intensity > remainingIntensity)
            {
                shouldReplace = true;
            }
        }

        if (shouldReplace)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            currentIntensity = intensity;
            currentDuration = duration;
            currentElapsed = 0f;
            shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
        }
    }

    /// <summary>
    /// 震动协程: 在持续时间内随机偏移摄像机位置，然后平滑恢复
    /// </summary>
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            currentElapsed = elapsed;

            // 计算当前帧的震动衰减系数 (从 1 衰减到 0)
            float progress = elapsed / duration;
            float decay = 1f - progress;

            // 生成随机偏移
            float offsetX = Random.Range(-intensity, intensity) * decay;
            float offsetY = Random.Range(-intensity, intensity) * decay;

            // 在 CameraFollow 的目标位置基础上施加偏移
            // 直接偏移 transform 位置，CameraFollow 的 LateUpdate 会平滑回位
            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0f);
            transform.position += shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 震动结束，重置状态
        currentIntensity = 0f;
        currentDuration = 0f;
        currentElapsed = 0f;
        shakeCoroutine = null;
    }

    /// <summary>
    /// 立即停止当前震动
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        currentIntensity = 0f;
        currentDuration = 0f;
        currentElapsed = 0f;
    }
}
