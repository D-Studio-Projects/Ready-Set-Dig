using UnityEngine;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [FormerlySerializedAs("target")]
    [SerializeField]
    private Transform _target;

    [Header("Follow")]
    [FormerlySerializedAs("smoothSpeed")]
    [SerializeField]
    [Min(0f)]
    private float _verticalSmoothSpeed = 8f;

    [SerializeField]
    [Min(0f)]
    private float _horizontalSmoothSpeed = 3f;

    [SerializeField]
    [Min(0f)]
    private float _horizontalDeadZone = 2f;

    [FormerlySerializedAs("offset")]
    [SerializeField]
    private Vector3 _offset = new Vector3(0f, 0f, -10f);

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 desiredPosition = _target.position + _offset;
        Vector3 cameraPosition = transform.position;
        float horizontalTarget = GetHorizontalTarget(
            cameraPosition.x,
            desiredPosition.x
        );

        cameraPosition.x = Mathf.Lerp(
            cameraPosition.x,
            horizontalTarget,
            GetSmoothingFactor(_horizontalSmoothSpeed)
        );
        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            desiredPosition.y,
            GetSmoothingFactor(_verticalSmoothSpeed)
        );
        cameraPosition.z = desiredPosition.z;
        transform.position = cameraPosition;
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private float GetHorizontalTarget(float _cameraX, float _desiredX)
    {
        float distance = _desiredX - _cameraX;
        float deadZone = Mathf.Max(0f, _horizontalDeadZone);

        if (Mathf.Abs(distance) <= deadZone)
            return _cameraX;

        return _desiredX - Mathf.Sign(distance) * deadZone;
    }

    private float GetSmoothingFactor(float _smoothSpeed)
    {
        float smoothSpeed = Mathf.Max(0f, _smoothSpeed);
        return 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
    }

    #endregion
}
