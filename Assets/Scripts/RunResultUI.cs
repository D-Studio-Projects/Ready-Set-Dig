using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class RunResultUI : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private Text _titleText;

    [SerializeField]
    private Text _reasonText;

    [SerializeField]
    private Text _timeText;

    [SerializeField]
    private Text _depthText;

    [SerializeField]
    private Text _dugBlocksText;

    [SerializeField]
    private Button _retryButton;

    [Header("Display")]
    [SerializeField]
    private string _depthSuffix = " m";

    private bool _isVisible;

    #endregion

    #region Properties

    public bool IsVisible => _isVisible;

    #endregion

    #region Events

    public event Action RetryRequested;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_runManager != null)
            _runManager.RunFinished += Show;

        if (_retryButton != null)
            _retryButton.onClick.AddListener(HandleRetryClicked);

        Hide();
    }

    private void OnDisable()
    {
        if (_runManager != null)
            _runManager.RunFinished -= Show;

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(HandleRetryClicked);
    }

    #endregion

    #region Public Methods

    public void Show(RunResult _result)
    {
        if (_isVisible)
            return;

        _isVisible = true;
        SetTexts(_result);

        if (_panelRoot != null)
            _panelRoot.SetActive(true);
    }

    public void Hide()
    {
        _isVisible = false;

        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    #endregion

    #region Private Methods

    private void HandleRetryClicked()
    {
        if (!_isVisible)
            return;

        Hide();
        RetryRequested?.Invoke();
    }

    private void SetTexts(RunResult _result)
    {
        if (_titleText != null)
            _titleText.text = "GAME OVER";

        if (_reasonText != null)
            _reasonText.text = GetReasonText(_result.EndReason);

        if (_timeText != null)
            _timeText.text = $"Tempo: {FormatTime(_result.Time)}";

        if (_depthText != null)
        {
            _depthText.text = string.Format(
                CultureInfo.CurrentCulture,
                "Profundidade: {0:0.0}{1}",
                _result.Depth,
                _depthSuffix
            );
        }

        if (_dugBlocksText != null)
            _dugBlocksText.text = $"Blocos escavados: {_result.DugBlocks:N0}";
    }

    private string FormatTime(float _seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, _seconds));
        return time.ToString(@"mm\:ss");
    }

    private string GetReasonText(RunEndReason _reason)
    {
        switch (_reason)
        {
            case RunEndReason.EnergyDepleted:
                return "Energia esgotada";
            case RunEndReason.ReachedCore:
                return "Nucleo alcancado";
            case RunEndReason.ManualRestart:
                return "Run reiniciada";
            default:
                return string.Empty;
        }
    }

    #endregion
}

