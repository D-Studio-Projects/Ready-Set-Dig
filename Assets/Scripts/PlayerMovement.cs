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
    private float _horizontalSpeed = 7f;

    [SerializeField]
    private float _horizontalAcceleration = 24f;

    [SerializeField]
    private float _curveTiltDegrees = 22f;

    [SerializeField]
    private float _rotationSmoothing = 12f;

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
    private bool _isDashing;
    private bool _isPausedByObstacle;
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

    public bool IsPausedByObstacle => _isPausedByObstacle;

    public int MaximumDashCount => _maximumDashCount;

    public int RemainingDashCount => _remainingDashCount;

    public float MaximumFallSpeed => GetMaximumFallSpeed();

    #endregion

    #region Events

    public event Action<float> DistanceMovedDown;
    public event Action DashStarted;
    public event Action DashEnded;
    public event Action<int, int> DashCountChanged;
    public event Action<bool> ObstaclePauseChanged;
    public event Action<float> SpeedPenaltyApplied;

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

        if (_isMoving && !_isPausedByObstacle)
            _input = Input.GetAxisRaw("Horizontal");

        if (_isMoving &&
            !_isPausedByObstacle &&
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

        if (_isPausedByObstacle)
        {
            HoldPositionForObstacle();
            return;
        }

        UpdateDashTimer();
        MoveDown();
        MoveHorizontal();
        ApplyVelocity();
        ApplyCurvedRotation();
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
        SetObstaclePause(false);
        _isMoving = true;
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
        SetObstaclePause(false);
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
        _currentFallSpeed = 0f;
        _currentHorizontalSpeed = 0f;
        _input = 0f;
        _launchForce = 0f;
        _movementStartedFrame = -1;
        _lastTrackedY = _position.y;
        SetObstaclePause(false);
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
            _isPausedByObstacle ||
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

    public bool PauseForObstacle()
    {
        if (!_isMoving || _isPausedByObstacle)
            return false;

        EndDash();
        _input = 0f;
        _currentHorizontalSpeed = 0f;
        SetObstaclePause(true);
        HoldPositionForObstacle();
        return true;
    }

    public bool ResumeFromObstacle()
    {
        if (!_isMoving || !_isPausedByObstacle)
            return false;

        SetObstaclePause(false);
        _lastTrackedY = transform.position.y;

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.down * _currentFallSpeed;
        }

        return true;
    }

    public void ApplySpeedPenalty(float _remainingSpeedMultiplier)
    {
        float remainingSpeedMultiplier = float.IsNaN(_remainingSpeedMultiplier) ||
                                         float.IsInfinity(_remainingSpeedMultiplier)
            ? 1f
            : Mathf.Clamp01(_remainingSpeedMultiplier);
        _currentFallSpeed = Mathf.Max(
            0f,
            _currentFallSpeed * remainingSpeedMultiplier
        );

        if (_rigidbody != null && _isMoving && !_isPausedByObstacle)
        {
            _rigidbody.linearVelocity = new Vector2(
                _currentHorizontalSpeed,
                -_currentFallSpeed
            );
        }

        SpeedPenaltyApplied?.Invoke(remainingSpeedMultiplier);
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

    private void HoldPositionForObstacle()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.gravityScale = 0f;
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
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
        float maximumHorizontalSpeed = Mathf.Max(
            .01f,
            _horizontalSpeed * _steeringSpeedMultiplier
        );
        float steeringAmount = Mathf.Clamp(
            _currentHorizontalSpeed / maximumHorizontalSpeed,
            -1f,
            1f
        );
        float targetZ = -steeringAmount * _curveTiltDegrees;
        float smoothing = Mathf.Max(
            0f,
            _rotationSmoothing * _steeringSpeedMultiplier
        );
        float rotationProgress = 1f - Mathf.Exp(
            -smoothing * UnityEngine.Time.fixedDeltaTime
        );
        float nextRotation = Mathf.LerpAngle(
            _rigidbody.rotation,
            targetZ,
            rotationProgress
        );
        _rigidbody.MoveRotation(nextRotation);
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

    private void SetObstaclePause(bool _isPaused)
    {
        if (_isPausedByObstacle == _isPaused)
            return;

        _isPausedByObstacle = _isPaused;
        ObstaclePauseChanged?.Invoke(_isPausedByObstacle);
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
