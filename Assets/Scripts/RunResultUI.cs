using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RunResultUI : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private PlayerProgressService _progressService;

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
    private Text _scoreText;

    [SerializeField]
    private Text _earnedMoneyText;

    [SerializeField]
    private Text _totalMoneyText;

    [SerializeField]
    private Text _recordsText;

    [FormerlySerializedAs("_retryButton")]
    [SerializeField]
    private Button _continueButton;

    [Header("Display")]
    [SerializeField]
    private string _depthSuffix = " m";

    private bool _isVisible;

    #endregion

    #region Properties

    public bool IsVisible => _isVisible;

    #endregion

    #region Events

    public event Action ContinueRequested;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_progressService != null)
            _progressService.RunSettled += Show;

        if (_continueButton != null)
            _continueButton.onClick.AddListener(HandleContinueClicked);

        Hide();
    }

    private void OnDisable()
    {
        if (_progressService != null)
            _progressService.RunSettled -= Show;

        if (_continueButton != null)
            _continueButton.onClick.RemoveListener(HandleContinueClicked);
    }

    #endregion

    #region Public Methods

    public void Show(ProgressUpdate _update)
    {
        if (_isVisible)
            return;

        _isVisible = true;
        SetTexts(_update);

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

    private void HandleContinueClicked()
    {
        if (!_isVisible)
            return;

        Hide();
        ContinueRequested?.Invoke();
    }

    private void SetTexts(ProgressUpdate _update)
    {
        if (_titleText != null)
            _titleText.text = "RESULTADO DA RUN";

        if (_reasonText != null)
            _reasonText.text = GetReasonText(_update.RunResult.EndReason);
        if (_timeText != null)
            _timeText.text = $"Tempo: {FormatTime(_update.RunResult.Time)}";

        if (_depthText != null)
        {
            _depthText.text = string.Format(
                CultureInfo.CurrentCulture,
                "Profundidade: {0:0.0}{1}",
                _update.RunResult.Depth,
                _depthSuffix
            );
        }

        if (_dugBlocksText != null)
            _dugBlocksText.text = $"Blocos escavados: {_update.RunResult.DugBlocks:N0}";

        if (_scoreText != null)
            _scoreText.text = $"Score: {_update.Score:N0}";

        if (_earnedMoneyText != null)
            _earnedMoneyText.text = $"Recompensa: $ {_update.EarnedMoney:N0}";

        if (_totalMoneyText != null)
            _totalMoneyText.text = $"Dinheiro total: $ {_update.TotalMoney:N0}";

        if (_recordsText != null)
            _recordsText.text = GetRecordText(_update);
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

    private string GetRecordText(ProgressUpdate _update)
    {
        if (_update.IsNewBestDepth && _update.IsNewBestScore)
            return "NOVOS RECORDES: PROFUNDIDADE E SCORE!";

        if (_update.IsNewBestDepth)
            return "NOVO RECORDE DE PROFUNDIDADE!";

        if (_update.IsNewBestScore)
            return "NOVO RECORDE DE SCORE!";

        return string.Empty;
    }

    #endregion
}
