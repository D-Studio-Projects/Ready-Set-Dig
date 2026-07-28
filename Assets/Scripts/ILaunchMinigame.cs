public interface ILaunchMinigame
{
    #region Properties

    LauncherBehaviorId BehaviorId { get; }

    float CurrentResult { get; }

    #endregion

    #region Public Methods

    void ResetMinigame();

    void Tick(float _deltaTime);

    bool TryComplete(out float _result);

    #endregion
}
