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

    private bool _isMoving;
    private bool _canMoveHorizontal;
    private float _currentFallSpeed;
    private float _currentHorizontalSpeed;
    private float _input;
    private float _launchForce;

    #endregion

    #region Properties

    public float CurrentSpeed => _rigidbody == null ? 0f : _rigidbody.linearVelocity.magnitude;

    public bool IsMoving => _isMoving;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
        }
    }

    private void Update()
    {
        _input = 0f;

        if (_isMoving && _canMoveHorizontal)
        {
            _input = Input.GetAxisRaw("Horizontal");
        }
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

        MoveDown();
        MoveHorizontal();
        ApplyVelocity();
        ApplyCurvedRotation();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _canMoveHorizontal = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _canMoveHorizontal = false;
        }
    }

    #endregion

    #region Public Methods

    public void SetLaunchForce(float launchForce)
    {
        _launchForce = Mathf.Clamp01(launchForce);
    }

    public void StartMovement()
    {
        _isMoving = true;
        _canMoveHorizontal = true;
        _currentFallSpeed = Mathf.Lerp(_baseFallSpeed, _maxFallSpeed, _launchForce);

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = Vector2.down * _currentFallSpeed;
        }
    }

    public void StopMovement()
    {
        _isMoving = false;
        _currentFallSpeed = 0f;
        _currentHorizontalSpeed = 0f;

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    #endregion

    #region Private Methods

    private void MoveDown()
    {
        _currentFallSpeed = Mathf.MoveTowards(
            _currentFallSpeed,
            _maxFallSpeed,
            _acceleration * Time.fixedDeltaTime
        );
    }

    private void MoveHorizontal()
    {
        _currentHorizontalSpeed = Mathf.MoveTowards(
            _currentHorizontalSpeed,
            _input * _horizontalSpeed,
            _horizontalAcceleration * Time.fixedDeltaTime
        );
    }

    private void ApplyVelocity()
    {
        _rigidbody.linearVelocity = new Vector2(_currentHorizontalSpeed, -_currentFallSpeed);
    }

    private void DecelerateToStop()
    {
        _currentFallSpeed = Mathf.MoveTowards(_currentFallSpeed, 0f, _stopDeceleration * Time.fixedDeltaTime);
        _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0f, _stopDeceleration * Time.fixedDeltaTime);
        ApplyVelocity();
        ApplyCurvedRotation();
    }

    private void ApplyCurvedRotation()
    {
        float targetZ = -Mathf.Sign(_currentHorizontalSpeed) * _curveTiltDegrees;

        if (Mathf.Abs(_currentHorizontalSpeed) < 0.05f)
        {
            targetZ = 0f;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            _rotationSmoothing * Time.fixedDeltaTime
        );
    }

    #endregion
}
