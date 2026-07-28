using UnityEngine;

public enum PostRunScreen
{
    Result,
    Shop
}

public class PostRunFlowController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private RunResultUI _runResultUI;

    [SerializeField]
    private ShopUI _shopUI;

    [SerializeField]
    private RunRestartController _runRestartController;

    private bool _isTransitioning;
    private PostRunScreen _currentScreen = PostRunScreen.Result;

    #endregion

    #region Properties

    public PostRunScreen CurrentScreen => _currentScreen;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_runResultUI != null)
            _runResultUI.ContinueRequested += HandleContinueRequested;

        if (_shopUI != null)
            _shopUI.NewRunRequested += HandleNewRunRequested;
    }

    private void OnDisable()
    {
        if (_runResultUI != null)
            _runResultUI.ContinueRequested -= HandleContinueRequested;

        if (_shopUI != null)
            _shopUI.NewRunRequested -= HandleNewRunRequested;
    }

    #endregion

    #region Public Methods

    public void OpenResult()
    {
        if (_shopUI != null)
            _shopUI.Close();

        _currentScreen = PostRunScreen.Result;
    }

    public void OpenShop()
    {
        if (_isTransitioning || _runManager == null || !_runManager.IsFinished)
            return;

        _isTransitioning = true;

        if (_runResultUI != null)
            _runResultUI.Hide();

        _currentScreen = PostRunScreen.Shop;

        if (_shopUI != null)
            _shopUI.Open();

        _isTransitioning = false;
    }

    #endregion

    #region Private Methods

    private void HandleContinueRequested()
    {
        OpenShop();
    }

    private void HandleNewRunRequested()
    {
        if (_isTransitioning || _runManager == null || !_runManager.IsFinished)
            return;

        _isTransitioning = true;

        if (_shopUI != null)
            _shopUI.Close();

        if (_runRestartController != null)
            _runRestartController.RestartRun();

        _currentScreen = PostRunScreen.Result;
        _isTransitioning = false;
    }

    #endregion
}
