using UnityEngine;
using UnityEngine.UI;

public class PowerBarLaunchMinigame : MonoBehaviour, ILaunchMinigame
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private Slider _chargeBar;

    [Header("Charge")]
    [SerializeField]
    private float _chargeSpeed = 2f;

    [SerializeField]
    private bool _hideChargeBarOnLaunch = true;

    private float _charge;
    private float _chargeDirection = 1f;

    #endregion

    #region Properties

    public LauncherBehaviorId BehaviorId => LauncherBehaviorId.PowerBar;

    public float CurrentResult => Mathf.Clamp01(_charge);

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public void ResetMinigame()
    {
        _charge = 0f;
        _chargeDirection = 1f;

        if (_chargeBar == null)
            return;

        _chargeBar.value = _charge;
        _chargeBar.gameObject.SetActive(true);
    }

    public void Tick(float _deltaTime)
    {
        float chargeSpeed = Mathf.Max(.01f, _chargeSpeed);
        _charge += _chargeDirection * chargeSpeed * Mathf.Max(0f, _deltaTime);

        if (_charge >= 1f)
        {
            _charge = 1f;
            _chargeDirection = -1f;
        }
        else if (_charge <= 0f)
        {
            _charge = 0f;
            _chargeDirection = 1f;
        }

        if (_chargeBar != null)
            _chargeBar.value = _charge;
    }

    public bool TryComplete(out float _result)
    {
        _result = CurrentResult;

        if (!Input.GetKeyDown(KeyCode.Space))
            return false;

        if (_hideChargeBarOnLaunch && _chargeBar != null)
            _chargeBar.gameObject.SetActive(false);

        return true;
    }

    #endregion

    #region Private Methods

    #endregion
}
