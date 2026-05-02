using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Movement
    public Vector2 moveInput { get; private set; }
    public bool isSprinting { get; private set; }

    // Actions
    public bool shoot { get; private set; }
    public bool reload { get; private set; }
    public bool switchWeapon1 { get; private set; }
    public bool switchWeapon2 { get; private set; }
    public bool switchWeapon3 { get; private set; }
    public bool switchWeapon4 { get; private set; }
    public bool throwGrenade { get; private set; }
    public bool useMedkit7 { get; private set; }
    public bool useMedkit8 { get; private set; }
    public bool useMedkit9 { get; private set; }

    // Mouse
    public Vector3 mouseWorldPosition { get; private set; }

    // Action buffering (in seconds)
    private const float BUFFER_TIME = 0.15f;
    private float _reloadBuffer;
    private float _grenadeBuffer;
    private float _switchWeaponBuffer;

    // Input smoothing
    private Vector2 _smoothMoveInput;
    private const float MOVE_SMOOTH_SPEED = 10f;

    // Buffered action states (consumed once per press)
    private bool _reloadBuffered;
    private bool _grenadeBuffered;
    private bool _switchWeaponBuffered;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        UpdateMovement();
        UpdateActions();
        UpdateBufferedActions();
        UpdateMousePosition();
    }

    private void UpdateMovement()
    {
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _smoothMoveInput = Vector2.Lerp(_smoothMoveInput, rawInput, Time.deltaTime * MOVE_SMOOTH_SPEED);
        moveInput = _smoothMoveInput.normalized * rawInput.magnitude;
        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    private void UpdateActions()
    {
        // Continuous actions
        shoot = Input.GetMouseButton(0);

        // Single-frame actions
        reload = Input.GetKeyDown(KeyCode.R);
        switchWeapon1 = Input.GetKeyDown(KeyCode.Alpha1);
        switchWeapon2 = Input.GetKeyDown(KeyCode.Alpha2);
        switchWeapon3 = Input.GetKeyDown(KeyCode.Alpha3);
        switchWeapon4 = Input.GetKeyDown(KeyCode.Alpha4);
        throwGrenade = Input.GetKeyDown(KeyCode.G);
        useMedkit7 = Input.GetKeyDown(KeyCode.Alpha7);
        useMedkit8 = Input.GetKeyDown(KeyCode.Alpha8);
        useMedkit9 = Input.GetKeyDown(KeyCode.Alpha9);
    }

    private void UpdateBufferedActions()
    {
        // Decay buffers over time
        _reloadBuffer = Mathf.Max(0, _reloadBuffer - Time.deltaTime);
        _grenadeBuffer = Mathf.Max(0, _grenadeBuffer - Time.deltaTime);
        _switchWeaponBuffer = Mathf.Max(0, _switchWeaponBuffer - Time.deltaTime);

        // Buffer single-frame actions
        if (Input.GetKeyDown(KeyCode.R))
            _reloadBuffer = BUFFER_TIME;
        if (Input.GetKeyDown(KeyCode.G))
            _grenadeBuffer = BUFFER_TIME;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
            _switchWeaponBuffer = BUFFER_TIME;

        // Consume buffers (only once per press)
        _reloadBuffered = _reloadBuffer > 0 && !_reloadBuffered;
        _grenadeBuffered = _grenadeBuffer > 0 && !_grenadeBuffered;
        _switchWeaponBuffered = _switchWeaponBuffer > 0 && !_switchWeaponBuffered;
    }

    private void UpdateMousePosition()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // Helper methods for buffered actions
    public bool GetReloadBuffered()
    {
        if (_reloadBuffered)
        {
            _reloadBuffered = false;
            return true;
        }
        return false;
    }

    public bool GetGrenadeBuffered()
    {
        if (_grenadeBuffered)
        {
            _grenadeBuffered = false;
            return true;
        }
        return false;
    }

    public bool GetSwitchWeaponBuffered()
    {
        if (_switchWeaponBuffered)
        {
            _switchWeaponBuffered = false;
            return true;
        }
        return false;
    }

    // Check if buffer is active without consuming
    public bool IsReloadBuffered() => _reloadBuffer > 0;
    public bool IsGrenadeBuffered() => _grenadeBuffer > 0;
    public bool IsSwitchWeaponBuffered() => _switchWeaponBuffer > 0;

    // Input smoothing helper
    public Vector2 GetSmoothMoveInput(float smoothSpeed)
    {
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _smoothMoveInput = Vector2.Lerp(_smoothMoveInput, rawInput, Time.deltaTime * smoothSpeed);
        return _smoothMoveInput.normalized * rawInput.magnitude;
    }
}
