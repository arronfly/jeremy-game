using UnityEngine;

/// <summary>
/// 摄像机跟随脚本 - 正交相机平滑跟随目标
/// 摄像机被限制在地图边界内，防止显示地图外的区域
/// 坐标系统: 世界范围 x:-800~800, y:-600~600
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The target transform to follow (set by GameBootstrapper)")]
    public Transform target;

    [Tooltip("Smoothing factor (0 = no movement, 1 = instant snap)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.1f;

    [Tooltip("Orthographic camera size")]
    public float cameraSize = 10f;

    [Header("Map Bounds")]
    [Tooltip("Camera clamp bounds (accounting for camera viewport)")]
    public float boundsMinX = -760f;
    public float boundsMaxX = 760f;
    public float boundsMinY = -560f;
    public float boundsMaxY = 560f;

    [Header("Z Position")]
    public float cameraZ = -10f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = cameraSize;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Calculate desired position
        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            cameraZ
        );

        // Smooth interpolation toward target
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed
        );

        // Clamp within map bounds
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, boundsMinX, boundsMaxX);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, boundsMinY, boundsMaxY);
        smoothedPosition.z = cameraZ;

        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Sets the target transform for the camera to follow.
    /// Called by GameBootstrapper or other setup scripts.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // Snap camera to target immediately on first assign
        if (target != null)
        {
            transform.position = new Vector3(
                Mathf.Clamp(target.position.x, boundsMinX, boundsMaxX),
                Mathf.Clamp(target.position.y, boundsMinY, boundsMaxY),
                cameraZ
            );
        }
    }

    /// <summary>
    /// Recalculates bounds based on a given map size.
    /// Subtracts half the camera viewport to prevent showing outside the map.
    /// </summary>
    public void SetMapBounds(float mapHalfWidth, float mapHalfHeight)
    {
        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * cam.aspect;

        boundsMinX = -mapHalfWidth + horizExtent;
        boundsMaxX = mapHalfWidth - horizExtent;
        boundsMinY = -mapHalfHeight + vertExtent;
        boundsMaxY = mapHalfHeight - vertExtent;
    }
}
