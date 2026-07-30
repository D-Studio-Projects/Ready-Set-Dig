using UnityEngine;
using UnityEngine.UI;

public class ResistantRockQteUI : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private Slider _progressBar;

    [SerializeField]
    private Slider _timeBar;

    [SerializeField]
    private Text _promptText;

    [SerializeField]
    private Text _progressText;

    [SerializeField]
    private Text _resultText;

    [Header("Text")]
    [SerializeField]
    private string _prompt = "APERTE ESPACO!";

    [SerializeField]
    private string _successMessage = "ROCHA QUEBRADA!";

    [SerializeField]
    private string _failureMessage = "VOCE PERDEU VELOCIDADE!";

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Hide();
    }

    #endregion

    #region Public Methods

    public void Show(int _requiredPresses, float _duration)
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        if (_promptText != null)
            _promptText.text = _prompt;

        if (_resultText != null)
            _resultText.text = string.Empty;

        if (_progressBar != null)
        {
            _progressBar.minValue = 0f;
            _progressBar.maxValue = Mathf.Max(1, _requiredPresses);
            _progressBar.value = 0f;
        }

        if (_timeBar != null)
        {
            _timeBar.minValue = 0f;
            _timeBar.maxValue = Mathf.Max(.01f, _duration);
            _timeBar.value = _timeBar.maxValue;
        }

        SetProgress(0, _requiredPresses, _duration, _duration);
    }

    public void SetProgress(
        int _currentPresses,
        int _requiredPresses,
        float _remainingTime,
        float _duration)
    {
        int requiredPresses = Mathf.Max(1, _requiredPresses);
        int currentPresses = Mathf.Clamp(_currentPresses, 0, requiredPresses);

        if (_progressBar != null)
            _progressBar.value = currentPresses;

        if (_timeBar != null)
        {
            _timeBar.maxValue = Mathf.Max(.01f, _duration);
            _timeBar.value = Mathf.Clamp(_remainingTime, 0f, _timeBar.maxValue);
        }

        if (_progressText != null)
            _progressText.text = $"{currentPresses}/{requiredPresses}";
    }

    public void ShowResult(bool _success)
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        if (_promptText != null)
            _promptText.text = string.Empty;

        if (_resultText != null)
            _resultText.text = _success ? _successMessage : _failureMessage;
    }

    public void Hide()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    #endregion
}
