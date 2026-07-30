using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private Rigidbody2D _rigidbody;

    [Header("Vertical Movement")]
    [SerializeField]
    private float _baseFallSpeed = 6f;

    [SerializeField]
    private float _maxFallSpeed = 14f;

    [SerializeField]
    private float _acceleration = 4f;

    [SerializeField]
    private float _stopDeceleration = 18f;

    [Header("Curved Horizontal Movement")]
    [SerializeField]
    private float _horizontalSpeed = 5f;

    [SerializeField]
    private float _horizontalAcceleration = 10f;

    [SerializeField]
    private float _curveTiltDegrees = 20f;

    [SerializeField]
    private float _rotationSmoothing = 10f;

    [Header("Dash")]
    [SerializeField]
    [Min(0)]
    private int _baseDashCount = 1;

    [SerializeField]
    [Min(1f)]
    private float _dashSpeedMultiplier = 1.5f;

    [SerializeField]
    [Min(.01f)]
    private float _dashDuration = .4f;

    private bool _isMoving;
    private bool _canMoveHorizontal;
    private bool _isDashing;
    private float _currentFallSpeed;
    private float _currentHorizontalSpeed;
    private float _input;
    private float _launchForce;
    private float _lastTrackedY;
    private float _speedLimitMultiplier = 1f;
    private float _steeringSpeedMultiplier = 1f;
    private float _dashTimeRemaining;
    private int _maximumDashCount;
    private int _remainingDashCount;
    private int _movementStartedFrame = -1;

    #endregion

    #region Properties

    public float CurrentSpeed => _rigidbody == null ? 0f : _rigidbody.linearVelocity.magnitude;

    public bool IsMoving => _isMoving;

    public bool IsDashing => _isDashing;

    public int MaximumDashCount => _maximumDashCount;

    public int RemainingDashCount => _remainingDashCount;

    public float MaximumFallSpeed => GetMaximumFallSpeed();

    #endregion

    #region Events

    public event Action<float> DistanceMovedDown;
    public event Action DashStarted;
    public event Action DashEnded;
    public event Action<int, int> DashCountChanged;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _maximumDashCount = Mathf.Max(0, _baseDashCount);
    }

    private void Start()
    {
        _lastTrackedY = transform.position.y;
        ResetDashCharges();

        if (_rigidbody != null)
            _rigidbody.gravityScale = 0f;
    }

    private void Update()
    {
        _input = 0f;

        if (_isMoving && _canMoveHorizontal)
            _input = Input.GetAxisRaw("Horizontal");

        if (_isMoving &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            TryDash();
        }

        TrackDownwardDistance();
    }

    private void FixedUpdate()
    {
        if (_rigidbody == null)
            return;

        if (!_isMoving)
        {
            DecelerateToStop();
            return;
        }

        UpdateDashTimer();
        MoveDown();
        MoveHorizontal();
        ApplyVelocity();
        ApplyCurvedRotation();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
            _canMoveHorizontal = true;
    }

    private void OnTriggerExit2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
            _canMoveHorizontal = false;
    }

    #endregion

    #region Public Methods

    public void SetLaunchForce(float _launchForceValue)
    {
        _launchForce = Mathf.Clamp01(_launchForceValue);
    }

    public void ApplyGlobalUpgrades(
        float _speedLimitMultiplier,
        float _steeringSpeedMultiplier,
        int _additionalDashCount)
    {
        this._speedLimitMultiplier = SanitizeMultiplier(_speedLimitMultiplier);
        this._steeringSpeedMultiplier = SanitizeMultiplier(_steeringSpeedMultiplier);
        _maximumDashCount = Mathf.Max(0, _baseDashCount + Mathf.Max(0, _additionalDashCount));
        ResetDashCharges();
    }

    public void StartMovement()
    {
        _isMoving = true;
        _canMoveHorizontal = true;
        _movementStartedFrame = Time.frameCount;
        _lastTrackedY = transform.position.y;
        _currentFallSpeed = Mathf.Lerp(
            _baseFallSpeed,
            GetMaximumFallSpeed(),
            _launchForce
        );
        ResetDashCharges();

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.down * _currentFallSpeed;
        }
    }

    public void StopMovement()
    {
        _isMoving = false;
        _canMoveHorizontal = false;
        EndDash();
        _currentFallSpeed = 0f;
        _currentHorizontalSpeed = 0f;
        _lastTrackedY = transform.position.y;

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }

    public void ResetMovement(Vector3 _position, Quaternion _rotation)
    {
        _isMoving = false;
        _canMoveHorizontal = false;
        _currentFallSpeed = 0f;
        _currentHorizontalSpeed = 0f;
        _input = 0f;
        _launchForce = 0f;
        _movementStartedFrame = -1;
        _lastTrackedY = _position.y;
        EndDash();
        ResetDashCharges();
        transform.SetPositionAndRotation(_position, _rotation);

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }

    public bool TryDash()
    {
        if (!_isMoving ||
            _isDashing ||
            _remainingDashCount <= 0 ||
            Time.frameCount <= _movementStartedFrame)
        {
            return false;
        }

        _remainingDashCount--;
        _isDashing = true;
        _dashTimeRemaining = Mathf.Max(.01f, _dashDuration);
        _currentFallSpeed = Mathf.Max(
            _currentFallSpeed,
            GetMaximumFallSpeed() * Mathf.Max(1f, _dashSpeedMultiplier)
        );

        DashCountChanged?.Invoke(_remainingDashCount, _maximumDashCount);
        DashStarted?.Invoke();
        return true;
    }

    #endregion

    #region Private Methods

    private void MoveDown()
    {
        float targetSpeed = GetMaximumFallSpeed();

        if (_isDashing)
            targetSpeed *= Mathf.Max(1f, _dashSpeedMultiplier);

        float speedChange = _currentFallSpeed > targetSpeed
            ? _stopDeceleration
            : _acceleration;
        _currentFallSpeed = Mathf.MoveTowards(
            _currentFallSpeed,
            targetSpeed,
            speedChange * UnityEngine.Time.fixedDeltaTime
        );
    }

    private void MoveHorizontal()
    {
        _currentHorizontalSpeed = Mathf.MoveTowards(
            _currentHorizontalSpeed,
            _input * _horizontalSpeed * _steeringSpeedMultiplier,
            _horizontalAcceleration *
            _steeringSpeedMultiplier *
            UnityEngine.Time.fixedDeltaTime
        );
    }

    private void ApplyVelocity()
    {
        _rigidbody.linearVelocity = new Vector2(_currentHorizontalSpeed, -_currentFallSpeed);
    }

    private void DecelerateToStop()
    {
        _currentFallSpeed = Mathf.MoveTowards(
            _currentFallSpeed,
            0f,
            _stopDeceleration * UnityEngine.Time.fixedDeltaTime
        );
        _currentHorizontalSpeed = Mathf.MoveTowards(
            _currentHorizontalSpeed,
            0f,
            _stopDeceleration * UnityEngine.Time.fixedDeltaTime
        );
        ApplyVelocity();
        ApplyCurvedRotation();
    }

    private void ApplyCurvedRotation()
    {
        float targetZ = -Mathf.Sign(_currentHorizontalSpeed) * _curveTiltDegrees;

        if (Mathf.Abs(_currentHorizontalSpeed) < .05f)
            targetZ = 0f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            _rotationSmoothing *
            _steeringSpeedMultiplier *
            UnityEngine.Time.fixedDeltaTime
        );
    }

    private void UpdateDashTimer()
    {
        if (!_isDashing)
            return;

        _dashTimeRemaining -= UnityEngine.Time.fixedDeltaTime;

        if (_dashTimeRemaining <= 0f)
            EndDash();
    }

    private void EndDash()
    {
        if (!_isDashing)
        {
            _dashTimeRemaining = 0f;
            return;
        }

        _isDashing = false;
        _dashTimeRemaining = 0f;
        DashEnded?.Invoke();
    }

    private void ResetDashCharges()
    {
        _maximumDashCount = Mathf.Max(0, _maximumDashCount);
        _remainingDashCount = _maximumDashCount;
        DashCountChanged?.Invoke(_remainingDashCount, _maximumDashCount);
    }

    private float GetMaximumFallSpeed()
    {
        return Mathf.Max(0f, _maxFallSpeed) * _speedLimitMultiplier;
    }

    private float SanitizeMultiplier(float _multiplier)
    {
        if (float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
            return 1f;

        return Mathf.Max(1f, _multiplier);
    }

    private void TrackDownwardDistance()
    {
        float currentY = transform.position.y;

        if (_isMoving)
        {
            float distanceDown = Mathf.Max(0f, _lastTrackedY - currentY);

            if (distanceDown > 0f)
                DistanceMovedDown?.Invoke(distanceDown);
        }

        _lastTrackedY = currentY;
    }

    #endregion
}
